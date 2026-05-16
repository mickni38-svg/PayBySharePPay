namespace Service.PayBySharePay.DTOs;

public class InitMerchantOrderDto
{
    public int OrderId { get; set; }
    public int MerchantParticipantId { get; set; }
    public string ParticipantToken { get; set; } = string.Empty;
    public string MerchantDraftReference { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "DKK";
    public string PaymentMode { get; set; } = "AuthorizeThenCapture";
    public DateTime? ExpiresAtUtc { get; set; }
    public List<MerchantOrderLineDto> Lines { get; set; } = new();
}
