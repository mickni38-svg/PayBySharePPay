using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class CreateOrderRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CreatedByParticipantId { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public List<int> ParticipantIds { get; set; } = new();
}
