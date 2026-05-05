using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class CreateOrderRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public List<int> ParticipantIds { get; set; } = new();
}
