namespace Service.PayBySharePay.DTOs;

/// <summary>
/// Response fra POST /api/merchant-orders.
/// Returneres til Merchant Demo efter draft er gemt og reservation er startet.
/// </summary>
public class InitMerchantOrderResultDto
{
    /// <summary>Reservationsstatus: ReservationStarted, Reserved, AlreadyReserved, AlreadyCaptured, Failed.</summary>
    public string Status { get; set; } = string.Empty;

    public int OrderId { get; set; }

    /// <summary>Vores interne ParticipantPaymentId.</summary>
    public int ParticipantPaymentId { get; set; }

    /// <summary>Betalingsudbyderens payment reference/id.</summary>
    public string? ProviderPaymentId { get; set; }

    /// <summary>URL som Merchant Demo skal redirecte deltageren til (MobilePay/Vipps). Null ved Fake provider.</summary>
    public string? PaymentRedirectUrl { get; set; }

    public string? Message { get; set; }
}
