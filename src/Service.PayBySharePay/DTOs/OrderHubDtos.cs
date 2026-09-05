namespace Service.PayBySharePay.DTOs;

public sealed class OrderHubSettingsDto
{
    public bool Enabled { get; init; }
}

public sealed class OrderHubOrderDto
{
    public int Id { get; init; }
    public int SourceOrderId { get; init; }
    public string PayNSyncOrderNumber { get; init; } = string.Empty;
    public string Status { get; init; } = "New";
    public string PaymentStatus { get; init; } = "Paid";
    public string Currency { get; init; } = "DKK";
    public decimal TotalAmount { get; init; }
    public DateTime PaidAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
    public string HostName { get; init; } = string.Empty;
    public string? HostPhone { get; init; }
    public string? Note { get; init; }
    public OrderHubDeliveryAddressDto? DeliveryAddress { get; init; }
    public List<OrderHubOrderItemDto> Items { get; init; } = [];
}

public sealed class OrderHubDeliveryAddressDto
{
    public string? Address { get; init; }
    public string? PostalCode { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
}

public sealed class OrderHubOrderItemDto
{
    public string? Sku { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
    public string? ModifiersJson { get; init; }
}
