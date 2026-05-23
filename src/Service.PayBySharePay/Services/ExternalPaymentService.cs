using Microsoft.Extensions.Logging;
using Service.PayBySharePay.Interfaces;

namespace Service.PayBySharePay.Services;

/// <summary>
/// Dummy-implementering af <see cref="IExternalPaymentService"/>.
/// Simulerer et eksternt betalings-API og returnerer altid success.
///
/// TODO: Erstat med rigtig implementering (Nets Easy, Stripe, MobilePay osv.)
///       ved at injecte IHttpClientFactory og kalde det eksterne API.
/// </summary>
public class ExternalPaymentService : IExternalPaymentService
{
    private readonly ILogger<ExternalPaymentService> _logger;

    public ExternalPaymentService(ILogger<ExternalPaymentService> logger)
    {
        _logger = logger;
    }

    public async Task<ExternalPaymentResult> ChargeAsync(ExternalPaymentRequest request)
    {
        // TODO: Erstat med rigtigt API-kald, fx:
        // var response = await _httpClient.PostAsJsonAsync("https://api.payment-provider.com/charge", payload);
        // var result = await response.Content.ReadFromJsonAsync<ExternalPaymentResponse>();

        // Simuler netværksforsinkelse (fjern i produktion)
        await Task.Delay(300);

        var reference = $"DUMMY-{request.OrderId}-{Guid.NewGuid():N}"[..20];

        _logger.LogInformation(
            "[DUMMY] Betaling gennemført for ordre {OrderId}: {Amount} {Currency} — reference: {Ref}",
            request.OrderId, request.Amount, request.Currency, reference);

        return new ExternalPaymentResult(
            Success: true,
            PaymentReference: reference
        );
    }
}
