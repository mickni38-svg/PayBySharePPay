namespace Api.PayBySharePay.DTOs;

/// <summary>Request fra Host for at annullere ordren og alle ikke-capturede reservationer.</summary>
public sealed class CancelOrderRequest
{
    /// <summary>Legacy-kompatibilitet. Ignoreres ved autorisation; brugeridentiteten hentes fra JWT.</summary>
    public int RequestingParticipantId { get; set; }
}
