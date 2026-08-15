namespace Service.PayBySharePay.DTOs;

public class UpdateMerchantLogoDto
{
    public byte[] ImageData { get; set; } = [];
    public string ContentType { get; set; } = string.Empty;
    public string? FileName { get; set; }
}
