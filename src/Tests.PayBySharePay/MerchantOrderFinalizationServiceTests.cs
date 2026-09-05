using DataStorage.PayBySharePay.Entities;
using DataStorage.PayBySharePay.Repositories;
using FluentAssertions;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

public sealed class MerchantOrderFinalizationServiceTests
{
    private sealed class FakeMerchantOrderRepository : IMerchantOrderRepository
    {
        private readonly List<MerchantOrder> _orders = [];

        public IReadOnlyList<MerchantOrder> Orders => _orders;

        public Task<MerchantOrder?> GetBySourceOrderIdAsync(
            int sourceOrderId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.SingleOrDefault(order => order.SourceOrderId == sourceOrderId));

        public Task<MerchantOrder> AddAsync(
            MerchantOrder merchantOrder,
            CancellationToken cancellationToken = default)
        {
            merchantOrder.Id = _orders.Count + 1;
            _orders.Add(merchantOrder);
            return Task.FromResult(merchantOrder);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task EnsureFinalized_Creates_One_Flat_PrivacySafe_MerchantOrder()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        var paidAtUtc = new DateTime(2026, 9, 5, 12, 30, 0, DateTimeKind.Utc);

        var result = await sut.EnsureFinalizedAsync(order, payments, paidAtUtc);

        repository.Orders.Should().ContainSingle();
        var saved = repository.Orders.Single();
        saved.SourceOrderId.Should().Be(42);
        saved.PayNSyncOrderNumber.Should().Be("PNS-00000042");
        saved.MerchantParticipantId.Should().Be(99);
        saved.HostName.Should().Be("Michael");
        saved.HostPhone.Should().Be("20112233");
        saved.DeliveryAddress.Should().Be("Hovedgaden 1");
        saved.DeliveryPostalCode.Should().Be("4000");
        saved.DeliveryCity.Should().Be("Roskilde");
        saved.DeliveryCountry.Should().Be("DK");
        saved.TotalAmount.Should().Be(210m);
        saved.Currency.Should().Be("DKK");
        saved.PaymentStatus.Should().Be("Paid");
        saved.PaidAtUtc.Should().Be(paidAtUtc);
        saved.Items.Should().HaveCount(3);

        result.Lines.Should().HaveCount(3);
        result.Lines[0].Modifiers.Should().ContainSingle();
        result.Lines[0].Modifiers[0].Id.Should().Be("extra-cheese");
        result.Lines[0].Modifiers[0].Name.Should().Be("Ekstra ost");
        result.TotalAmount.Should().Be(210m);
        result.Host.Name.Should().Be("Michael");
        result.Host.Phone.Should().Be("20112233");
        result.DeliveryAddress!.City.Should().Be("Roskilde");

        typeof(Service.PayBySharePay.DTOs.PayNSyncFinalGroupOrderDto).GetProperties()
            .Select(property => property.Name)
            .Should().NotContain("Participants")
            .And.NotContain("ParticipantId")
            .And.NotContain("ProviderPaymentId")
            .And.NotContain("ProviderReference");

        typeof(MerchantOrder).GetProperties().Select(property => property.Name)
            .Should().NotContain("ParticipantId")
            .And.NotContain("ParticipantName")
            .And.NotContain("ProviderPaymentId")
            .And.NotContain("ProviderReference");
        typeof(MerchantOrderItem).GetProperties().Select(property => property.Name)
            .Should().NotContain("ParticipantId")
            .And.NotContain("ParticipantName")
            .And.NotContain("ProviderPaymentId")
            .And.NotContain("ProviderReference");
    }

    [Fact]
    public async Task EnsureFinalized_Is_Idempotent_And_Preserves_Original_Snapshot()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        var originalPaidAt = new DateTime(2026, 9, 5, 12, 30, 0, DateTimeKind.Utc);

        var first = await sut.EnsureFinalizedAsync(order, payments, originalPaidAt);
        order.OrderParticipants.Single(participant => participant.ParticipantId == 1).Participant.Name = "Nyt navn";
        order.DeliveryAddress = "Ny adresse 2";
        var second = await sut.EnsureFinalizedAsync(order, payments, originalPaidAt.AddHours(1));

        repository.Orders.Should().ContainSingle();
        second.PaynsyncOrderNumber.Should().Be(first.PaynsyncOrderNumber);
        second.Host.Name.Should().Be("Michael");
        second.DeliveryAddress!.Address.Should().Be("Hovedgaden 1");
        second.PaidAtUtc.Should().Be(originalPaidAt);
    }

