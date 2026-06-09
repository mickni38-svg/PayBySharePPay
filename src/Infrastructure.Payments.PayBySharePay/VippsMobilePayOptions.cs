namespace Infrastructure.Payments.PayBySharePay;

public sealed class VippsMobilePayOptions
{
    public const string SectionName = "Payments:VippsMobilePay";

    /// <summary>Fx https://apitest.vipps.no (test) eller https://api.vipps.no (prod).</summary>
    public string BaseUrl { get; set; } = "https://apitest.vipps.no";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SubscriptionKey { get; set; } = string.Empty;
    public string MerchantSerialNumber { get; set; } = string.Empty;

    /// <summary>Base-URL som Vipps sender webhooks/callbacks til. Fx https://din-api.azurewebsites.net</summary>
    public string CallbackBaseUrl { get; set; } = string.Empty;
}
