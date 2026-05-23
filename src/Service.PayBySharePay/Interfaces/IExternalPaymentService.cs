namespace Service.PayBySharePay.Interfaces;

/// <summary>
/// Repræsenterer integration mod et eksternt betalings-API (fx Nets, Stripe, MobilePay).
/// I fremtiden erstattes dummy-implementeringen med et rigtigt API-kald.
/// </summary>
public interface IExternalPaymentService
{
    /// <summary>
    /// Initier en betaling hos det eksterne betalings-API.
    /// Returnerer et <see cref="ExternalPaymentResult"/> med status og betalingsreference.
    /// </summary>
    Task<ExternalPaymentResult> ChargeAsync(ExternalPaymentRequest request);
}

public record ExternalPaymentRequest(
    int OrderId,
    decimal Amount,
    string Currency,
    string Description
);

public record ExternalPaymentResult(
    bool Success,
    string PaymentReference,
    string? ErrorMessage = null
);
