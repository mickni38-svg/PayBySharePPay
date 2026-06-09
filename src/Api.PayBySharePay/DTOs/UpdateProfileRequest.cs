namespace Api.PayBySharePay.DTOs;

public class UpdateProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
