namespace Api.PayBySharePay.DTOs;

/// <summary>Request fra Host for at godkende ordren og igangsætte capture af alle reserverede betalinger.</summary>
public sealed class ApproveOrderRequest
{
    /// <summary>Participantid på den bruger der klikker "Godkend ordre" — skal være host.</summary>
    public int RequestingParticipantId { get; set; }
}
