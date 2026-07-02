namespace Service.PayBySharePay.Interfaces;

/// <summary>
/// Abstraherer en betalingsudbyder (fx MobilePay, Vipps, Fake).
/// Al payment-provider kode skal ligge bag dette interface.
/// </summary>
public interface IPaymentProvider
{
    Task<ReservePaymentResult> ReserveAsync(ReservePaymentRequest request, CancellationToken cancellationToken = default);
    Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request, CancellationToken cancellationToken = default);
    Task<CancelPaymentResult> CancelAsync(CancelPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentStatusResult> GetStatusAsync(PaymentStatusRequest request, CancellationToken cancellationToken = default);
}

// ── Reserve ──────────────────────────────────────────────────────────────────

public sealed record ReservePaymentRequest(
    string GroupPaymentId,
    string ParticipantPaymentId,
    string MerchantId,
    long AmountMinorUnits,
    string Currency,
    string Description,
    string ReturnUrl,
    string CallbackUrl,
    string IdempotencyKey,
    string? TestPhoneNumber = null,
    string? MerchantSerialNumber = null,
    string? MerchantClientId = null,
    string? MerchantClientSecret = null,
    string? MerchantSubscriptionKey = null);

public sealed record ReservePaymentResult(
    bool Success,
    string? ProviderPaymentId,
    string? RedirectUrl,
    string? Status,
    string? ErrorCode,
    string? ErrorMessage);

// ── Capture ──────────────────────────────────────────────────────────────────

public sealed record CapturePaymentRequest(
    string ProviderPaymentId,
    long AmountMinorUnits,
    string Currency,
    string IdempotencyKey,
    string? MerchantSerialNumber = null,
    string? MerchantClientId = null,
    string? MerchantClientSecret = null,
    string? MerchantSubscriptionKey = null);

public sealed record CapturePaymentResult(
    bool Success,
    string? ProviderCaptureId,
    string? Status,
    string? ErrorCode,
    string? ErrorMessage);

// ── Cancel ───────────────────────────────────────────────────────────────────

public sealed record CancelPaymentRequest(
    string ProviderPaymentId,
    string Reason,
    string IdempotencyKey);

public sealed record CancelPaymentResult(
    bool Success,
    string? Status,
    string? ErrorCode,
    string? ErrorMessage);

// ── Status ───────────────────────────────────────────────────────────────────

public sealed record PaymentStatusRequest(string ProviderPaymentId);

public sealed record PaymentStatusResult(
    bool Success,
    string? Status,
    long? ReservedAmountMinorUnits,
    long? CapturedAmountMinorUnits,
    string? ErrorCode,
    string? ErrorMessage);
