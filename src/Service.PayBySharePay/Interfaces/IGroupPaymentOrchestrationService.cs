using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

/// <summary>
/// Orkestrerer betalingsflowet: reservation pr. deltager og capture ved Host-godkendelse.
/// Holder domænelogik adskilt fra IPaymentProvider-implementeringen.
/// </summary>
public interface IGroupPaymentOrchestrationService
{
    /// <summary>
    /// Opretter en betaling for én deltager og starter reservationen hos betalingsudbyderen.
    /// Returnerer redirect-url, som deltageren sender til for at godkende i MobilePay/Vipps/Fake.
    /// </summary>
    Task<ReserveParticipantPaymentResult> ReserveParticipantPaymentAsync(
        int orderId,
        int participantId,
        string? merchantId,
        long amountMinorUnits,
        string currency,
        string returnUrl,
        string callbackUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validerer at Host godkender ordren, og capture'r herefter alle Reserved betalinger én ad gangen.
    /// Er idempotent: betalinger i status Captured springes over.
    /// Sender merchant callback når alle er captured.
    /// </summary>
    Task<ApproveAndCaptureResult> ApproveAndCaptureAllAsync(
        int orderId,
        int requestingParticipantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Annullerer en ordre: canceller alle ikke-capturede reservationer hos betalingsudbyderen.
    /// Sætter ordre til Cancelled.
    /// </summary>
    Task<CancelOrderResult> CancelOrderAsync(
        int orderId,
        int requestingParticipantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returnerer capture-status for alle betalinger tilknyttet ordren.
    /// </summary>
    Task<CaptureStatusDto> GetCaptureStatusAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}
