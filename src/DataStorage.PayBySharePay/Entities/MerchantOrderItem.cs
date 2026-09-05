namespace DataStorage.PayBySharePay.Entities;

/// <summary>
/// One immutable line in a final merchant order. Lines are copied one-to-one from the drafts;
/// they are deliberately not linked to participants or payment-provider records.
/// </summary>
public sealed class MerchantOrderItem
{
    public int Id { get; set; }

    public int MerchantOrderId { get; set; }
    public MerchantOrder MerchantOrder { get; set; } = null!;

    public string? Sku { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
