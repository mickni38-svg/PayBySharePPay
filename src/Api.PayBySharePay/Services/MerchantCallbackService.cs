using Microsoft.Extensions.Logging;
using Service.PayBySharePay.DTOs;
using Service.PayBySharePay.Interfaces;
using System.Net.Http.Json;

namespace Api.PayBySharePay.Services;

/// <summary>
/// Sender standard GroupOrderPaid-payload til merchant efter fuld capture.
/// Fejl stopper IKKE flowet — betalingerne er allerede captured.
/// </summary>
public sealed class MerchantCallbackService(
    IHttpClientFactory httpClientFactory,
    ILastMerchantCallbackStore callbackStore,
    ILogger<MerchantCallbackService> logger) : IMerchantCallbackService
{
    public async Task SendGroupOrderPaidAsync(
        PayNSyncFinalGroupOrderDto payload,
        string? callbackUrl,
        CancellationToken cancellationToken = default)
    {
        // Altid gem til dev-store (uanset om callbackUrl er sat)
        callbackStore.Set(payload.PaynsyncOrderId, payload);

        if (string.IsNullOrEmpty(callbackUrl))
        {
            logger.LogInformation(
                "[MerchantCallback] No callback URL for Order {OrderId} — payload stored for dev inspection, skipping HTTP POST",
                payload.PaynsyncOrderId);
            return;
        }

        try
        {
            var client = httpClientFactory.CreateClient("MerchantCallback");
            var response = await client.PostAsJsonAsync(callbackUrl, payload, cancellationToken);

            if (response.IsSuccessStatusCode)
                logger.LogInformation(
                    "[MerchantCallback] GroupOrderPaid sent OK for Order {OrderId} → {Url}",
                    payload.PaynsyncOrderId, callbackUrl);
            else
                logger.LogWarning(
                    "[MerchantCallback] Non-2xx {Status} for Order {OrderId} → {Url}",
                    (int)response.StatusCode, payload.PaynsyncOrderId, callbackUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[MerchantCallback] Exception sending GroupOrderPaid for Order {OrderId} → {Url}",
                payload.PaynsyncOrderId, callbackUrl);
        }
    }
}
