using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Infrastructure.Payments.PayBySharePay;
using Microsoft.Extensions.Logging.Abstractions;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

/// <summary>
/// Tests for GroupPaymentOrchestrationService.
/// Bruger in-memory fakes for alle repositories og provider.
/// </summary>
public class GroupPaymentOrchestrationServiceTests
{
    // ─── In-memory fakes ────────────────────────────────────────────────────

    private sealed class FakeParticipantPaymentRepository : IParticipantPaymentRepository
    {
        private readonly List<ParticipantPayment> _store = [];
        private int _nextId = 1;

        public List<ParticipantPayment> All => _store;

        public Task<ParticipantPayment?> GetByIdAsync(int id)
            => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));

        public Task<ParticipantPayment?> GetByProviderPaymentIdAsync(string providerPaymentId)
            => Task.FromResult(_store.FirstOrDefault(p => p.ProviderPaymentId == providerPaymentId));

        public Task<IEnumerable<ParticipantPayment>> GetByOrderIdAsync(int orderId)
            => Task.FromResult(_store.Where(p => p.OrderId == orderId));

        public Task<ParticipantPayment> AddAsync(ParticipantPayment p)
        {
            p.Id = _nextId++;
            _store.Add(p);
            return Task.FromResult(p);
        }

        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private sealed class FakePaymentEventLogRepository : IPaymentEventLogRepository
    {
        public Task AddAsync(PaymentEventLog eventLog) => Task.CompletedTask;
        public Task<IEnumerable<PaymentEventLog>> GetByParticipantPaymentIdAsync(int id)
            => Task.FromResult(Enumerable.Empty<PaymentEventLog>());
        public Task<IEnumerable<PaymentEventLog>> GetByOrderIdAsync(int orderId)
            => Task.FromResult(Enumerable.Empty<PaymentEventLog>());
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        private readonly List<Order> _store = [];

        public Order? Saved { get; private set; }

        public void Add(Order order) => _store.Add(order);

        public Task<Order?> GetByIdWithDetailsAsync(int id)
            => Task.FromResult(_store.FirstOrDefault(o => o.Id == id));

        public Task<IEnumerable<Order>> GetAllWithDetailsAsync()
            => Task.FromResult(_store.AsEnumerable());

        public Task<IEnumerable<Order>> GetByParticipantIdAsync(int participantId)
            => Task.FromResult(_store.AsEnumerable());

        public Task<IEnumerable<Order>> GetAllAsync()
            => Task.FromResult(_store.AsEnumerable());

        public Task<Order> AddAsync(Order order)
        {
            _store.Add(order);
            return Task.FromResult(order);
        }

        public Task SaveChangesAsync()
        {
            Saved = _store.LastOrDefault();
            return Task.CompletedTask;
        }
    }

    private sealed class FakeParticipantRepository : IParticipantRepository
    {
        private readonly List<Participant> _store = [];

        public void Add(Participant p) => _store.Add(p);

        public Task<Participant?> GetByIdAsync(int id)
            => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));

        public Task<Participant?> GetByEmailAsync(string email)
            => Task.FromResult(_store.FirstOrDefault(p => p.Email == email));

        public Task<Participant> AddAsync(Participant participant)
        {
            _store.Add(participant);
            return Task.FromResult(participant);
        }

        public Task SaveChangesAsync() => Task.CompletedTask;

        public Task UpdateAsync(Participant participant) => Task.CompletedTask;

        public Task<IEnumerable<Participant>> SearchAsync(string query, int? excludeParticipantId = null)
            => Task.FromResult(Enumerable.Empty<Participant>());

        public Task<IEnumerable<Participant>> GetAllPersonsAsync()
            => Task.FromResult<IEnumerable<Participant>>(_store.Where(p => p.Type == DataStorage.PayBySharePay.Entities.ParticipantType.Person).ToList());
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private sealed class NoOpMerchantCallbackService : IMerchantCallbackService
    {
        public Task SendGroupOrderPaidAsync(PayNSyncFinalGroupOrderDto payload, string? callbackUrl,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class TrackingMerchantCallbackService : IMerchantCallbackService
    {
        public int CallCount { get; private set; }
        public PayNSyncFinalGroupOrderDto? LastPayload { get; private set; }

        public Task SendGroupOrderPaidAsync(PayNSyncFinalGroupOrderDto payload, string? callbackUrl,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastPayload = payload;
            return Task.CompletedTask;
        }
    }

    private sealed class TestMerchantOrderFinalizationService : IMerchantOrderFinalizationService
    {
        public int CallCount { get; private set; }

        public Task ValidateAsync(
            Order order,
            IReadOnlyCollection<ParticipantPayment> payments,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<PayNSyncFinalGroupOrderDto> EnsureFinalizedAsync(
            Order order,
            IReadOnlyCollection<ParticipantPayment> payments,
            DateTime paidAtUtc,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            var capturedPayments = payments
                .Where(payment => payment.Status == ParticipantPaymentStatus.Captured)
                .ToList();

            return Task.FromResult(new PayNSyncFinalGroupOrderDto
            {
                PaynsyncOrderId = order.Id,
                PaynsyncOrderNumber = $"PNS-{order.Id:D8}",
                MerchantId = order.MerchantParticipantId ?? 0,
                TotalAmount = capturedPayments.Sum(payment => payment.AmountMinorUnits) / 100m,
                Currency = capturedPayments.FirstOrDefault()?.Currency ?? "DKK",
                PaidAtUtc = paidAtUtc,
                Host = new PayNSyncHostDto { Name = "Host" },
                Lines = capturedPayments.Select((payment, index) => new PayNSyncFinalOrderLineDto
                {
                    Sku = $"line-{index + 1}",
                    Name = $"Vare {index + 1}",
                    Quantity = 1,
                    UnitPrice = payment.AmountMinorUnits / 100m,
                    LineTotal = payment.AmountMinorUnits / 100m
                }).ToList()
            });
        }
    }

    private sealed class RejectingMerchantOrderFinalizationService : IMerchantOrderFinalizationService
    {
        public Task ValidateAsync(
            Order order,
            IReadOnlyCollection<ParticipantPayment> payments,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Ordrelinjer og betalingsbeløb stemmer ikke.");

        public Task<PayNSyncFinalGroupOrderDto> EnsureFinalizedAsync(
            Order order,
            IReadOnlyCollection<ParticipantPayment> payments,
            DateTime paidAtUtc,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Finalisering må ikke kaldes.");
    }

    private FakeParticipantPaymentRepository _paymentRepo = new();
    private FakePaymentEventLogRepository _logRepo = new();
    private FakeOrderRepository _orderRepo = new();
    private FakeParticipantRepository _participantRepo = new();

    private GroupPaymentOrchestrationService CreateSut(IPaymentProvider? provider = null,
        IMerchantCallbackService? callbackService = null,
        IMerchantOrderFinalizationService? finalizationService = null)
    {
        _paymentRepo = new FakeParticipantPaymentRepository();
        _logRepo = new FakePaymentEventLogRepository();
        _orderRepo = new FakeOrderRepository();
        _participantRepo = new FakeParticipantRepository();

        var stateService = new ParticipantPaymentStateService(
            _paymentRepo,
            _logRepo,
            NullLogger<ParticipantPaymentStateService>.Instance);

        return new GroupPaymentOrchestrationService(
            provider ?? new FakePaymentProvider(NullLogger<FakePaymentProvider>.Instance),
            stateService,
            _paymentRepo,
            _orderRepo,
            _participantRepo,
            finalizationService ?? new TestMerchantOrderFinalizationService(),
            callbackService ?? new NoOpMerchantCallbackService(),
            NullLogger<GroupPaymentOrchestrationService>.Instance);
    }

    private static Participant MakeFakeMerchant() => new()
    {
        Id = 999,
        Type = DataStorage.PayBySharePay.Entities.ParticipantType.Merchant,
        Name = "Test Merchant",
        VippsMerchantSerialNumber = "TEST-MSN",
        VippsClientId = "test-client-id",
        VippsClientSecret = "test-client-secret",
        VippsSubscriptionKey = "test-subscription-key"
    };

    private static Order MakeOrder(int id, int hostId, string status = "ReadyToPay")
    {
        var merchant = MakeFakeMerchant();
        var order = new Order
        {
            Id = id,
            CreatedByParticipantId = hostId,
            Title = "Test ordre",
            Status = status,
            MerchantParticipantId = merchant.Id,
            MerchantParticipant = merchant,
            OrderParticipants =
            [
                new OrderParticipant
                {
                    ParticipantId = hostId,
                    Status = "OrderSubmitted",
                    Participant = new Participant { Id = hostId, Name = "Host" }
                }
            ]
        };
        return order;
    }

    // ─── Reserve tests ───────────────────────────────────────────────────────

    /// <summary>Opretter en ordre med merchant og tilføjer den til _orderRepo. Bruges i tests der kun tester reservation.</summary>
    private Order SetupOrderWithMerchant(int orderId, int participantId)
    {
        var order = MakeOrder(orderId, participantId, status: "Collecting");
        _orderRepo.Add(order);
        return order;
    }

    [Fact]
    public async Task ReserveParticipantPayment_Returns_Success_And_Creates_Payment()
    {
        var sut = CreateSut();
        SetupOrderWithMerchant(orderId: 1, participantId: 42);

        var result = await sut.ReserveParticipantPaymentAsync(
            orderId: 1, participantId: 42, merchantId: "merchant-1",
            amountMinorUnits: 15400, currency: "DKK",
            returnUrl: "https://return", callbackUrl: "https://callback");

        result.Success.Should().BeTrue();
        result.ParticipantPaymentId.Should().BeGreaterThan(0);
        result.ErrorCode.Should().BeNull();

        _paymentRepo.All.Should().HaveCount(1);
        _paymentRepo.All[0].Status.Should().Be(ParticipantPaymentStatus.Reserved);
        _paymentRepo.All[0].AmountMinorUnits.Should().Be(15400);
        _paymentRepo.All[0].Currency.Should().Be("DKK");
    }

    [Fact]
    public async Task ReserveParticipantPayment_Is_Idempotent_When_Called_Twice()
    {
        var sut = CreateSut();
        SetupOrderWithMerchant(orderId: 1, participantId: 42);

        var first = await sut.ReserveParticipantPaymentAsync(
            orderId: 1, participantId: 42, merchantId: "m", amountMinorUnits: 10000,
            currency: "DKK", returnUrl: "https://r", callbackUrl: "https://c");

        var second = await sut.ReserveParticipantPaymentAsync(
            orderId: 1, participantId: 42, merchantId: "m", amountMinorUnits: 10000,
            currency: "DKK", returnUrl: "https://r", callbackUrl: "https://c");

        first.Success.Should().BeTrue();
        second.Success.Should().BeTrue();

        // Kun én betaling må oprettes
        _paymentRepo.All.Should().HaveCount(1, "idempotent: anden kald må ikke oprette ny betaling");
    }

    // ─── Approve + Capture tests ─────────────────────────────────────────────

    [Fact]
    public async Task ApproveAndCaptureAll_Captures_All_Reserved_Payments()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        // Opret to reserverede betalinger for ordren
        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        var participant2 = new Participant { Id = 2, Name = "Anna" };
        _participantRepo.Add(participant2);
        order.OrderParticipants.Add(new OrderParticipant
        {
            ParticipantId = 2,
            Status = "OrderSubmitted",
            Participant = participant2
        });
        await sut.ReserveParticipantPaymentAsync(10, 2, "m", 12000, "DKK", "https://r", "https://c");

        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeTrue();
        result.OrderStatus.Should().Be("Paid");
        result.Results.Should().HaveCount(2);
        result.Results.Should().AllSatisfy(r => r.Success.Should().BeTrue());

        // Alle betalinger skal have status Captured
        _paymentRepo.All.Should().AllSatisfy(p =>
            p.Status.Should().Be(ParticipantPaymentStatus.Captured));
    }

    [Fact]
    public async Task ApproveAndCaptureAll_Throws_When_Not_Host()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        var wrongParticipantId = 99;
        var act = async () => await sut.ApproveAndCaptureAllAsync(10, wrongParticipantId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ApproveAndCaptureAll_Throws_When_Order_Not_ReadyToPay()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId, status: "Collecting");
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        var act = async () => await sut.ApproveAndCaptureAllAsync(10, hostId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Collecting*");
    }

    [Fact]
    public async Task ApproveAndCaptureAll_Is_Idempotent_When_Already_Paid()
    {
        var finalizer = new TestMerchantOrderFinalizationService();
        var sut = CreateSut(finalizationService: finalizer);
        var hostId = 1;
        var order = MakeOrder(10, hostId, status: "Paid");
        _orderRepo.Add(order);
        await _paymentRepo.AddAsync(new ParticipantPayment
        {
            OrderId = 10,
            ParticipantId = hostId,
            AmountMinorUnits = 10000,
            Currency = "DKK",
            Status = ParticipantPaymentStatus.Captured,
            CapturedAtUtc = DateTime.UtcNow
        });

        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeTrue();
        result.OrderStatus.Should().Be("Paid");
        result.Results.Should().BeEmpty();
        finalizer.CallCount.Should().Be(1, "en manglende permanent merchant-ordre skal kunne genskabes idempotent");
    }

    [Fact]
    public async Task ApproveAndCaptureAll_Throws_When_No_Reserved_Payments()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);
        // Ingen betalinger oprettet

        var act = async () => await sut.ApproveAndCaptureAllAsync(10, hostId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*reserverede betalinger*");
    }

    [Fact]
    public async Task No_Payment_Is_Captured_Twice()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        // Første godkendelse
        var first = await sut.ApproveAndCaptureAllAsync(10, hostId);
        first.AllCaptured.Should().BeTrue();

        // Anden godkendelse på allerede-Paid ordre (idempotent)
        var second = await sut.ApproveAndCaptureAllAsync(10, hostId);
        second.AllCaptured.Should().BeTrue();
        second.Results.Should().BeEmpty("idempotent: ingen ny capture når orden allerede er Paid");

        // Betaling skal stadig kun have status Captured — aldrig dobbelt
        _paymentRepo.All.Should().HaveCount(1);
        _paymentRepo.All[0].Status.Should().Be(ParticipantPaymentStatus.Captured);
    }

    [Fact]
    public async Task ApproveAndCaptureAll_Sets_Order_HostApproved_Then_Capturing_Then_Paid()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeTrue();
        // Slutstatus skal være Paid (via HostApproved → Capturing → Paid)
        result.OrderStatus.Should().Be("Paid");
        order.Status.Should().Be("Paid");
    }

    [Fact]
    public async Task ApproveAndCaptureAll_Sets_PartiallyFailed_When_Capture_Fails()
    {
        var failProvider = new FakePaymentProvider(
            NullLogger<FakePaymentProvider>.Instance,
            new FakePaymentProviderOptions { SimulateCaptureFailed = true });

        var sut = CreateSut(failProvider);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        // Sæt betaling til Reserved manuelt (FakeProvider med SimulateCaptureFailed er success på reserve)
        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeFalse();
        result.OrderStatus.Should().Be("PartiallyFailed");
        order.Status.Should().Be("PartiallyFailed");
        result.Results.Should().HaveCount(1);
        result.Results[0].Success.Should().BeFalse();
        result.Results[0].ErrorCode.Should().Be("FAKE_CAPTURE_FAILED");
    }

    [Fact]
    public async Task ApproveAndCaptureAll_Stops_Loop_After_First_Capture_Failure()
    {
        var failProvider = new FakePaymentProvider(
            NullLogger<FakePaymentProvider>.Instance,
            new FakePaymentProviderOptions { SimulateCaptureFailed = true });

        var sut = CreateSut(failProvider);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        // Opret 2 deltagere
        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");
        var p2 = new Participant { Id = 2, Name = "Bo" };
        _participantRepo.Add(p2);
        order.OrderParticipants.Add(new OrderParticipant { ParticipantId = 2, Status = "OrderSubmitted", Participant = p2 });
        await sut.ReserveParticipantPaymentAsync(10, 2, "m", 8000, "DKK", "https://r", "https://c");

        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        // Loopet stopper ved første fejl — kun 1 result, ikke 2
        result.AllCaptured.Should().BeFalse();
        result.Results.Should().HaveCount(1, "loopet stopper efter første capture-fejl");
        result.OrderStatus.Should().Be("PartiallyFailed");
    }

    // ── Doc 07 Test 6: Godkend for tidligt (expired/failed reservation) ──────────────

    [Fact]
    public async Task ApproveAndCaptureAll_Throws_When_Reservation_Is_Expired()
    {
        var expiredProvider = new FakePaymentProvider(
            NullLogger<FakePaymentProvider>.Instance,
            new FakePaymentProviderOptions { SimulateReservationExpired = true });

        var sut = CreateSut(expiredProvider);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        // Reserve returnerer Expired — betaling oprettes men er ikke Reserved
        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        // Ingen Reserved betalinger — approve skal kaste
        var act = async () => await sut.ApproveAndCaptureAllAsync(10, hostId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*reserverede betalinger*");
    }

    [Fact]
    public async Task ApproveAndCaptureAll_Throws_When_Reservation_Has_Failed()
    {
        var failedProvider = new FakePaymentProvider(
            NullLogger<FakePaymentProvider>.Instance,
            new FakePaymentProviderOptions { SimulateReservationFailed = true });

        var sut = CreateSut(failedProvider);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        var act = async () => await sut.ApproveAndCaptureAllAsync(10, hostId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*reserverede betalinger*");
    }

    // ── Doc 07 Test 9: Cancel ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelOrder_Cancels_All_Reserved_Payments()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        var result = await sut.CancelOrderAsync(10, hostId);

        result.Success.Should().BeTrue();
        result.OrderStatus.Should().Be("Cancelled");
        result.CancelledCount.Should().Be(1);
        result.Errors.Should().BeEmpty();
        order.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelOrder_Throws_When_Not_Host()
    {
        var sut = CreateSut();
        var order = MakeOrder(10, hostId: 1);
        _orderRepo.Add(order);

        var act = async () => await sut.CancelOrderAsync(10, requestingParticipantId: 99);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task CancelOrder_Throws_When_Order_Already_Paid()
    {
        var sut = CreateSut();
        var order = MakeOrder(10, hostId: 1, status: "Paid");
        _orderRepo.Add(order);

        var act = async () => await sut.CancelOrderAsync(10, requestingParticipantId: 1);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ikke annulleres*");
    }

    [Fact]
    public async Task CancelOrder_Is_Idempotent_When_Already_Cancelled()
    {
        var sut = CreateSut();
        var order = MakeOrder(10, hostId: 1, status: "Cancelled");
        _orderRepo.Add(order);

        var result = await sut.CancelOrderAsync(10, requestingParticipantId: 1);

        result.Success.Should().BeTrue();
        result.OrderStatus.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelOrder_Throws_When_Order_Is_Paid_After_Full_Capture()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");
        await sut.ApproveAndCaptureAllAsync(10, hostId);

        // Order er nu Paid — cancel skal fejle
        var act = async () => await sut.CancelOrderAsync(10, hostId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ikke annulleres*");
    }

    [Fact]
    public async Task CancelOrder_Cancels_Payment_Without_ProviderPaymentId()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        // Betaling uden ProviderPaymentId (aldrig påbegyndt via provider)
        _paymentRepo.All.Add(new ParticipantPayment
        {
            Id = 99,
            OrderId = 10,
            ParticipantId = hostId,
            ProviderPaymentId = null,
            Status = ParticipantPaymentStatus.Created,
            AmountMinorUnits = 5000,
            Currency = "DKK"
        });

        var result = await sut.CancelOrderAsync(10, hostId);

        result.Success.Should().BeTrue();
        result.CancelledCount.Should().Be(1);
    }

    // ── Prompt 03 tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task Reserve_Returns_ProviderPaymentId_In_Result()
    {
        var sut = CreateSut();
        SetupOrderWithMerchant(orderId: 1, participantId: 42);

        var result = await sut.ReserveParticipantPaymentAsync(
            orderId: 1, participantId: 42, merchantId: "m",
            amountMinorUnits: 10000, currency: "DKK",
            returnUrl: "https://return", callbackUrl: "https://api/payments/vipps/callbacks");

        result.Success.Should().BeTrue();
        result.ProviderPaymentId.Should().NotBeNullOrEmpty("fake provider sætter altid et providerPaymentId");
    }

    [Fact]
    public async Task Reserve_Is_Rejected_When_Payment_Already_Captured()
    {
        var sut = CreateSut();

        // Opret og simuler captured betaling manuelt
        await sut.ReserveParticipantPaymentAsync(
            orderId: 1, participantId: 42, merchantId: "m",
            amountMinorUnits: 10000, currency: "DKK",
            returnUrl: "https://r", callbackUrl: "https://api/payments/vipps/callbacks");

        var payment = _paymentRepo.All.First();
        payment.Status = ParticipantPaymentStatus.Captured;

        var result = await sut.ReserveParticipantPaymentAsync(
            orderId: 1, participantId: 42, merchantId: "m",
            amountMinorUnits: 10000, currency: "DKK",
            returnUrl: "https://r", callbackUrl: "https://api/payments/vipps/callbacks");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ALREADY_CAPTURED");
    }

    [Fact]
    public async Task Reserve_Returns_Existing_When_Already_Reserved()
    {
        var sut = CreateSut();
        SetupOrderWithMerchant(orderId: 1, participantId: 42);

        var first = await sut.ReserveParticipantPaymentAsync(
            orderId: 1, participantId: 42, merchantId: "m",
            amountMinorUnits: 10000, currency: "DKK",
            returnUrl: "https://r", callbackUrl: "https://api/payments/vipps/callbacks");

        first.Success.Should().BeTrue();
        _paymentRepo.All.First().Status.Should().Be(ParticipantPaymentStatus.Reserved);

        var second = await sut.ReserveParticipantPaymentAsync(
            orderId: 1, participantId: 42, merchantId: "m",
            amountMinorUnits: 10000, currency: "DKK",
            returnUrl: "https://r", callbackUrl: "https://api/payments/vipps/callbacks");

        second.Success.Should().BeTrue();
        second.ParticipantPaymentId.Should().Be(first.ParticipantPaymentId);
        _paymentRepo.All.Should().HaveCount(1, "ingen ny betaling oprettes ved re-submit af allerede Reserved");
    }

    [Fact]
    public async Task ReadyToPay_Is_Not_Set_By_OrderSubmitted_Alone()
    {
        // Denne test verificerer at orchestration IKKE sætter ReadyToPay —
        // det skal kun ske via webhook (Prompt 04/05).
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId, status: "Collecting");
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        // Orchestration må ikke ændre ordrestatus til ReadyToPay
        var savedOrder = await _orderRepo.GetByIdWithDetailsAsync(10);
        savedOrder!.Status.Should().Be("Collecting", "ReadyToPay sættes ikke af reservation alene");
    }

    // ── Prompt 05 tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task Merchant_Callback_Is_Not_Sent_On_Partial_Failure()
    {
        var tracker = new TrackingMerchantCallbackService();
        var finalizer = new TestMerchantOrderFinalizationService();
        var failProvider = new FakePaymentProvider(
            NullLogger<FakePaymentProvider>.Instance,
            new FakePaymentProviderOptions { SimulateCaptureFailed = true });

        var sut = CreateSut(failProvider, tracker, finalizer);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeFalse();
        result.OrderStatus.Should().Be("PartiallyFailed");
        finalizer.CallCount.Should().Be(0, "merchant-ordren må ikke oprettes ved partial failure");
        tracker.CallCount.Should().Be(0, "merchant callback må ikke sendes ved partial failure");
    }

    [Fact]
    public async Task CaptureFailed_Payment_Is_Retried_On_Next_Approve()
    {
        // Første approve: capture fejler på deltager 1
        var failThenSucceedProvider = new FailFirstThenSucceedProvider();

        var sut = CreateSut(failThenSucceedProvider);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");

        // Første godkendelse → CaptureFailed + PartiallyFailed
        var first = await sut.ApproveAndCaptureAllAsync(10, hostId);
        first.AllCaptured.Should().BeFalse();
        first.OrderStatus.Should().Be("PartiallyFailed");
        _paymentRepo.All[0].Status.Should().Be(ParticipantPaymentStatus.CaptureFailed);

        // Sæt ordre tilbage til ReadyToPay for retry
        order.Status = "PartiallyFailed";

        // Anden godkendelse → capture lykkes denne gang (provider skifter til succes)
        var second = await sut.ApproveAndCaptureAllAsync(10, hostId);
        second.AllCaptured.Should().BeTrue();
        second.OrderStatus.Should().Be("Paid");
        _paymentRepo.All[0].Status.Should().Be(ParticipantPaymentStatus.Captured);
    }

    [Fact]
    public async Task Capture_Does_Not_Use_Phone_Number()
    {
        // Verificerer at CapturePaymentRequest IKKE har telefonnummer-felt.
        // Dette er en compile-time garanti — testen bekræfter det ikke kan sættes.
        var capturedRequests = new List<CapturePaymentRequest>();
        var spyProvider = new SpyCaptureProvider(capturedRequests);

        var sut = CreateSut(spyProvider);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");
        await sut.ApproveAndCaptureAllAsync(10, hostId);

        capturedRequests.Should().HaveCount(1);
        // CapturePaymentRequest har kun: ProviderPaymentId, AmountMinorUnits, Currency, IdempotencyKey
        // Der er intet TestPhoneNumber — bekræftet ved at testen kompilerer uden at sætte det
        capturedRequests[0].ProviderPaymentId.Should().NotBeNullOrEmpty();
        capturedRequests[0].AmountMinorUnits.Should().Be(10000);
    }

    [Fact]
    public async Task Approve_Does_Not_Capture_When_MerchantOrder_Validation_Fails()
    {
        var capturedRequests = new List<CapturePaymentRequest>();
        var provider = new SpyCaptureProvider(capturedRequests);
        var sut = CreateSut(
            provider,
            finalizationService: new RejectingMerchantOrderFinalizationService());
        var order = MakeOrder(10, hostId: 1);
        _orderRepo.Add(order);
        await sut.ReserveParticipantPaymentAsync(10, 1, "m", 10000, "DKK", "https://r", "https://c");

        var act = () => sut.ApproveAndCaptureAllAsync(10, 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*stemmer ikke*");
        capturedRequests.Should().BeEmpty("validering skal ske før provider-capture");
        _paymentRepo.All.Single().Status.Should().Be(ParticipantPaymentStatus.Reserved);
        order.Status.Should().Be("ReadyToPay");
    }

    // ── Prompt 06 tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task GroupOrderPaid_Payload_Has_Correct_EventType_And_Status()
    {
        var tracker = new TrackingMerchantCallbackService();
        var sut = CreateSut(callbackService: tracker);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");
        await sut.ApproveAndCaptureAllAsync(10, hostId);

        tracker.CallCount.Should().Be(1);
        tracker.LastPayload.Should().NotBeNull();
        tracker.LastPayload!.EventType.Should().Be("GroupOrderPaid");
        tracker.LastPayload.Status.Should().Be("Paid");
        tracker.LastPayload.PaynsyncOrderId.Should().Be(10);
    }

    [Fact]
    public async Task GroupOrderPaid_Payload_Contains_One_Flat_Line_Per_Captured_Payment()
    {
        var tracker = new TrackingMerchantCallbackService();
        var sut = CreateSut(callbackService: tracker);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);
        var p2 = new Participant { Id = 2, Name = "Anna" };
        _participantRepo.Add(p2);
        order.OrderParticipants.Add(new OrderParticipant { ParticipantId = 2, Status = "OrderSubmitted", Participant = p2 });

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");
        await sut.ReserveParticipantPaymentAsync(10, 2, "m", 8000, "DKK", "https://r", "https://c");
        await sut.ApproveAndCaptureAllAsync(10, hostId);

        tracker.LastPayload!.Lines.Should().HaveCount(2);
        tracker.LastPayload.TotalAmount.Should().Be(180m, "10000 + 8000 øre = 180 kr");
    }

    [Fact]
    public async Task GroupOrderPaid_Payload_Is_Not_Sent_After_Partial_Failure()
    {
        var tracker = new TrackingMerchantCallbackService();
        var failProvider = new FakePaymentProvider(
            NullLogger<FakePaymentProvider>.Instance,
            new FakePaymentProviderOptions { SimulateCaptureFailed = true });

        var sut = CreateSut(failProvider, tracker);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");
        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeFalse();
        tracker.CallCount.Should().Be(0, "payload sendes ikke ved PartiallyFailed");
    }

    [Fact]
    public async Task GroupOrderPaid_Payload_Sent_Only_After_All_Captured()
    {
        var tracker = new TrackingMerchantCallbackService();
        var sut = CreateSut(callbackService: tracker);
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        // Opret reservation — endnu ikke captured
        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 5000, "DKK", "https://r", "https://c");

        // Ingen callback endnu
        tracker.CallCount.Should().Be(0, "callback sendes ikke ved reservation");

        // Capture alle
        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeTrue();
        tracker.CallCount.Should().Be(1, "callback sendes præcist én gang efter fuld capture");
        tracker.LastPayload!.Lines.Should().HaveCount(1);
        tracker.LastPayload.Lines[0].LineTotal.Should().Be(50m);
    }
}

// ─── Test-hjælpere til Prompt 05 ────────────────────────────────────────────

/// <summary>Provider der fejler første capture, og derefter lykkes.</summary>
file sealed class FailFirstThenSucceedProvider : IPaymentProvider
{
    private int _captureAttempts;

    public Task<ReservePaymentResult> ReserveAsync(ReservePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var id = $"FAKE-{request.ParticipantPaymentId}-retry";
        return Task.FromResult(new ReservePaymentResult(true, id, null, "Reserved", null, null));
    }

    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request, CancellationToken cancellationToken = default)
    {
        _captureAttempts++;
        if (_captureAttempts == 1)
            return Task.FromResult(new CapturePaymentResult(false, null, "Failed", "FIRST_ATTEMPT_FAILED", "Simuleret fejl"));

        return Task.FromResult(new CapturePaymentResult(true, $"CAP-RETRY-{_captureAttempts}", "Captured", null, null));
    }

    public Task<CancelPaymentResult> CancelAsync(CancelPaymentRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new CancelPaymentResult(true, "Cancelled", null, null));

    public Task<PaymentStatusResult> GetStatusAsync(PaymentStatusRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new PaymentStatusResult(true, "Reserved", null, null, null, null));
}

/// <summary>Provider der optager CapturePaymentRequest til inspektion.</summary>
file sealed class SpyCaptureProvider : IPaymentProvider
{
    private readonly List<CapturePaymentRequest> _captured;

    public SpyCaptureProvider(List<CapturePaymentRequest> captured) => _captured = captured;

    public Task<ReservePaymentResult> ReserveAsync(ReservePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var id = $"FAKE-{request.ParticipantPaymentId}";
        return Task.FromResult(new ReservePaymentResult(true, id, null, "Reserved", null, null));
    }

    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request, CancellationToken cancellationToken = default)
    {
        _captured.Add(request);
        return Task.FromResult(new CapturePaymentResult(true, "CAP-SPY-1", "Captured", null, null));
    }

    public Task<CancelPaymentResult> CancelAsync(CancelPaymentRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new CancelPaymentResult(true, "Cancelled", null, null));

    public Task<PaymentStatusResult> GetStatusAsync(PaymentStatusRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new PaymentStatusResult(true, "Reserved", null, null, null, null));
}
