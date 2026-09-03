namespace Service.PayBySharePay.DTOs;

public class ParticipantDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? CompanyName { get; set; }
    /// <remarks>Udfyldes kun internt ved login-validering – eksponeres aldrig i API-svar.</remarks>
    public string? PasswordHash { get; set; }
}
