namespace Service.PayBySharePay.Interfaces;

/// <summary>
/// Sender callback til merchant, når alle betalinger er captured.
/// Implementeres i API-laget, så service-laget ikke har HttpClient-afhængighed.
/// </summary>
public interface IMerchantCallbackService
{
    /// <summary>Sender "Paid"-notifikation til merchantens CallbackUrl.</summary>
    Task SendPaidCallbackAsync(
        int orderId,
        string? callbackUrl,
        string? merchantId,
        IEnumerable<MerchantCallbackParticipantOrder> participantOrders,
        CancellationToken cancellationToken = default);
}

public sealed class MerchantCallbackParticipantOrder
{
    public int ParticipantId { get; init; }
    public bool Success { get; init; }
    public string? ProviderTransactionId { get; init; }
}
