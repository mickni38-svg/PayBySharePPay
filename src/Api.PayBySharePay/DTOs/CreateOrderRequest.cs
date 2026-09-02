using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class CreateOrderRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CreatedByParticipantId { get; set; }

    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public string? Category { get; set; }

    [StringLength(500)]
    public string? Message { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? MerchantParticipantId { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> ParticipantIds { get; set; } = new();

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string IdempotencyKey { get; set; } = string.Empty;
}
