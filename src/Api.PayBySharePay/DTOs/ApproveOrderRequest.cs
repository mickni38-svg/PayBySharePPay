namespace Api.PayBySharePay.DTOs;

/// <summary>Request fra Host for at godkende ordren og igangsætte capture af alle reserverede betalinger.</summary>
public sealed class ApproveOrderRequest
{
    /// <summary>Legacy-kompatibilitet. Ignoreres ved autorisation; brugeridentiteten hentes fra JWT.</summary>
    public int RequestingParticipantId { get; set; }
}
