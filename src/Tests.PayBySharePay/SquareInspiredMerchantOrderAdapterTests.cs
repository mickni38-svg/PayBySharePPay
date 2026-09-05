using FluentAssertions;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Services;

namespace Tests.PayBySharePay;

public sealed class SquareInspiredMerchantOrderAdapterTests
{
    [Fact]
    public void Map_Maps_Products_Prices_Modifiers_Host_And_Delivery()
    {
        var sut = new SquareInspiredMerchantOrderAdapter();
        var source = new PayNSyncFinalGroupOrderDto
        {
            PaynsyncOrderId = 42,
            PaynsyncOrderNumber = "PNS-00000042",
            MerchantId = 99,
            Currency = "DKK",
            TotalAmount = 124m,
            PaidAtUtc = DateTime.UtcNow,
            Host = new PayNSyncHostDto { Name = "Michael", Phone = "20112233" },
            DeliveryAddress = new PayNSyncDeliveryAddressDto
            {
                Address = "Hovedgaden 1",
                PostalCode = "4000",
                City = "Roskilde",
                Country = "DK"
            },
            Lines =
            [
                new PayNSyncFinalOrderLineDto
                {
                    Sku = "pizza-1",
                    Name = "Pizza",
                    Quantity = 1,
                    UnitPrice = 124m,
                    LineTotal = 124m,
                    Modifiers =
                    [
                        new PayNSyncFinalModifierDto { Id = "extra-cheese", Name = "Ekstra ost", Price = 15m }
                    ]
                }
            ]
        };

        var result = sut.Map(source);

        result.IdempotencyKey.Should().Be("paynsync-42");
        result.ReferenceId.Should().Be("PNS-00000042");
        result.Customer.DisplayName.Should().Be("Michael");
        result.Customer.PhoneNumber.Should().Be("20112233");
        result.Fulfillment.Type.Should().Be("DELIVERY");
        result.Fulfillment.DeliveryAddress!.Locality.Should().Be("Roskilde");
        result.TotalMoney.Amount.Should().Be(12400);
        result.TotalMoney.Currency.Should().Be("DKK");
        result.LineItems.Should().ContainSingle();
        result.LineItems[0].CatalogObjectId.Should().Be("pizza-1");
        result.LineItems[0].BasePriceMoney.Amount.Should().Be(10900, "line unit price already includes the 15 kr modifier");
        result.LineItems[0].Modifiers.Should().ContainSingle();
        result.LineItems[0].Modifiers[0].CatalogObjectId.Should().Be("extra-cheese");
        result.LineItems[0].Modifiers[0].BasePriceMoney.Amount.Should().Be(1500);
    }

    [Fact]
    public void Map_Uses_Pickup_When_No_Delivery_Address()
    {
        var sut = new SquareInspiredMerchantOrderAdapter();

        var result = sut.Map(new PayNSyncFinalGroupOrderDto
        {
            PaynsyncOrderId = 1,
            PaynsyncOrderNumber = "PNS-00000001",
            Currency = "DKK",
            TotalAmount = 50m,
            Host = new PayNSyncHostDto { Name = "Host" },
            Lines =
            [
                new PayNSyncFinalOrderLineDto
                {
                    Name = "Vare",
                    Quantity = 1,
                    UnitPrice = 50m,
                    LineTotal = 50m
                }
            ]
        });

        result.Fulfillment.Type.Should().Be("PICKUP");
        result.Fulfillment.DeliveryAddress.Should().BeNull();
    }
}