    [Fact]
    public async Task EnsureFinalized_Rejects_When_A_Participant_Is_Not_Captured()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        payments.Single(payment => payment.ParticipantId == 2).Status = ParticipantPaymentStatus.Reserved;

        var act = () => sut.EnsureFinalizedAsync(order, payments, DateTime.UtcNow);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Alle deltagere*");
        repository.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task EnsureFinalized_Rejects_When_Line_Total_Differs_From_Captured_Total()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        order.MerchantOrderDrafts.First().Lines.First().LineTotal += 1m;

        var act = () => sut.EnsureFinalizedAsync(order, payments, DateTime.UtcNow);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*stemmer ikke med betalingsbeløbet*");
        repository.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task EnsureFinalized_Rejects_Mixed_Currencies()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        payments.Single(payment => payment.ParticipantId == 2).Currency = "EUR";

        var act = () => sut.EnsureFinalizedAsync(order, payments, DateTime.UtcNow);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*samme valuta*");
        repository.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_Rejects_Draft_With_Different_Merchant_Before_Capture()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        order.Status = "ReadyToPay";
        payments.ForEach(payment => payment.Status = ParticipantPaymentStatus.Reserved);
        order.MerchantOrderDrafts.First().MerchantParticipantId = 1234;

        var act = () => sut.ValidateAsync(order, payments);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*merchant-draft*");
        repository.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_Rejects_Draft_With_Different_Currency_Before_Capture()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        order.Status = "ReadyToPay";
        payments.ForEach(payment => payment.Status = ParticipantPaymentStatus.Reserved);
        order.MerchantOrderDrafts.First().Currency = "EUR";

        var act = () => sut.ValidateAsync(order, payments);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*samme valuta*");
        repository.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task EnsureFinalized_Preserves_Separate_Source_Lines()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        var firstDraft = order.MerchantOrderDrafts.First();
        firstDraft.Lines =
        [
            new MerchantOrderLine { LineId = "pizza-extra-cheese", Name = "Pizza + ekstra ost", Quantity = 1, UnitPrice = 60m, LineTotal = 60m },
            new MerchantOrderLine { LineId = "pizza-no-cheese", Name = "Pizza uden ost", Quantity = 1, UnitPrice = 60m, LineTotal = 60m }
        ];

        var result = await sut.EnsureFinalizedAsync(order, payments, DateTime.UtcNow);

