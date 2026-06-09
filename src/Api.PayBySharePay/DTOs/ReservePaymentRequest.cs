namespace Api.PayBySharePay.DTOs;

/// <summary>
/// Request fra deltager (eller merchant på vegne af deltager) for at starte betalingsreservation.
/// </summary>
public sealed class ReservePaymentRequest
{
    /// <summary>Participantid på den deltager der reserverer betaling.</summary>
    public int ParticipantId { get; set; }

    /// <summary>Beløb i minor units (øre). Fx 12000 = 120,00 DKK.</summary>
    public long AmountMinorUnits { get; set; }

    /// <summary>Valuta. Default: DKK.</summary>
    public string Currency { get; set; } = "DKK";

    /// <summary>MerchantId til betalingsudbyderen — valgfri.</summary>
    public string? MerchantId { get; set; }

    /// <summary>URL som betalingsudbyderen redirecter til efter deltager godkender/afviser.</summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>URL som betalingsudbyderen kalder (webhook) ved statusopdatering.</summary>
    public string CallbackUrl { get; set; } = string.Empty;
}
