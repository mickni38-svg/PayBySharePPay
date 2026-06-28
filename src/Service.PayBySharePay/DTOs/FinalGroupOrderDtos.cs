namespace Service.PayBySharePay.DTOs;

/// <summary>
/// Standard "GroupOrderPaid" payload som PayNSync sender til merchant efter fuld capture.
/// Merchant mapper dette til sit eget system — ingen merchant-specifikke adapters i v1.
/// </summary>
public sealed class PayNSyncFinalGroupOrderDto
{
    public string EventType { get; init; } = "GroupOrderPaid";
    public int PaynsyncOrderId { get; init; }
    public int? MerchantId { get; init; }
    public string Status { get; init; } = "Paid";
    public string Currency { get; init; } = "DKK";
    public decimal TotalAmount { get; init; }
    public DateTime PaidAtUtc { get; init; }
    public List<PayNSyncFinalParticipantOrderDto> Participants { get; init; } = [];
}

public sealed class PayNSyncFinalParticipantOrderDto
{
    public int ParticipantId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    /// <summary>Beløb i kr (decimal), ikke øre.</summary>
    public decimal Amount { get; init; }
    public string PaymentStatus { get; init; } = string.Empty;
    public string? ProviderPaymentId { get; init; }
    public string? MerchantDraftId { get; init; }
    public List<PayNSyncFinalOrderLineDto> Lines { get; init; } = [];
}

public sealed class PayNSyncFinalOrderLineDto
{
    public string? Sku { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}
