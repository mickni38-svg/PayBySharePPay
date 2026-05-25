# 03 — MobilePay/Vipps Sandbox Provider

## Mål
Implementér `MobilePaySandboxPaymentProvider`, som bruger Vipps MobilePay ePayment API i Merchant Test miljø.

## Dokumentation
Brug den officielle Vipps MobilePay Developer dokumentation for:

- ePayment API quick start
- API keys og authentication
- Merchant Test environment
- Create/reserve payment
- Capture payment
- Cancel payment
- Get payment status

## Konfiguration
Tilføj options:

```csharp
public sealed class MobilePayOptions
{
    public string Environment { get; set; } = "MerchantTest";
    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SubscriptionKey { get; set; } = string.Empty;
    public string MerchantSerialNumber { get; set; } = string.Empty;
    public string CallbackBaseUrl { get; set; } = string.Empty;
    public string ReturnBaseUrl { get; set; } = string.Empty;
}
```

Secrets må kun komme fra:

- user-secrets
- environment variables
- Key Vault senere

Aldrig hardcode secrets i `appsettings.json`.

## HTTP client
Implementér typed HttpClient:

```csharp
public sealed class MobilePayApiClient
{
    // Authenticate
    // Create payment
    // Capture payment
    // Cancel payment
    // Get payment status
}
```

## Headers
Sørg for at API client håndterer de headers, som Vipps MobilePay kræver, fx:

- Authorization Bearer token
- Ocp-Apim-Subscription-Key
- Merchant-Serial-Number
- Idempotency-Key hvor relevant

Brug den officielle dokumentation som kilde for eksakte header-navne og payloads.

## ReserveAsync
`ReserveAsync` skal:

1. Modtage `ReservePaymentRequest`.
2. Oprette betaling i MobilePay/Vipps sandbox.
3. Sende amount i minor units.
4. Sætte redirect/return URL.
5. Sætte webhook/callback reference hvis API'et kræver det.
6. Returnere provider payment id/reference og redirect URL.

## CaptureAsync
`CaptureAsync` skal:

1. Kalde capture endpoint på provider payment id/reference.
2. Capture det reserverede beløb.
3. Returnere capture-status og provider capture id hvis tilgængeligt.

## CancelAsync
`CancelAsync` skal:

1. Kalde cancel endpoint på provider payment id/reference.
2. Bruges når reservation skal frigives.
3. Returnere cancel-status.

## GetStatusAsync
`GetStatusAsync` skal hente aktuel provider-status og mappe den til interne statusnavne.

## Fejlhåndtering
Implementér:

- timeout handling
- non-success HTTP response handling
- provider error code mapping
- structured logging uden secrets
- retry kun hvor det er sikkert

## DI
Hvis config siger:

```json
"Payments": {
  "Provider": "MobilePaySandbox"
}
```

så registrér `MobilePaySandboxPaymentProvider` som `IPaymentProvider`.

## Tests
Lav unit tests med mocked HttpMessageHandler for:

- token/authentication request
- create/reserve payment success
- capture success
- cancel success
- provider error response

## Definition of Done
- Provideren kan konfigureres uden kodeændringer.
- Alle provider-kald er bag `IPaymentProvider`.
- Ingen secrets logges.
- Unit tests bruger mocked HTTP, ikke rigtigt netværk.
