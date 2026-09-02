using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class LoginRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ParticipantType { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
