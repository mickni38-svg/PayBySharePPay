namespace Service.PayBySharePay.DTOs;

public class MerchantOrderDraftDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int MerchantParticipantId { get; set; }
    public string MerchantDraftReference { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<MerchantOrderLineDto> Lines { get; set; } = new();
}
