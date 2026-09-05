namespace Service.PayBySharePay.DTOs;

public sealed class SquareInspiredMerchantOrderRequest
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public string ReferenceId { get; init; } = string.Empty;
    public SquareInspiredCustomerDto Customer { get; init; } = new();
    public SquareInspiredFulfillmentDto Fulfillment { get; init; } = new();
    public SquareInspiredMoneyDto TotalMoney { get; init; } = new();
    public List<SquareInspiredLineItemDto> LineItems { get; init; } = [];
}

public sealed class SquareInspiredCustomerDto
{
    public string DisplayName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

public sealed class SquareInspiredFulfillmentDto
{
    public string Type { get; init; } = "PICKUP";
    public SquareInspiredAddressDto? DeliveryAddress { get; init; }
}

public sealed class SquareInspiredAddressDto
{
    public string? AddressLine1 { get; init; }
    public string? PostalCode { get; init; }
    public string? Locality { get; init; }
    public string? Country { get; init; }
}

public sealed class SquareInspiredMoneyDto
{
    public long Amount { get; init; }
    public string Currency { get; init; } = "DKK";
}

public sealed class SquareInspiredLineItemDto
{
    public string? CatalogObjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public SquareInspiredMoneyDto BasePriceMoney { get; init; } = new();
    public SquareInspiredMoneyDto TotalMoney { get; init; } = new();
    public List<SquareInspiredModifierDto> Modifiers { get; init; } = [];
}

public sealed class SquareInspiredModifierDto
{
    public string? CatalogObjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public SquareInspiredMoneyDto BasePriceMoney { get; init; } = new();
}

public sealed class SquareInspiredMerchantOrderResponse
{
    public string OrderId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string ReferenceId { get; init; } = string.Empty;
}

public sealed record MerchantOrderDeliveryResultDto(
    bool Success,
    string? ExternalOrderNumber,
    string? ResponseBody,
    string? ErrorMessage = null);
