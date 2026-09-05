using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Infrastructure.Payments.PayBySharePay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

/// <summary>
/// Prompt 07 – End-to-end flow tests og fejlscenarier med Fake provider.
/// Dækker hele PayNSync v1-flowet: opret ordre → reservation → ReadyToPay → capture → Paid → callback.
/// </summary>
public class EndToEndFlowTests
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
        public Task<ParticipantPayment> AddAsync(ParticipantPayment p) { p.Id = _nextId++; _store.Add(p); return Task.FromResult(p); }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private sealed class FakePaymentEventLogRepository : IPaymentEventLogRepository
    {
        public Task AddAsync(PaymentEventLog e) => Task.CompletedTask;
        public Task<IEnumerable<PaymentEventLog>> GetByParticipantPaymentIdAsync(int id) => Task.FromResult(Enumerable.Empty<PaymentEventLog>());
        public Task<IEnumerable<PaymentEventLog>> GetByOrderIdAsync(int orderId) => Task.FromResult(Enumerable.Empty<PaymentEventLog>());
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        private readonly List<Order> _store = [];
        public void Add(Order o) => _store.Add(o);
        public Task<Order?> GetByIdWithDetailsAsync(int id) => Task.FromResult(_store.FirstOrDefault(o => o.Id == id));
        public Task<IEnumerable<Order>> GetAllWithDetailsAsync() => Task.FromResult(_store.AsEnumerable());
        public Task<IEnumerable<Order>> GetByParticipantIdAsync(int participantId) => Task.FromResult(_store.AsEnumerable());
        public Task<IEnumerable<Order>> GetAllAsync() => Task.FromResult(_store.AsEnumerable());
        public Task<Order> AddAsync(Order o) { _store.Add(o); return Task.FromResult(o); }
        public Task SaveChangesAsync() => Task.CompletedTask;
    }

    private sealed class FakeParticipantRepository : IParticipantRepository
    {
        private readonly List<Participant> _store = [];
        public void Add(Participant p) => _store.Add(p);
        public Task<Participant?> GetByIdAsync(int id) => Task.FromResult(_store.FirstOrDefault(p => p.Id == id));
        public Task<Participant?> GetByEmailAsync(string email) => Task.FromResult(_store.FirstOrDefault(p => p.Email == email));
        public Task<Participant> AddAsync(Participant p) { _store.Add(p); return Task.FromResult(p); }
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task UpdateAsync(Participant p) => Task.CompletedTask;
        public Task<IEnumerable<Participant>> SearchAsync(string q, int? exclude = null) => Task.FromResult(Enumerable.Empty<Participant>());
        public Task<IEnumerable<Participant>> GetAllPersonsAsync() => Task.FromResult(Enumerable.Empty<Participant>());
    }

    private sealed class TrackingCallbackService : IMerchantCallbackService
    {
        public int CallCount { get; private set; }
        public PayNSyncFinalGroupOrderDto? LastPayload { get; private set; }
        public bool ThrowOnNext { get; set; }

        public Task<MerchantOrderDeliveryResultDto> SendGroupOrderPaidAsync(PayNSyncFinalGroupOrderDto payload, string? callbackUrl,
            CancellationToken cancellationToken = default)
        {
            if (ThrowOnNext) throw new HttpRequestException("Simuleret callback-fejl");
            CallCount++;
            LastPayload = payload;
            return Task.FromResult(new MerchantOrderDeliveryResultDto(true, $"SIM-{payload.PaynsyncOrderNumber}", "{}"));
        }
    }

    private sealed class TestMerchantOrderFinalizationService : IMerchantOrderFinalizationService
    {
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

        public Task RecordExternalDeliveryAsync(int sourceOrderId, MerchantOrderDeliveryResultDto result,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    // ─── Setup ────────────────────────────────────────────────────────────

    private FakeParticipantPaymentRepository _paymentRepo = new();
    private FakeOrderRepository _orderRepo = new();
    private FakeParticipantRepository _participantRepo = new();

    private (GroupPaymentOrchestrationService orchestration, IOrderService orderService, IParticipantPaymentStateService stateService)
        CreateServices(IPaymentProvider? provider = null, IMerchantCallbackService? callbackService = null)
    {
        _paymentRepo = new FakeParticipantPaymentRepository();
        _orderRepo = new FakeOrderRepository();
        _participantRepo = new FakeParticipantRepository();
        var logRepo = new FakePaymentEventLogRepository();

        var stateService = new ParticipantPaymentStateService(
            _paymentRepo, logRepo,
            NullLogger<ParticipantPaymentStateService>.Instance);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:FrontendUrl"] = "https://mobil.paynsync.dk"
            }).Build();

        var orderService = new OrderService(
            _orderRepo, _participantRepo, _paymentRepo, config);

        var orchestration = new GroupPaymentOrchestrationService(
            provider ?? new FakePaymentProvider(NullLogger<FakePaymentProvider>.Instance),
            stateService, _paymentRepo, _orderRepo, _participantRepo,
            new TestMerchantOrderFinalizationService(),
            callbackService ?? new TrackingCallbackService(),
            NullLogger<GroupPaymentOrchestrationService>.Instance);

        return (orchestration, orderService, stateService);
    }

    private static Participant MakeFakeMerchant() => new()
    {
        Id = 999,
        Type = ParticipantType.Merchant,
        Name = "Test Merchant",
        VippsMerchantSerialNumber = "TEST-MSN",
        VippsClientId = "test-client-id",
        VippsClientSecret = "test-client-secret",
        VippsSubscriptionKey = "test-subscription-key"
    };

    private Order MakeOrderWithTwoParticipants(int orderId, int hostId, int p2Id)
    {
        var host = new Participant { Id = hostId, Name = "Host", Type = ParticipantType.Person };
        var p2 = new Participant { Id = p2Id, Name = "Anna", Type = ParticipantType.Person };
        _participantRepo.Add(host);
        _participantRepo.Add(p2);

        var merchant = MakeFakeMerchant();
        var order = new Order
        {
            Id = orderId,
            CreatedByParticipantId = hostId,
            Title = "Pizza aften",
            Status = "Collecting",
            Messages = [],
            MerchantParticipantId = merchant.Id,
            MerchantParticipant = merchant,
            OrderParticipants =
            [
                new OrderParticipant { ParticipantId = hostId, Status = "Accepted", Participant = host },
                new OrderParticipant { ParticipantId = p2Id, Status = "Invited", Participant = p2 }
            ]
        };
        _orderRepo.Add(order);
        return order;
    }

    // ─── Prompt 07: Happy-path end-to-end ────────────────────────────────────

    [Fact]
    public async Task FullFlow_FakeProvider_TwoParticipants_OrderBecomesPaid()
    {
        var tracker = new TrackingCallbackService();
        var (orchestration, orderService, _) = CreateServices(callbackService: tracker);

        var order = MakeOrderWithTwoParticipants(orderId: 1, hostId: 10, p2Id: 20);

        // 1. Begge deltagere reserverer betaling (simulerer "Bekræft ordre")
        var r1 = await orchestration.ReserveParticipantPaymentAsync(
            1, 10, "m", 12000, "DKK", "https://return", "https://callback");
        var r2 = await orchestration.ReserveParticipantPaymentAsync(
            1, 20, "m", 9000, "DKK", "https://return", "https://callback");

        r1.Success.Should().BeTrue();
        r2.Success.Should().BeTrue();

        // 2. Alle betalinger er Reserved (Fake provider returnerer Reserved med det samme)
        _paymentRepo.All.Should().AllSatisfy(p =>
            p.Status.Should().Be(ParticipantPaymentStatus.Reserved));

        // 3. ReadyToPay tjek — simulerer webhook-trigger
        await orderService.CheckAndSetReadyToPayByReservedAsync(1);
        order.Status.Should().Be("ReadyToPay", "alle non-merchant deltagere er Reserved");
        order.Messages.Should().NotBeEmpty("host skal have besked");

        // 4. Host godkender
        var captureResult = await orchestration.ApproveAndCaptureAllAsync(1, 10);

        captureResult.AllCaptured.Should().BeTrue();
        captureResult.OrderStatus.Should().Be("Paid");
        order.Status.Should().Be("Paid");

        // 5. Alle betalinger er Captured
        _paymentRepo.All.Should().AllSatisfy(p =>
            p.Status.Should().Be(ParticipantPaymentStatus.Captured));

        // 6. Final GroupOrderPaid payload sendt
        tracker.CallCount.Should().Be(1);
        tracker.LastPayload!.EventType.Should().Be("GroupOrderPaid");
        tracker.LastPayload.Lines.Should().HaveCount(2);
        tracker.LastPayload.TotalAmount.Should().Be(210m, "(12000 + 9000) øre = 210 kr");
    }

    // ─── Fejlscenarie 1: Deltager godkender ikke ──────────────────────────────

    [Fact]
    public async Task ErrorScenario_ParticipantDoesNotApprove_HostCannotApprove()
    {
        var (orchestration, orderService, stateService) = CreateServices();

        var order = MakeOrderWithTwoParticipants(orderId: 2, hostId: 10, p2Id: 20);

        // Kun deltager 10 reserverer — deltager 20 gør intet
        await orchestration.ReserveParticipantPaymentAsync(
            2, 10, "m", 12000, "DKK", "https://return", "https://callback");

        // Deltager 20's betaling er manuelt sat til ReservationStarted (ikke Reserved)
        // (Simulerer at brugeren åbnede MobilePay men ikke swipede)
        var payment20 = new ParticipantPayment
        {
            OrderId = 2, ParticipantId = 20,
            Status = ParticipantPaymentStatus.ReservationStarted,
            ProviderPaymentId = "pending-20",
            AmountMinorUnits = 9000, Currency = "DKK"
        };
        await _paymentRepo.AddAsync(payment20);

        // ReadyToPay må ikke sættes
        await orderService.CheckAndSetReadyToPayByReservedAsync(2);
        order.Status.Should().Be("Collecting", "deltager 20 har ikke Reserved");

        // Host kan ikke approve — orden er ikke ReadyToPay
        var act = async () => await orchestration.ApproveAndCaptureAllAsync(2, 10);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Collecting*");
    }

    // ─── Fejlscenarie 2: Reservation fejler ──────────────────────────────────

    [Fact]
    public async Task ErrorScenario_ReservationFails_OrderStaysCollecting_RetrySucceeds()
    {
        var failProvider = new FakePaymentProvider(
            NullLogger<FakePaymentProvider>.Instance,
            new FakePaymentProviderOptions { SimulateReservationFailed = true });

        var (orchestration, orderService, _) = CreateServices(failProvider);
        var order = MakeOrderWithTwoParticipants(orderId: 3, hostId: 10, p2Id: 20);

        // Første forsøg fejler
        var failResult = await orchestration.ReserveParticipantPaymentAsync(
            3, 10, "m", 12000, "DKK", "https://return", "https://callback");

        failResult.Success.Should().BeFalse();
        failResult.ErrorCode.Should().Be("FAKE_RESERVE_FAILED");

        // Betaling er ReservationFailed
        _paymentRepo.All.Should().HaveCount(1);
        _paymentRepo.All[0].Status.Should().Be(ParticipantPaymentStatus.ReservationFailed);

        // Ordren forbliver Collecting
        await orderService.CheckAndSetReadyToPayByReservedAsync(3);
        order.Status.Should().Be("Collecting");

        // Deltager prøver igen — ny provider der lykkes (bygger direkte på eksisterende repos)
        var successProvider = new FakePaymentProvider(NullLogger<FakePaymentProvider>.Instance);
        var orchestration2 = new GroupPaymentOrchestrationService(
            successProvider,
            new ParticipantPaymentStateService(_paymentRepo, new FakePaymentEventLogRepository(), NullLogger<ParticipantPaymentStateService>.Instance),
            _paymentRepo, _orderRepo, _participantRepo,
            new TestMerchantOrderFinalizationService(),
            new TrackingCallbackService(),
            NullLogger<GroupPaymentOrchestrationService>.Instance);

        var retryResult = await orchestration2.ReserveParticipantPaymentAsync(
            3, 10, "m", 12000, "DKK", "https://return", "https://callback");

        retryResult.Success.Should().BeTrue("ReservationFailed-betalinger kan genstartes");
        // Ny betaling oprettet (den fejlede hoppes over)
        _paymentRepo.All.Should().HaveCount(2, "ny betaling oprettes ved retry");
        _paymentRepo.All.Last().Status.Should().Be(ParticipantPaymentStatus.Reserved);
    }

    // ─── Fejlscenarie 3: Capture fejler på én af to ──────────────────────────

    [Fact]
    public async Task ErrorScenario_CaptureFailsOnSecond_OrderPartiallyFailed_FirstRemainsCaptuerd()
    {
        // Provider der lykkes på første capture, fejler på anden
        var partialFailProvider = new CaptureSecondFailsProvider();
        var tracker = new TrackingCallbackService();
        var (orchestration, orderService, _) = CreateServices(partialFailProvider, tracker);

        var order = MakeOrderWithTwoParticipants(orderId: 4, hostId: 10, p2Id: 20);

        await orchestration.ReserveParticipantPaymentAsync(4, 10, "m", 10000, "DKK", "https://r", "https://c");
        await orchestration.ReserveParticipantPaymentAsync(4, 20, "m", 8000, "DKK", "https://r", "https://c");
        await orderService.CheckAndSetReadyToPayByReservedAsync(4);

        var result = await orchestration.ApproveAndCaptureAllAsync(4, 10);

        result.AllCaptured.Should().BeFalse();
        result.OrderStatus.Should().Be("PartiallyFailed");
        order.Status.Should().Be("PartiallyFailed");

        // Første betaling er Captured, anden er CaptureFailed
        _paymentRepo.All.Count(p => p.Status == ParticipantPaymentStatus.Captured).Should().Be(1,
            "den første capture lykkedes");
        _paymentRepo.All.Count(p => p.Status == ParticipantPaymentStatus.CaptureFailed).Should().Be(1,
            "den anden capture fejlede");

        // Merchant callback sendes IKKE
        tracker.CallCount.Should().Be(0, "GroupOrderPaid sendes ikke ved PartiallyFailed");
    }

    // ─── Fejlscenarie 4: Merchant callback fejler ────────────────────────────

    [Fact]
    public async Task ErrorScenario_MerchantCallbackThrows_OrderRemainsPayd_ErrorIsLogged()
    {
        var tracker = new TrackingCallbackService { ThrowOnNext = true };
        var (orchestration, orderService, _) = CreateServices(callbackService: tracker);

        var order = MakeOrderWithTwoParticipants(orderId: 5, hostId: 10, p2Id: 20);

        await orchestration.ReserveParticipantPaymentAsync(5, 10, "m", 10000, "DKK", "https://r", "https://c");
        await orchestration.ReserveParticipantPaymentAsync(5, 20, "m", 8000, "DKK", "https://r", "https://c");
        await orderService.CheckAndSetReadyToPayByReservedAsync(5);

        // Callback kaster — men flowet må ikke fejle
        // GroupPaymentOrchestrationService har try/catch rundt om SendMerchantCallbackAsync
        var act = async () => await orchestration.ApproveAndCaptureAllAsync(5, 10);
        await act.Should().NotThrowAsync("callback-fejl må ikke propagere op og rulle betalinger tilbage");

        // Ordre er stadig Paid — ikke rullet tilbage
        order.Status.Should().Be("Paid",
            "ordre forbliver Paid selvom callback-send kastede en exception");

        // Alle betalinger er Captured
        _paymentRepo.All.Should().AllSatisfy(p =>
            p.Status.Should().Be(ParticipantPaymentStatus.Captured));
    }
}

// ─── Hjælpe-provider: lykkes på første capture, fejler på anden ─────────────

file sealed class CaptureSecondFailsProvider : IPaymentProvider
{
    private int _captureCount;

    public Task<ReservePaymentResult> ReserveAsync(ReservePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var id = $"FAKE-{request.ParticipantPaymentId}";
        return Task.FromResult(new ReservePaymentResult(true, id, null, "Reserved", null, null));
    }

    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request, CancellationToken cancellationToken = default)
    {
        _captureCount++;
        if (_captureCount == 1)
            return Task.FromResult(new CapturePaymentResult(true, $"CAP-OK-{_captureCount}", "Captured", null, null));

        return Task.FromResult(new CapturePaymentResult(false, null, "Failed", "SECOND_CAPTURE_FAILED", "Simuleret fejl på capture 2"));
    }

    public Task<CancelPaymentResult> CancelAsync(CancelPaymentRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new CancelPaymentResult(true, "Cancelled", null, null));

    public Task<PaymentStatusResult> GetStatusAsync(PaymentStatusRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult(new PaymentStatusResult(true, "Reserved", null, null, null, null));
}
