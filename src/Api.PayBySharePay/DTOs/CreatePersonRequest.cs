using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class CreatePersonRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
