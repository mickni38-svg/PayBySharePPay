using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class RegisterPersonRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
}

public class RegisterMerchantRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string CompanyName { get; set; } = string.Empty;

    public string? CvrNumber { get; set; }
    public string? ContactPerson { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }
    public string? CompanyAddress { get; set; }
}
