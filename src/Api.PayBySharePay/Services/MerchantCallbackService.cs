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
    ISquareInspiredMerchantOrderAdapter adapter,
    ILogger<MerchantCallbackService> logger) : IMerchantCallbackService
{
    public async Task<MerchantOrderDeliveryResultDto> SendGroupOrderPaidAsync(
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
            return new MerchantOrderDeliveryResultDto(false, null, null, "MerchantOrderUrl er ikke konfigureret.");
        }

        try
        {
            var request = adapter.Map(payload);
            var client = httpClientFactory.CreateClient("MerchantCallback");
            var response = await client.PostAsJsonAsync(callbackUrl, request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            SquareInspiredMerchantOrderResponse? merchantResponse = null;
            try
            {
                merchantResponse = System.Text.Json.JsonSerializer.Deserialize<SquareInspiredMerchantOrderResponse>(responseBody,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (System.Text.Json.JsonException) { }

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("[MerchantCallback] Merchant order sent OK for Order {OrderId} → {Url}", payload.PaynsyncOrderId, callbackUrl);
                return new MerchantOrderDeliveryResultDto(true, merchantResponse?.OrderId, responseBody);
            }

            logger.LogWarning("[MerchantCallback] Non-2xx {Status} for Order {OrderId} → {Url}", (int)response.StatusCode, payload.PaynsyncOrderId, callbackUrl);
            return new MerchantOrderDeliveryResultDto(false, null, responseBody, $"HTTP {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[MerchantCallback] Exception sending merchant order for Order {OrderId} → {Url}", payload.PaynsyncOrderId, callbackUrl);
            return new MerchantOrderDeliveryResultDto(false, null, null, ex.Message);
        }
    }
}
