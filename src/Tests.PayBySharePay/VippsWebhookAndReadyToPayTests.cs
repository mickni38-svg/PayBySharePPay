using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Service.PayBySharePay.Interfaces;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

/// <summary>
/// Tests for Prompt 04: ReadyToPay kun nÃ¥r alle participants er Reserved.
/// Webhook-adfÃ¦rden (AUTHORIZEDâ†’Reserved) testes via stateService direkte.
/// </summary>
public class VippsWebhookAndReadyToPayTests
{
    // â”€â”€â”€ In-memory fakes â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private sealed class FakeParticipantPaymentRepository : IParticipantPaymentRepository
    {
        private readonly List<ParticipantPayment> _store = [];
        private int _nextId = 1;

        public List<ParticipantPayment> All => _store;

        public void Add(ParticipantPayment p) { p.Id = _nextId++; _store.Add(p); }

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
        public Order? LastSaved { get; private set; }

        public void Add(Order o) => _store.Add(o);

        public Task<Order?> GetByIdWithDetailsAsync(int id)
            => Task.FromResult(_store.FirstOrDefault(o => o.Id == id));

        public Task<IEnumerable<Order>> GetAllWithDetailsAsync()
            => Task.FromResult(_store.AsEnumerable());

        public Task<IEnumerable<Order>> GetByParticipantIdAsync(int participantId)
            => Task.FromResult(_store.AsEnumerable());

        public Task<IEnumerable<Order>> GetAllAsync()
            => Task.FromResult(_store.AsEnumerable());

        public Task<Order> AddAsync(Order o) { _store.Add(o); return Task.FromResult(o); }

        public Task SaveChangesAsync() { LastSaved = _store.LastOrDefault(); return Task.CompletedTask; }
    }

    private sealed class FakeParticipantRepository : IParticipantRepository
    {
        public Task<Participant?> GetByIdAsync(int id) => Task.FromResult<Participant?>(null);
        public Task<Participant?> GetByEmailAsync(string email) => Task.FromResult<Participant?>(null);
        public Task<Participant> AddAsync(Participant p) => Task.FromResult(p);
        public Task SaveChangesAsync() => Task.CompletedTask;
        public Task UpdateAsync(Participant p) => Task.CompletedTask;
        public Task<IEnumerable<Participant>> SearchAsync(string q, int? exclude = null)
            => Task.FromResult(Enumerable.Empty<Participant>());
        public Task<IEnumerable<Participant>> GetAllPersonsAsync()
            => Task.FromResult(Enumerable.Empty<Participant>());
    }

    // â”€â”€â”€ Setup â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private FakeParticipantPaymentRepository _paymentRepo = new();
    private FakeOrderRepository _orderRepo = new();

    private (IOrderService orderService, IParticipantPaymentStateService stateService)
        CreateServices()
    {
        _paymentRepo = new FakeParticipantPaymentRepository();
        _orderRepo = new FakeOrderRepository();
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
            _orderRepo,
            new FakeParticipantRepository(),
            _paymentRepo,
            config);

