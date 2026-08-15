namespace Service.PayBySharePay.DTOs;

public class MerchantLogoDto
{
    public byte[] ImageData { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