        result.Lines.Should().HaveCount(3);
        result.Lines.Take(2).Select(line => line.Sku)
            .Should().Equal("pizza-extra-cheese", "pizza-no-cheese");
    }


    [Fact]
    public async Task EnsureFinalized_Preserves_Structured_Modifiers_From_Raw_Merchant_Payload()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        var draft = order.MerchantOrderDrafts.First();
        draft.RawMerchantPayloadJson = """
        {"items":[
          {"productId":"pizza","quantity":1,"unitPrice":90,"lineTotal":90,
           "modifiers":[{"id":"extra-cheese","name":"Ekstra ost","price":15}]},
          {"productId":"cola","quantity":1,"unitPrice":30,"lineTotal":30,"modifiers":[]}
        ]}
        """;

        var result = await sut.EnsureFinalizedAsync(order, payments, DateTime.UtcNow);

        result.Lines[0].Modifiers.Should().ContainSingle();
        result.Lines[0].Modifiers[0].Id.Should().Be("extra-cheese");
        result.Lines[0].Modifiers[0].Name.Should().Be("Ekstra ost");
        repository.Orders.Single().Items.First().ModifiersJson.Should().Contain("extra-cheese");
    }

    [Fact]
    public async Task RecordExternalDelivery_Persists_Result_And_Does_Not_Overwrite_External_Order()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        await sut.EnsureFinalizedAsync(order, payments, DateTime.UtcNow);

        await sut.RecordExternalDeliveryAsync(42,
            new Service.PayBySharePay.DTOs.MerchantOrderDeliveryResultDto(true, "EXT-1", "{\"orderId\":\"EXT-1\"}"));
        await sut.RecordExternalDeliveryAsync(42,
            new Service.PayBySharePay.DTOs.MerchantOrderDeliveryResultDto(true, "EXT-2", "{\"orderId\":\"EXT-2\"}"));

        var saved = repository.Orders.Single();
        saved.ExternalOrderNumber.Should().Be("EXT-1");
        saved.ExternalResponseJson.Should().Contain("EXT-1");
    }

    [Fact]
    public async Task RecordExternalDelivery_Persists_Response_And_External_Order_Number_Idempotently()
    {
        var repository = new FakeMerchantOrderRepository();
        var sut = new MerchantOrderFinalizationService(repository);
        var (order, payments) = CreatePaidOrder();
        await sut.EnsureFinalizedAsync(order, payments, DateTime.UtcNow);

        await sut.RecordExternalDeliveryAsync(order.Id,
            new Service.PayBySharePay.DTOs.MerchantOrderDeliveryResultDto(true, "EXT-123", "{\"orderId\":\"EXT-123\"}"));
        await sut.RecordExternalDeliveryAsync(order.Id,
            new Service.PayBySharePay.DTOs.MerchantOrderDeliveryResultDto(true, "EXT-999", "{\"orderId\":\"EXT-999\"}"));

        var saved = repository.Orders.Single();
        saved.ExternalOrderNumber.Should().Be("EXT-123");
        saved.ExternalResponseJson.Should().Be("{\"orderId\":\"EXT-123\"}");
    }

    private static (Order Order, List<ParticipantPayment> Payments) CreatePaidOrder()
    {
        var host = new Participant
        {
            Id = 1,
            Type = ParticipantType.Person,
            Name = "Michael",
            Phone = "20112233"
        };
        var participant = new Participant
        {
            Id = 2,
            Type = ParticipantType.Person,
            Name = "Anna"
        };
        var merchant = new Participant
        {
            Id = 99,
            Type = ParticipantType.Merchant,
            Name = "Pizzeria Roma"
        };

        var order = new Order
        {
            Id = 42,
            CreatedByParticipantId = host.Id,
            CreatedBy = host,
            MerchantParticipantId = merchant.Id,
            MerchantParticipant = merchant,
            Status = "Paid",
            DeliveryAddress = "Hovedgaden 1",
            DeliveryPostalCode = "4000",
            DeliveryCity = "Roskilde",
            DeliveryCountry = "DK",
            OrderParticipants =
            [
                new OrderParticipant { ParticipantId = host.Id, Participant = host, Status = "OrderSubmitted" },
                new OrderParticipant { ParticipantId = participant.Id, Participant = participant, Status = "OrderSubmitted" }
            ],
            MerchantOrderDrafts =
            [
                new MerchantOrderDraft
                {
                    ParticipantId = host.Id,
                    MerchantParticipantId = merchant.Id,
                    TotalAmount = 120m,
                    Currency = "DKK",
                    RawMerchantPayloadJson = """
                    {"items":[
                      {"productId":"pizza","quantity":1,"unitPrice":90,"lineTotal":90,"modifiers":[{"id":"extra-cheese","name":"Ekstra ost","price":15}]},
                      {"productId":"cola","quantity":1,"unitPrice":30,"lineTotal":30,"modifiers":[]}
                    ]}
                    """,
                    Lines =
                    [
                        new MerchantOrderLine { LineId = "pizza", Name = "Pizza", Quantity = 1, UnitPrice = 90m, LineTotal = 90m },
                        new MerchantOrderLine { LineId = "cola", Name = "Cola", Quantity = 1, UnitPrice = 30m, LineTotal = 30m }
                    ]
                },
                new MerchantOrderDraft
                {
                    ParticipantId = participant.Id,
                    MerchantParticipantId = merchant.Id,
                    TotalAmount = 90m,
                    Currency = "DKK",
                    Lines =
                    [
                        new MerchantOrderLine { LineId = "pasta", Name = "Pasta", Quantity = 1, UnitPrice = 90m, LineTotal = 90m }
                    ]
                }
            ]
        };

        var payments = new List<ParticipantPayment>
        {
            new()
            {
                OrderId = order.Id,
                ParticipantId = host.Id,
                AmountMinorUnits = 12000,
                Currency = "DKK",
                Status = ParticipantPaymentStatus.Captured,
                ProviderPaymentId = "provider-host",
                CapturedAtUtc = DateTime.UtcNow
            },
            new()
            {
                OrderId = order.Id,
                ParticipantId = participant.Id,
                AmountMinorUnits = 9000,
                Currency = "DKK",
                Status = ParticipantPaymentStatus.Captured,
                ProviderPaymentId = "provider-participant",
                CapturedAtUtc = DateTime.UtcNow
            }
        };

        return (order, payments);
    }
}
