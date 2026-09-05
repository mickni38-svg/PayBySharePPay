using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

public sealed class SquareInspiredMerchantOrderAdapter : ISquareInspiredMerchantOrderAdapter
{
    public SquareInspiredMerchantOrderRequest Map(PayNSyncFinalGroupOrderDto order)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new SquareInspiredMerchantOrderRequest
        {
            IdempotencyKey = $"paynsync-{order.PaynsyncOrderId}",
            ReferenceId = order.PaynsyncOrderNumber,
            Customer = new SquareInspiredCustomerDto
            {
                DisplayName = order.Host.Name,
                PhoneNumber = order.Host.Phone
            },
            Fulfillment = new SquareInspiredFulfillmentDto
            {
                Type = order.DeliveryAddress is null ? "PICKUP" : "DELIVERY",
                DeliveryAddress = order.DeliveryAddress is null ? null : new SquareInspiredAddressDto
                {
                    AddressLine1 = order.DeliveryAddress.Address,
                    PostalCode = order.DeliveryAddress.PostalCode,
                    Locality = order.DeliveryAddress.City,
                    Country = order.DeliveryAddress.Country
                }
            },
            TotalMoney = Money(order.TotalAmount, order.Currency),
            LineItems = order.Lines.Select(line =>
            {
                var modifierUnitTotal = line.Modifiers.Sum(modifier => modifier.Price);
                var baseUnitPrice = line.UnitPrice - modifierUnitTotal;

                return new SquareInspiredLineItemDto
                {
                    CatalogObjectId = line.Sku,
                    Name = line.Name,
                    Quantity = line.Quantity,
                    BasePriceMoney = Money(baseUnitPrice, order.Currency),
                    TotalMoney = Money(line.LineTotal, order.Currency),
                Modifiers = line.Modifiers.Select(modifier => new SquareInspiredModifierDto
                {
                    CatalogObjectId = modifier.Id,
                    Name = modifier.Name,
                    BasePriceMoney = Money(modifier.Price, order.Currency)
                    }).ToList()
                };
            }).ToList()
        };
    }

    private static SquareInspiredMoneyDto Money(decimal amount, string currency)
        => new()
        {
            Amount = checked((long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero)),
            Currency = currency.Trim().ToUpperInvariant()
        };
}
