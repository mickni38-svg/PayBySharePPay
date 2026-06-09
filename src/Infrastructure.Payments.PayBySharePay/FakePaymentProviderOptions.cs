namespace Infrastructure.Payments.PayBySharePay;

/// <summary>
/// Konfigurationsindstillinger til <see cref="FakePaymentProvider"/>.
/// Bruges til at styre fejlsimulering i tests og lokal udvikling.
/// Konfigureres under sektionen <c>Payments:Fake</c> i appsettings.
/// </summary>
public sealed class FakePaymentProviderOptions
{
    public const string SectionName = "Payments:Fake";

    /// <summary>
    /// Simulerer at reservationen fejler.
    /// Returnerer <c>FAKE_RESERVE_FAILED</c> fra ReserveAsync.
    /// </summary>
    public bool SimulateReservationFailed { get; set; } = false;

    /// <summary>
    /// Simulerer at reservationen er udløbet (expired).
    /// Returnerer <c>FAKE_RESERVATION_EXPIRED</c> fra ReserveAsync.
    /// </summary>
    public bool SimulateReservationExpired { get; set; } = false;

    /// <summary>
    /// Simulerer at capture fejler.
    /// Returnerer <c>FAKE_CAPTURE_FAILED</c> fra CaptureAsync.
    /// </summary>
    public bool SimulateCaptureFailed { get; set; } = false;

    /// <summary>
    /// Simulerer at cancel fejler.
    /// Returnerer <c>FAKE_CANCEL_FAILED</c> fra CancelAsync.
    /// </summary>
    public bool SimulateCancelFailed { get; set; } = false;

    /// <summary>
    /// Simulerer at ReserveAsync kaster en uventet exception.
    /// Bruges til at teste exception-håndtering i orkestratoren.
    /// </summary>
    public bool SimulateReserveException { get; set; } = false;

    /// <summary>
    /// Simulerer at CaptureAsync kaster en uventet exception.
    /// Bruges til at teste exception-håndtering i orkestratoren.
    /// </summary>
    public bool SimulateCaptureException { get; set; } = false;
}
