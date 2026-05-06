namespace DataStorage.PayBySharePay.Entities;

public class MerchantOrderLine
{
    public int Id { get; set; }

    public int MerchantOrderDraftId { get; set; }
    public MerchantOrderDraft MerchantOrderDraft { get; set; } = null!;

    /// <summary>Merchantens eget linje-id</summary>
    public string LineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
