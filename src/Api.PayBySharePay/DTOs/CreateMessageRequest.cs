using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class CreateMessageRequest
{
    [Required]
    public int OrderId { get; set; }
    [Required]
    public int ParticipantId { get; set; }
    [Required]
    public string Content { get; set; } = string.Empty;
}
