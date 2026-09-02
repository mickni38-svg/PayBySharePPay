using System.ComponentModel.DataAnnotations;

namespace Api.PayBySharePay.DTOs;

public class CompleteOrderRequest
{
    /// <summary>Legacy-kompatibilitet. Ignoreres ved autorisation; brugeridentiteten hentes fra JWT.</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int RequestingParticipantId { get; set; }
}
