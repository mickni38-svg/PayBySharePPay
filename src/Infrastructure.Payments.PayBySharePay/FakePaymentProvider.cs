using Microsoft.Extensions.Logging;
using Service.PayBySharePay.Interfaces;

namespace Infrastructure.Payments.PayBySharePay;

/// <summary>
/// Fake-implementering af <see cref="IPaymentProvider"/>.
/// Bruges lokalt og i tests uden rigtige MobilePay/Vipps credentials.
/// Returnerer altid success med simulerede værdier.
/// </summary>
public sealed class FakePaymentProvider : IPaymentProvider
{
    private readonly ILogger<FakePaymentProvider> _logger;

    public FakePaymentProvider(ILogger<FakePaymentProvider> logger)
    {
        _logger = logger;
    }

    public Task<ReservePaymentResult> ReserveAsync(ReservePaymentRequest request, CancellationToken cancellationToken = default)
    {
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
