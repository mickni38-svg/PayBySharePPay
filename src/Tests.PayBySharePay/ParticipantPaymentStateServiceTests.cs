using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

public class ParticipantPaymentStateServiceTests
{
    // ---- in-memory fakes ----

    private sealed class FakeParticipantPaymentRepository : IParticipantPaymentRepository
    {
        private readonly List<ParticipantPayment> _store = [];
        private int _nextId = 1;

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
        public List<PaymentEventLog> Logs { get; } = [];

        public Task AddAsync(PaymentEventLog eventLog)
        {
            Logs.Add(eventLog);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<PaymentEventLog>> GetByParticipantPaymentIdAsync(int id)
            => Task.FromResult(Logs.Where(e => e.ParticipantPaymentId == id));

        public Task<IEnumerable<PaymentEventLog>> GetByOrderIdAsync(int orderId)
            => Task.FromResult(Logs.Where(e => e.OrderId == orderId));
    }

    private FakeParticipantPaymentRepository _paymentRepo = new();
    private FakePaymentEventLogRepository _logRepo = new();

    private ParticipantPaymentStateService CreateSut()
    {
        _paymentRepo = new FakeParticipantPaymentRepository();
        _logRepo = new FakePaymentEventLogRepository();
        return new ParticipantPaymentStateService(
            _paymentRepo,
            _logRepo,
            NullLogger<ParticipantPaymentStateService>.Instance);
    }

    private async Task<int> CreatePaymentAsync(ParticipantPaymentStateService svc)
    {
        var payment = await svc.CreateAsync(1, 1, "merchant-1", 10000, "DKK", "Fake");
        return payment.Id;
    }

    // ---- tests ----

    [Fact]
    public async Task Created_To_ReservationStarted_To_Reserved()
    {
        var svc = CreateSut();
        var id = await CreatePaymentAsync(svc);

        await svc.SetReservationStartedAsync(id, "prov-001");
        await svc.SetReservedAsync(id);

        var payment = await _paymentRepo.GetByIdAsync(id);
        payment!.Status.Should().Be(ParticipantPaymentStatus.Reserved);
        payment.ReservedAtUtc.Should().NotBeNull();
        _logRepo.Logs.Should().Contain(l => l.EventType == "Reserved");
    }

    [Fact]
    public async Task Reserved_To_CapturePending_To_Captured()
    {
        var svc = CreateSut();
        var id = await CreatePaymentAsync(svc);

        await svc.SetReservationStartedAsync(id, "prov-002");
        await svc.SetReservedAsync(id);
        await svc.SetCapturePendingAsync(id);
        await svc.SetCapturedAsync(id);

        var payment = await _paymentRepo.GetByIdAsync(id);
        payment!.Status.Should().Be(ParticipantPaymentStatus.Captured);
        payment.CapturedAtUtc.Should().NotBeNull();
        _logRepo.Logs.Should().Contain(l => l.EventType == "Captured");
    }

    [Fact]
    public async Task CapturePending_To_CaptureFailed_To_CapturePending()
    {
        var svc = CreateSut();
        var id = await CreatePaymentAsync(svc);

        await svc.SetReservationStartedAsync(id, "prov-003");
        await svc.SetReservedAsync(id);
        await svc.SetCapturePendingAsync(id);
        await svc.SetCaptureFailedAsync(id, "ERR", "timeout");
        await svc.SetCapturePendingAsync(id);

        var payment = await _paymentRepo.GetByIdAsync(id);
        payment!.Status.Should().Be(ParticipantPaymentStatus.CapturePending);
    }

    [Fact]
    public async Task Duplicate_Webhook_Same_Status_Is_Idempotent()
    {
        var svc = CreateSut();
        var id = await CreatePaymentAsync(svc);

        await svc.SetReservationStartedAsync(id, "prov-004");
        await svc.SetReservedAsync(id);

        var logCountBefore = _logRepo.Logs.Count;
        await svc.SetReservedAsync(id); // second call — idempotent
        _logRepo.Logs.Count.Should().Be(logCountBefore);

        var payment = await _paymentRepo.GetByIdAsync(id);
        payment!.Status.Should().Be(ParticipantPaymentStatus.Reserved);
    }

    [Fact]
    public async Task Invalid_Transition_Throws()
    {
        var svc = CreateSut();
        var id = await CreatePaymentAsync(svc);

        // Created -> Captured is not a valid transition
        var act = async () => await svc.SetCapturedAsync(id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid state transition*");
    }
}
