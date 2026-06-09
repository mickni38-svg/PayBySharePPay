using Microsoft.Extensions.Logging;
using Service.PayBySharePay.Interfaces;
using System.Net.Http.Json;

namespace Api.PayBySharePay.Services;

/// <summary>
/// Sender HTTP callback til merchant når alle betalinger er captured.
/// Fejl stopper IKKE flowet — betalingerne er allerede captured.
/// </summary>
public sealed class MerchantCallbackService(
    IHttpClientFactory httpClientFactory,
    ILogger<MerchantCallbackService> logger) : IMerchantCallbackService
{
    public async Task SendPaidCallbackAsync(
        int orderId,
        string? callbackUrl,
        string? merchantId,
        IEnumerable<MerchantCallbackParticipantOrder> participantOrders,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(callbackUrl))
        {
            logger.LogInformation(
                "[MerchantCallback] No callback URL for Order {OrderId} — skipping", orderId);
            return;
        }

        var payload = new
        {
            orderId,
            merchantId,
            status = "Paid",
            participantOrders = participantOrders.Select(r => new
            {
                participantId = r.ParticipantId,
                status = r.Success ? "Paid" : "Failed",
                providerTransactionId = r.ProviderTransactionId
            })
        };

        try
        {
            var client = httpClientFactory.CreateClient("MerchantCallback");
            var response = await client.PostAsJsonAsync(callbackUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
                logger.LogInformation("[MerchantCallback] OK for Order {OrderId} → {Url}", orderId, callbackUrl);
            else
                logger.LogWarning("[MerchantCallback] Non-2xx {Status} for Order {OrderId} → {Url}",
                    (int)response.StatusCode, orderId, callbackUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[MerchantCallback] Exception for Order {OrderId} → {Url}", orderId, callbackUrl);
        }
    }
}
