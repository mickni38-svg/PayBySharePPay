using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Service.PayBySharePay.Interfaces;

namespace Infrastructure.Payments.PayBySharePay;

/// <summary>
/// Vipps MobilePay ePayment implementering af <see cref="IPaymentProvider"/>.
/// Bruger testmiljø (apitest.vipps.no) eller produktion afhængigt af konfiguration.
/// </summary>
public sealed class MobilePaySandboxPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _http;
    private readonly VippsMobilePayOptions _options;
    private readonly VippsMobilePayTokenService _tokenService;
    private readonly ILogger<MobilePaySandboxPaymentProvider> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public MobilePaySandboxPaymentProvider(
        HttpClient http,
        VippsMobilePayOptions options,
        VippsMobilePayTokenService tokenService,
        ILogger<MobilePaySandboxPaymentProvider> logger)
    {
        _http = http;
        _options = options;
        _tokenService = tokenService;
        _logger = logger;
    }

    // ── Reserve (Create payment) ──────────────────────────────────────────────

    public async Task<ReservePaymentResult> ReserveAsync(ReservePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _tokenService.GetAccessTokenAsync(
            merchantClientId: request.MerchantClientId,
            merchantClientSecret: request.MerchantClientSecret,
            merchantSubscriptionKey: request.MerchantSubscriptionKey,
            merchantSerialNumber: request.MerchantSerialNumber,
            cancellationToken: cancellationToken);

        // Vipps callback-URL skal pege på vores dedikerede endpoint.
        // Ignorerer hvad frontend sender for at sikre korrekt Vipps-format.
        var webhookUrl = $"{_options.CallbackBaseUrl.TrimEnd('/')}/api/payments/vipps/callbacks/{request.ParticipantPaymentId}";

        object body;
        if (!string.IsNullOrWhiteSpace(request.TestPhoneNumber))
        {
            body = new
            {
                amount = new { value = request.AmountMinorUnits, currency = request.Currency },
                paymentMethod = new { type = "WALLET" },
                reference = request.ParticipantPaymentId,
                userFlow = "WEB_REDIRECT",
                returnUrl = request.ReturnUrl,
                paymentDescription = request.Description,
                webhookUrl,
                profile = new { scope = "name phoneNumber" },
                customer = new { phoneNumber = request.TestPhoneNumber }
            };
        }
        else
        {
            body = new
            {
                amount = new { value = request.AmountMinorUnits, currency = request.Currency },
                paymentMethod = new { type = "WALLET" },
                reference = request.ParticipantPaymentId,
                userFlow = "WEB_REDIRECT",
                returnUrl = request.ReturnUrl,
                paymentDescription = request.Description,
                webhookUrl,
                profile = new { scope = "name phoneNumber" }
            };
        }

        var httpRequest = BuildRequest(HttpMethod.Post, "/epayment/v1/payments", token, request.IdempotencyKey, request.MerchantSerialNumber, request.MerchantSubscriptionKey);
        httpRequest.Content = JsonContent.Create(body, options: _jsonOptions);

        _logger.LogInformation("[VippsMobilePay] Opretter betaling {Reference}", request.ParticipantPaymentId);

        using var response = await _http.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("[VippsMobilePay] Reserve fejlede {Status}: {Error}", response.StatusCode, error);
            return new ReservePaymentResult(false, null, null, null, response.StatusCode.ToString(), error);
        }

        var result = await response.Content.ReadFromJsonAsync<VippsCreatePaymentResponse>(_jsonOptions, cancellationToken);
        return new ReservePaymentResult(
            Success: true,
            ProviderPaymentId: result?.Reference,
            RedirectUrl: result?.RedirectUrl,
            Status: result?.State,
            ErrorCode: null,
            ErrorMessage: null);
    }

    // ── Capture ───────────────────────────────────────────────────────────────

    public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _tokenService.GetAccessTokenAsync(
            merchantClientId: request.MerchantClientId,
            merchantClientSecret: request.MerchantClientSecret,
            merchantSubscriptionKey: request.MerchantSubscriptionKey,
            merchantSerialNumber: request.MerchantSerialNumber,
            cancellationToken: cancellationToken);

        var body = new
        {
            modificationAmount = new { value = request.AmountMinorUnits, currency = request.Currency }
        };

        var httpRequest = BuildRequest(HttpMethod.Post, $"/epayment/v1/payments/{request.ProviderPaymentId}/capture", token, request.IdempotencyKey, request.MerchantSerialNumber, request.MerchantSubscriptionKey);
        httpRequest.Content = JsonContent.Create(body, options: _jsonOptions);

        _logger.LogInformation("[VippsMobilePay] Capturer betaling {Reference}", request.ProviderPaymentId);

        using var response = await _http.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("[VippsMobilePay] Capture fejlede {Status}: {Error}", response.StatusCode, error);
            return new CapturePaymentResult(false, null, null, response.StatusCode.ToString(), error);
        }

        var result = await response.Content.ReadFromJsonAsync<VippsModifyResponse>(_jsonOptions, cancellationToken);
        return new CapturePaymentResult(true, result?.PspReference, result?.State, null, null);
    }

    // ── Cancel ────────────────────────────────────────────────────────────────

    public async Task<CancelPaymentResult> CancelAsync(CancelPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _tokenService.GetAccessTokenAsync(cancellationToken: cancellationToken);

        var httpRequest = BuildRequest(HttpMethod.Post, $"/epayment/v1/payments/{request.ProviderPaymentId}/cancel", token, request.IdempotencyKey);
        httpRequest.Content = JsonContent.Create(new { }, options: _jsonOptions);

        _logger.LogInformation("[VippsMobilePay] Annullerer betaling {Reference}", request.ProviderPaymentId);

        using var response = await _http.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("[VippsMobilePay] Cancel fejlede {Status}: {Error}", response.StatusCode, error);
            return new CancelPaymentResult(false, null, response.StatusCode.ToString(), error);
        }

        var result = await response.Content.ReadFromJsonAsync<VippsModifyResponse>(_jsonOptions, cancellationToken);
        return new CancelPaymentResult(true, result?.State, null, null);
    }

    // ── Status ────────────────────────────────────────────────────────────────

    public async Task<PaymentStatusResult> GetStatusAsync(PaymentStatusRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _tokenService.GetAccessTokenAsync(cancellationToken: cancellationToken);

        var httpRequest = BuildRequest(HttpMethod.Get, $"/epayment/v1/payments/{request.ProviderPaymentId}", token);

        using var response = await _http.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new PaymentStatusResult(false, null, null, null, "NOT_FOUND", "Betaling ikke fundet.");

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return new PaymentStatusResult(false, null, null, null, response.StatusCode.ToString(), error);
        }

        var result = await response.Content.ReadFromJsonAsync<VippsPaymentStatusResponse>(_jsonOptions, cancellationToken);
        return new PaymentStatusResult(
            Success: true,
            Status: result?.State,
            ReservedAmountMinorUnits: result?.Amount?.Value,
            CapturedAmountMinorUnits: result?.Aggregate?.CapturedAmount?.Value,
            ErrorCode: null,
            ErrorMessage: null);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private HttpRequestMessage BuildRequest(HttpMethod method, string path, string token, string? idempotencyKey = null, string? merchantSerialNumber = null, string? merchantSubscriptionKey = null)
    {
        var req = new HttpRequestMessage(method, $"{_options.BaseUrl.TrimEnd('/')}{path}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (string.IsNullOrWhiteSpace(merchantSubscriptionKey))
            throw new InvalidOperationException("merchantSubscriptionKey er ikke angivet.");
        req.Headers.Add("Ocp-Apim-Subscription-Key", merchantSubscriptionKey);
        req.Headers.Add("Merchant-Serial-Number", merchantSerialNumber ?? throw new InvalidOperationException("Merchant-Serial-Number er ikke angivet."));
        req.Headers.Add("Vipps-System-Name", "paybysharepay");
        if (idempotencyKey is not null)
            req.Headers.Add("Idempotency-Key", idempotencyKey);
        return req;
    }

    // ── Response DTOs ─────────────────────────────────────────────────────────

    private sealed record VippsCreatePaymentResponse(string? Reference, string? RedirectUrl, string? State);
    private sealed record VippsModifyResponse(string? PspReference, string? State);
    private sealed record VippsAmountDto(long Value, string? Currency);
    private sealed record VippsAggregateDto(VippsAmountDto? CapturedAmount, VippsAmountDto? RefundedAmount);
    private sealed record VippsPaymentStatusResponse(string? Reference, string? State, VippsAmountDto? Amount, VippsAggregateDto? Aggregate);
}
