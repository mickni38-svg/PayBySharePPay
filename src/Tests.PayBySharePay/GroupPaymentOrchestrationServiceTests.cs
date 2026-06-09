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
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private sealed class NoOpMerchantCallbackService : IMerchantCallbackService
    {
        public Task SendPaidCallbackAsync(int orderId, string? callbackUrl, string? merchantId,
            IEnumerable<MerchantCallbackParticipantOrder> participantOrders,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private FakeParticipantPaymentRepository _paymentRepo = new();
    private FakePaymentEventLogRepository _logRepo = new();
    private FakeOrderRepository _orderRepo = new();
    private FakeParticipantRepository _participantRepo = new();

    private GroupPaymentOrchestrationService CreateSut(IPaymentProvider? provider = null)
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
            new NoOpMerchantCallbackService(),
            NullLogger<GroupPaymentOrchestrationService>.Instance);
    }

    private static Order MakeOrder(int id, int hostId, string status = "ReadyToPay")
    {
        var order = new Order
        {
            Id = id,
            CreatedByParticipantId = hostId,
            Title = "Test ordre",
            Status = status,
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

    [Fact]
    public async Task ReserveParticipantPayment_Returns_Success_And_Creates_Payment()
    {
        var sut = CreateSut();

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
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId, status: "Paid");
        _orderRepo.Add(order);

        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeTrue();
        result.OrderStatus.Should().Be("Paid");
        result.Results.Should().BeEmpty();
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

    // ── Doc 07 Test 4: Alle reserveret → begge captures lykkes ──────────────────────

    [Fact]
    public async Task All_Participants_Reserved_Means_Both_Payments_Are_Captured()
    {
        var sut = CreateSut();
        var hostId = 1;
        var order = MakeOrder(10, hostId);
        _orderRepo.Add(order);

        var p2 = new Participant { Id = 2, Name = "Anna" };
        _participantRepo.Add(p2);
        order.OrderParticipants.Add(new OrderParticipant
            { ParticipantId = 2, Status = "OrderSubmitted", Participant = p2 });

        await sut.ReserveParticipantPaymentAsync(10, hostId, "m", 10000, "DKK", "https://r", "https://c");
        await sut.ReserveParticipantPaymentAsync(10, 2, "m", 8000, "DKK", "https://r", "https://c");

        var result = await sut.ApproveAndCaptureAllAsync(10, hostId);

        result.AllCaptured.Should().BeTrue();
        result.Results.Should().HaveCount(2);
        result.Results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        result.OrderStatus.Should().Be("Paid");
    }
}
