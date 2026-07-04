using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Payments.PayBySharePay;

/// <summary>
/// Henter og cacher Vipps MobilePay access tokens.
/// Understøtter både global config og per-merchant credentials.
/// Tokens caches per ClientId for at undgå unødvendige token-kald.
/// </summary>
public sealed class VippsMobilePayTokenService
{
    private readonly HttpClient _http;
    private readonly VippsMobilePayOptions _options;
    private readonly ILogger<VippsMobilePayTokenService> _logger;

    // Cache per clientId → (token, expiresAt)
    private readonly Dictionary<string, (string Token, DateTime ExpiresAt)> _cache = new();
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public VippsMobilePayTokenService(
        HttpClient http,
        VippsMobilePayOptions options,
        ILogger<VippsMobilePayTokenService> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Henter token med merchant-specifikke credentials hvis tilgængelige,
    /// ellers bruges global config fra appsettings.
    /// </summary>
    public async Task<string> GetAccessTokenAsync(
        string? merchantClientId = null,
        string? merchantClientSecret = null,
        string? merchantSubscriptionKey = null,
        string? merchantSerialNumber = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(merchantClientId) || string.IsNullOrWhiteSpace(merchantClientSecret) || string.IsNullOrWhiteSpace(merchantSubscriptionKey))
            throw new InvalidOperationException("Merchant-specifikke Vipps-credentials (ClientId/ClientSecret/SubscriptionKey) er påkrævet.");

        var clientId = merchantClientId;
        var clientSecret = merchantClientSecret;
        var subscriptionKey = merchantSubscriptionKey;
        var msn = merchantSerialNumber;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(clientId, out var cached) && DateTime.UtcNow < cached.ExpiresAt)
                return cached.Token;

            _logger.LogInformation("[VippsMobilePay] Henter access token for ClientId {ClientId}...", clientId[..Math.Min(8, clientId.Length)]);

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/accesstoken/get");
            request.Headers.Add("client_id", clientId);
            request.Headers.Add("client_secret", clientSecret);
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            if (!string.IsNullOrWhiteSpace(msn))
                request.Headers.Add("Merchant-Serial-Number", msn);

            var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("[VippsMobilePay] Token-kald fejlede: {StatusCode} – {Body}", (int)response.StatusCode, errorBody);
                response.EnsureSuccessStatusCode();
            }

            var body = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: cancellationToken)
                       ?? throw new InvalidOperationException("Tom respons fra access token endpoint.");

            DateTime expiresAt;
            if (int.TryParse(body.ExpiresIn, out var expiresInSec))
                expiresAt = DateTime.UtcNow.AddSeconds(expiresInSec - 300);
            else
                expiresAt = DateTime.UtcNow.AddMinutes(55);

            _cache[clientId] = (body.AccessToken, expiresAt);
            _logger.LogInformation("[VippsMobilePay] Token hentet for {ClientId}. Udløber ca. {ExpiresAt}", clientId[..Math.Min(8, clientId.Length)], expiresAt);
            return body.AccessToken;
        }
        finally
        {
            _lock.Release();
        }
    }

    private sealed record AccessTokenResponse(
        string token_type,
        string expires_in,
        string access_token)
    {
        public string ExpiresIn => expires_in;
        public string AccessToken => access_token;
    }
}