        return (orderService, stateService);
    }

    private static Order MakeOrderTwoParticipants(int id, int hostId, int participant2Id,
        string status = "Collecting")
    {
        return new Order
        {
            Id = id,
            CreatedByParticipantId = hostId,
            Title = "Testordre",
            Status = status,
            Messages = [],
            OrderParticipants =
            [
                new OrderParticipant
                {
                    ParticipantId = hostId,
                    Status = "OrderSubmitted",
                    Participant = new Participant { Id = hostId, Name = "Host", Type = ParticipantType.Person }
                },
                new OrderParticipant
                {
                    ParticipantId = participant2Id,
                    Status = "OrderSubmitted",
                    Participant = new Participant { Id = participant2Id, Name = "Anna", Type = ParticipantType.Person }
                }
            ]
        };
    }

    // â”€â”€â”€ Tests: AUTHORIZED webhook â†’ Reserved â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task SetReservedAsync_Changes_Status_To_Reserved()
    {
        var (_, stateService) = CreateServices();

        _paymentRepo.Add(new ParticipantPayment
        {
            OrderId = 10, ParticipantId = 1,
            ProviderPaymentId = "vipps-ref-1",
            Status = ParticipantPaymentStatus.ReservationStarted
        });

        await stateService.SetReservedAsync(_paymentRepo.All[0].Id, "webhook-test");

        _paymentRepo.All[0].Status.Should().Be(ParticipantPaymentStatus.Reserved);
    }

    [Fact]
    public async Task SetCancelledAsync_Changes_Status_To_Cancelled()
    {
        var (_, stateService) = CreateServices();

        _paymentRepo.Add(new ParticipantPayment
        {
            OrderId = 10, ParticipantId = 1,
            ProviderPaymentId = "vipps-ref-1",
            Status = ParticipantPaymentStatus.ReservationStarted
        });

        await stateService.SetCancelledAsync(_paymentRepo.All[0].Id, "webhook-cancelled");

        _paymentRepo.All[0].Status.Should().Be(ParticipantPaymentStatus.Cancelled);
    }

    [Fact]
    public async Task SetReservationFailedAsync_Changes_Status_To_ReservationFailed()
    {
        var (_, stateService) = CreateServices();

        _paymentRepo.Add(new ParticipantPayment
        {
            OrderId = 10, ParticipantId = 1,
            ProviderPaymentId = "vipps-ref-1",
            Status = ParticipantPaymentStatus.ReservationStarted
        });

        await stateService.SetReservationFailedAsync(_paymentRepo.All[0].Id, "EXPIRED", "UdlÃ¸bet", "webhook-expired");

        _paymentRepo.All[0].Status.Should().Be(ParticipantPaymentStatus.ReservationFailed);
    }

    // â”€â”€â”€ Tests: CheckAndSetReadyToPayByReservedAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    [Fact]
    public async Task ReadyToPay_Is_Not_Set_When_Only_One_Of_Two_Is_Reserved()
    {
        var (orderService, _) = CreateServices();

        var order = MakeOrderTwoParticipants(10, 1, 2);
        _orderRepo.Add(order);

        // Kun deltager 1 er Reserved â€” deltager 2 mangler
        _paymentRepo.Add(new ParticipantPayment
        {
            OrderId = 10, ParticipantId = 1,
            Status = ParticipantPaymentStatus.Reserved
        });

        await orderService.CheckAndSetReadyToPayByReservedAsync(10);

        order.Status.Should().Be("Collecting", "ReadyToPay krÃ¦ver at alle participants er Reserved");
    }

    [Fact]
    public async Task ReadyToPay_Is_Set_When_All_Participants_Are_Reserved()
    {
        var (orderService, _) = CreateServices();

        var order = MakeOrderTwoParticipants(10, 1, 2);
        _orderRepo.Add(order);

        _paymentRepo.Add(new ParticipantPayment
        {
            OrderId = 10, ParticipantId = 1,
            Status = ParticipantPaymentStatus.Reserved
        });
        _paymentRepo.Add(new ParticipantPayment
        {
            OrderId = 10, ParticipantId = 2,
            Status = ParticipantPaymentStatus.Reserved
        });

        await orderService.CheckAndSetReadyToPayByReservedAsync(10);

        order.Status.Should().Be("ReadyToPay");
        order.Messages.Should().NotBeEmpty("host skal have besked om at alle er klar");
    }

    [Fact]
    public async Task ReadyToPay_Is_Not_Set_When_Order_Already_Beyond_Collecting()
    {
        var (orderService, _) = CreateServices();

        var order = MakeOrderTwoParticipants(10, 1, 2, status: "ReadyToPay");
        _orderRepo.Add(order);

        _paymentRepo.Add(new ParticipantPayment { OrderId = 10, ParticipantId = 1, Status = ParticipantPaymentStatus.Reserved });
        _paymentRepo.Add(new ParticipantPayment { OrderId = 10, ParticipantId = 2, Status = ParticipantPaymentStatus.Reserved });

        await orderService.CheckAndSetReadyToPayByReservedAsync(10);

        order.Status.Should().Be("ReadyToPay", "status Ã¦ndres ikke hvis allerede ReadyToPay");
        order.Messages.Should().BeEmpty("ingen ekstra besked sendes");
    }

    [Fact]
    public async Task ReadyToPay_Not_Set_When_One_Participant_Has_ReservationStarted_Not_Reserved()
    {
        var (orderService, _) = CreateServices();

        var order = MakeOrderTwoParticipants(10, 1, 2);
        _orderRepo.Add(order);

        _paymentRepo.Add(new ParticipantPayment { OrderId = 10, ParticipantId = 1, Status = ParticipantPaymentStatus.Reserved });
        _paymentRepo.Add(new ParticipantPayment { OrderId = 10, ParticipantId = 2, Status = ParticipantPaymentStatus.ReservationStarted });

        await orderService.CheckAndSetReadyToPayByReservedAsync(10);

        order.Status.Should().Be("Collecting", "ReservationStarted er ikke Reserved");
    }

    [Fact]
    public async Task Merchant_Participant_Is_Excluded_From_Reserved_Check()
    {
        var (orderService, _) = CreateServices();

        // Ordre med 1 person + 1 merchant deltager
        var order = new Order
        {
            Id = 10,
            CreatedByParticipantId = 1,
            Title = "Testordre",
            Status = "Collecting",
            Messages = [],
            OrderParticipants =
            [
                new OrderParticipant
                {
                    ParticipantId = 1,
                    Status = "OrderSubmitted",
                    Participant = new Participant { Id = 1, Name = "Host", Type = ParticipantType.Person }
                },
                new OrderParticipant
                {
                    ParticipantId = 99,
                    Status = "Invited",
                    Participant = new Participant { Id = 99, Name = "Roma", Type = ParticipantType.Merchant }
                }
            ]
        };
        _orderRepo.Add(order);

        // Kun person-deltager er Reserved â€” merchant tÃ¦lles ikke
        _paymentRepo.Add(new ParticipantPayment
        {
            OrderId = 10, ParticipantId = 1,
            Status = ParticipantPaymentStatus.Reserved
        });

        await orderService.CheckAndSetReadyToPayByReservedAsync(10);

        order.Status.Should().Be("ReadyToPay", "merchant tÃ¦ller ikke â€” kun person-deltager skal vÃ¦re Reserved");
    }
}

