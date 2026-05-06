using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class LoginRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int ParticipantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
