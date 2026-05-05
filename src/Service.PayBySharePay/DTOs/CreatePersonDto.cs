namespace Service.PayBySharePay.DTOs;

public class CreatePersonDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
