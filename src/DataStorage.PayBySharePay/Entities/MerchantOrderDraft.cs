namespace DataStorage.PayBySharePay.Entities;

public class MerchantOrderDraft
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int MerchantParticipantId { get; set; }
    public Participant MerchantParticipant { get; set; } = null!;

    /// <summary>Merchantens eget interne ordre-id/reference</summary>
    public string MerchantDraftReference { get; set; } = string.Empty;

    public decimal SubtotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "DKK";

    /// <summary>AuthorizeThenCapture eller ManualCapture</summary>
    public string PaymentMode { get; set; } = "AuthorizeThenCapture";

    /// <summary>Draft / Collecting / AllAuthorized / Released / Expired</summary>
    public string Status { get; set; } = "Draft";

    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<MerchantOrderLine> Lines { get; set; } = new List<MerchantOrderLine>();
}
