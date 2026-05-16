using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class CompleteOrderRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int RequestingParticipantId { get; set; }
}
