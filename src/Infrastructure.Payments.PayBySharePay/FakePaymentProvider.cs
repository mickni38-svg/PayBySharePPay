using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.PayBySharePay.Interfaces;

namespace Infrastructure.Payments.PayBySharePay;

/// <summary>
/// Fake-implementering af <see cref="IPaymentProvider"/>.
/// Bruges lokalt og i tests uden rigtige MobilePay/Vipps credentials.
/// <para>
/// Fejlscenarier styres via <see cref="FakePaymentProviderOptions"/> (konfigureret under <c>Payments:Fake</c>):
/// <list type="bullet">
///   <item><see cref="FakePaymentProviderOptions.SimulateReservationFailed"/> — reservation afvises</item>
///   <item><see cref="FakePaymentProviderOptions.SimulateReservationExpired"/> — reservation udløber</item>
///   <item><see cref="FakePaymentProviderOptions.SimulateCaptureFailed"/> — capture fejler</item>
///   <item><see cref="FakePaymentProviderOptions.SimulateCancelFailed"/> — cancel fejler</item>
///   <item><see cref="FakePaymentProviderOptions.SimulateReserveException"/> — reserve kaster exception</item>
///   <item><see cref="FakePaymentProviderOptions.SimulateCaptureException"/> — capture kaster exception</item>
/// </list>
/// </para>
/// </summary>
public sealed class FakePaymentProvider : IPaymentProvider
{
    private readonly ILogger<FakePaymentProvider> _logger;
    private readonly FakePaymentProviderOptions _options;

    public FakePaymentProvider(ILogger<FakePaymentProvider> logger)
        : this(logger, new FakePaymentProviderOptions()) { }

    public FakePaymentProvider(ILogger<FakePaymentProvider> logger, FakePaymentProviderOptions options)
    {
        _logger = logger;
        _options = options;
    }

    // Bruges af DI via IOptions<T>
    public FakePaymentProvider(ILogger<FakePaymentProvider> logger, IOptions<FakePaymentProviderOptions> options)
        : this(logger, options.Value) { }

    public Task<ReservePaymentResult> ReserveAsync(ReservePaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (_options.SimulateReserveException)
            throw new InvalidOperationException("[FakePaymentProvider] Simuleret reserve-exception.");

        if (_options.SimulateReservationExpired)
        {
            _logger.LogWarning("[FakePaymentProvider] Simulerer udløbet reservation for ParticipantPaymentId={Id}", request.ParticipantPaymentId);
            return Task.FromResult(new ReservePaymentResult(
                Success: false,
                ProviderPaymentId: null,
                RedirectUrl: null,
                Status: "Expired",
                ErrorCode: "FAKE_RESERVATION_EXPIRED",
                ErrorMessage: "Simuleret: Reservation er udløbet."));
        }

        if (_options.SimulateReservationFailed)
        {
            _logger.LogWarning("[FakePaymentProvider] Simulerer fejlet reservation for ParticipantPaymentId={Id}", request.ParticipantPaymentId);
            return Task.FromResult(new ReservePaymentResult(
                Success: false,
                ProviderPaymentId: null,
                RedirectUrl: null,
                Status: "Failed",
                ErrorCode: "FAKE_RESERVE_FAILED",
                ErrorMessage: "Simuleret: Reservation afvist."));
        }

        var providerPaymentId = $"FAKE-{request.ParticipantPaymentId}-{Guid.NewGuid():N}"[..36];
        var redirectUrl = $"https://fake-payment.local/pay/{providerPaymentId}?return={Uri.EscapeDataString(request.ReturnUrl)}";

        _logger.LogInformation(
            "[FakePaymentProvider] Reserve: ParticipantPaymentId={ParticipantPaymentId}, Amount={Amount} {Currency}, ProviderPaymentId={ProviderPaymentId}",
            request.ParticipantPaymentId, request.AmountMinorUnits, request.Currency, providerPaymentId);

        return Task.FromResult(new ReservePaymentResult(
            Success: true,
            ProviderPaymentId: providerPaymentId,
            RedirectUrl: redirectUrl,
            Status: "Reserved",
            ErrorCode: null,
            ErrorMessage: null));
    }

    public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (_options.SimulateCaptureException)
            throw new InvalidOperationException("[FakePaymentProvider] Simuleret capture-exception.");

        if (_options.SimulateCaptureFailed)
        {
            _logger.LogWarning("[FakePaymentProvider] Simulerer fejlet capture for ProviderPaymentId={Id}", request.ProviderPaymentId);
            return Task.FromResult(new CapturePaymentResult(
                Success: false,
                ProviderCaptureId: null,
                Status: "Failed",
                ErrorCode: "FAKE_CAPTURE_FAILED",
                ErrorMessage: "Simuleret: Capture afvist."));
        }

        var captureId = $"FAKE-CAP-{Guid.NewGuid():N}"[..24];

        _logger.LogInformation(
            "[FakePaymentProvider] Capture: ProviderPaymentId={ProviderPaymentId}, Amount={Amount} {Currency}, CaptureId={CaptureId}",
            request.ProviderPaymentId, request.AmountMinorUnits, request.Currency, captureId);

        return Task.FromResult(new CapturePaymentResult(
            Success: true,
            ProviderCaptureId: captureId,
            Status: "Captured",
            ErrorCode: null,
            ErrorMessage: null));
    }

    public Task<CancelPaymentResult> CancelAsync(CancelPaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (_options.SimulateCancelFailed)
        {
            _logger.LogWarning("[FakePaymentProvider] Simulerer fejlet cancel for ProviderPaymentId={Id}", request.ProviderPaymentId);
            return Task.FromResult(new CancelPaymentResult(
                Success: false,
                Status: "Failed",
                ErrorCode: "FAKE_CANCEL_FAILED",
                ErrorMessage: "Simuleret: Cancel afvist."));
        }

        _logger.LogInformation(
            "[FakePaymentProvider] Cancel: ProviderPaymentId={ProviderPaymentId}, Reason={Reason}",
            request.ProviderPaymentId, request.Reason);

        return Task.FromResult(new CancelPaymentResult(
            Success: true,
            Status: "Cancelled",
            ErrorCode: null,
            ErrorMessage: null));
    }

    public Task<PaymentStatusResult> GetStatusAsync(PaymentStatusRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[FakePaymentProvider] GetStatus: ProviderPaymentId={ProviderPaymentId}",
            request.ProviderPaymentId);

        return Task.FromResult(new PaymentStatusResult(
            Success: true,
            Status: "Reserved",
            ReservedAmountMinorUnits: 10000,
            CapturedAmountMinorUnits: 0,
            ErrorCode: null,
            ErrorMessage: null));
    }
}
