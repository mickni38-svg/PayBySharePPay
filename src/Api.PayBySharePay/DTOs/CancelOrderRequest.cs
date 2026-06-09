namespace Api.PayBySharePay.DTOs;

/// <summary>Request fra Host for at annullere ordren og alle ikke-capturede reservationer.</summary>
public sealed class CancelOrderRequest
{
    public int RequestingParticipantId { get; set; }
}
