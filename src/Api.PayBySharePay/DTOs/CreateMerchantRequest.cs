using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class CreateMerchantRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string CompanyName { get; set; } = string.Empty;
    public string? CvrNumber { get; set; }
    public string? VatNumber { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? CompanyAddress { get; set; }
    public string? PaymentReference { get; set; }
    public string? PayoutAccountInfo { get; set; }
    public string? PaymentProvider { get; set; }
}
