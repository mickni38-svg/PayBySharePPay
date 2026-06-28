using Service.PayBySharePay.DTOs;

namespace Service.PayBySharePay.Interfaces;

/// <summary>
/// Sender standard GroupOrderPaid-payload til merchant efter fuld capture.
/// Implementeres i API-laget, så service-laget ikke har HttpClient-afhængighed.
/// </summary>
public interface IMerchantCallbackService
{
    /// <summary>
    /// Sender den endelige group order til merchantens GroupOrderUrl.
    /// Kaldes kun når Order.Status = Paid og alle betalinger er Captured.
    /// </summary>
    Task SendGroupOrderPaidAsync(
        PayNSyncFinalGroupOrderDto payload,
        string? callbackUrl,
        CancellationToken cancellationToken = default);
}

