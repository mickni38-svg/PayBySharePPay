using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Payments.PayBySharePay;

/// <summary>
/// Henter og cacher et Vipps MobilePay access token.
/// Token er gyldigt i 1 time (test) / 24 timer (prod).
/// Vi fornyer det 5 minutter før udløb for at undgå race conditions.
/// </summary>
public sealed class VippsMobilePayTokenService
{
    private readonly HttpClient _http;
    private readonly VippsMobilePayOptions _options;
    private readonly ILogger<VippsMobilePayTokenService> _logger;

    private string? _cachedToken;
    private DateTime _expiresAt = DateTime.MinValue;

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

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _expiresAt)
            return _cachedToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check inden for lock
            if (_cachedToken is not null && DateTime.UtcNow < _expiresAt)
                return _cachedToken;

            _logger.LogInformation("[VippsMobilePay] Henter nyt access token...");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/accesstoken/get");
            request.Headers.Add("client_id", _options.ClientId);
            request.Headers.Add("client_secret", _options.ClientSecret);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _options.SubscriptionKey);

            if (!string.IsNullOrWhiteSpace(_options.MerchantSerialNumber))
                request.Headers.Add("Merchant-Serial-Number", _options.MerchantSerialNumber);

            var response = await _http.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<AccessTokenResponse>(cancellationToken: cancellationToken)
                       ?? throw new InvalidOperationException("Tom respons fra access token endpoint.");

            _cachedToken = body.AccessToken;

            // expires_in er i sekunder — træk 5 min fra som buffer
            if (int.TryParse(body.ExpiresIn, out var expiresInSec))
                _expiresAt = DateTime.UtcNow.AddSeconds(expiresInSec - 300);
            else
                _expiresAt = DateTime.UtcNow.AddMinutes(55);

            _logger.LogInformation("[VippsMobilePay] Access token hentet. Udløber ca. {ExpiresAt}", _expiresAt);
            return _cachedToken;
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
