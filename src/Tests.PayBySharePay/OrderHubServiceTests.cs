using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

public sealed class OrderHubServiceTests
{
    [Fact]
    public async Task GetActiveOrders_Returns_Only_Authenticated_Merchant_Orders()
    {
        var merchantRepo = new FakeMerchantOrderRepository(
            Order(1, 10, "New"),
            Order(2, 20, "New"),
            Order(3, 10, "Completed"));
        var participantRepo = new FakeParticipantRepository(
            new Participant { Id = 10, Type = ParticipantType.Merchant, Name = "Roma", OrderHubEnabled = true });

        var sut = new OrderHubService(merchantRepo, participantRepo);

        var result = await sut.GetActiveOrdersAsync(10);

        result.Select(order => order.Id).Should().Equal(1);
    }

    [Fact]
    public async Task GetOrders_Rejects_When_OrderHub_Is_Disabled()
    {
        var sut = new OrderHubService(
            new FakeMerchantOrderRepository(),
            new FakeParticipantRepository(
                new Participant { Id = 10, Type = ParticipantType.Merchant, Name = "Roma", OrderHubEnabled = false }));

        var act = () => sut.GetActiveOrdersAsync(10);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*ikke aktiveret*");
    }

    [Fact]
    public async Task UpdateStatus_Allows_Only_Next_Status()
    {
        var merchantRepo = new FakeMerchantOrderRepository(Order(1, 10, "New"));
        var sut = new OrderHubService(
            merchantRepo,
            new FakeParticipantRepository(
                new Participant { Id = 10, Type = ParticipantType.Merchant, Name = "Roma", OrderHubEnabled = true }));

        var accepted = await sut.UpdateStatusAsync(10, 1, "Accepted");

        accepted.Status.Should().Be("Accepted");
        var act = () => sut.UpdateStatusAsync(10, 1, "Ready");
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateStatus_Rejects_Order_From_Another_Merchant()
    {
        var sut = new OrderHubService(
            new FakeMerchantOrderRepository(Order(1, 20, "New")),
            new FakeParticipantRepository(
                new Participant { Id = 10, Type = ParticipantType.Merchant, Name = "Roma", OrderHubEnabled = true }));

        var act = () => sut.UpdateStatusAsync(10, 1, "Accepted");

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task SetEnabled_Persists_Manual_Access_Change()
    {
        var merchant = new Participant { Id = 10, Type = ParticipantType.Merchant, Name = "Roma" };
        var participants = new FakeParticipantRepository(merchant);
        var sut = new OrderHubService(new FakeMerchantOrderRepository(), participants);

        var result = await sut.SetEnabledAsync(10, true);

        result.Enabled.Should().BeTrue();
        merchant.OrderHubEnabled.Should().BeTrue();
        participants.SaveCount.Should().Be(1);
    }

    private static MerchantOrder Order(int id, int merchantId, string status)
        => new()
        {
            Id = id,
            SourceOrderId = 100 + id,
            MerchantParticipantId = merchantId,
            PayNSyncOrderNumber = $"PNS-{id:00000000}",
            HostName = "Host",
            TotalAmount = 100m,
            Currency = "DKK",
            PaymentStatus = "Paid",
            OrderHubStatus = status,
            PaidAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

    private sealed class FakeMerchantOrderRepository(params MerchantOrder[] seed) : IMerchantOrderRepository
    {
        private readonly List<MerchantOrder> orders = [.. seed];

        public Task<MerchantOrder?> GetBySourceOrderIdAsync(int sourceOrderId, CancellationToken cancellationToken = default)
            => Task.FromResult(orders.SingleOrDefault(order => order.SourceOrderId == sourceOrderId));

        public Task<MerchantOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(orders.SingleOrDefault(order => order.Id == id));

        public Task<IReadOnlyList<MerchantOrder>> GetByMerchantAsync(int merchantParticipantId, bool completed, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<MerchantOrder>>(orders
                .Where(order => order.MerchantParticipantId == merchantParticipantId)
                .Where(order => completed ? order.OrderHubStatus == "Completed" : order.OrderHubStatus != "Completed")
                .OrderByDescending(order => order.CreatedAtUtc)
                .ToList());

        public Task<MerchantOrder> AddAsync(MerchantOrder merchantOrder, CancellationToken cancellationToken = default)
        {
            orders.Add(merchantOrder);
            return Task.FromResult(merchantOrder);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeParticipantRepository(params Participant[] seed) : IParticipantRepository
    {
        private readonly List<Participant> participants = [.. seed];
        public int SaveCount { get; private set; }

        public Task<IEnumerable<Participant>> SearchAsync(string query, int? excludeFriendsOf = null)
            => Task.FromResult(Enumerable.Empty<Participant>());

        public Task<IEnumerable<Participant>> GetAllPersonsAsync()
            => Task.FromResult(Enumerable.Empty<Participant>());

        public Task<Participant?> GetByIdAsync(int id)
            => Task.FromResult(participants.SingleOrDefault(participant => participant.Id == id));

        public Task<Participant?> GetByEmailAsync(string email)
            => Task.FromResult(participants.SingleOrDefault(participant => participant.Email == email));

        public Task<Participant> AddAsync(Participant participant)
        {
            participants.Add(participant);
            return Task.FromResult(participant);
        }

        public Task UpdateAsync(Participant participant) => Task.CompletedTask;

        public Task SaveChangesAsync()
        {
            SaveCount++;
            return Task.CompletedTask;
        }
    }
}
