# 01 — Payment Provider Abstraction

## Mål
Indfør et betalingsinterface, så PayNSync ikke er afhængig af MobilePay/Vipps direkte. Resten af systemet skal kun kende `IPaymentProvider`.

## Opgave til Copilot
Analyser den eksisterende løsning og implementér en payment provider abstraction.

## Interface
Opret eller tilpas følgende interface i application-laget:

```csharp
public interface IPaymentProvider
{
    Task<ReservePaymentResult> ReserveAsync(ReservePaymentRequest request, CancellationToken cancellationToken = default);
    Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest request, CancellationToken cancellationToken = default);
    Task<CancelPaymentResult> CancelAsync(CancelPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentStatusResult> GetStatusAsync(PaymentStatusRequest request, CancellationToken cancellationToken = default);
}
```

## DTO'er
Opret simple request/response DTO'er:

```csharp
public sealed record ReservePaymentRequest(
    string GroupPaymentId,
    string ParticipantPaymentId,
    string MerchantId,
    long AmountMinorUnits,
    string Currency,
    string Description,
    string ReturnUrl,
    string CallbackUrl,
    string IdempotencyKey);

public sealed record ReservePaymentResult(
    bool Success,
    string? ProviderPaymentId,
    string? RedirectUrl,
    string? Status,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record CapturePaymentRequest(
    string ProviderPaymentId,
    long AmountMinorUnits,
    string Currency,
    string IdempotencyKey);

public sealed record CapturePaymentResult(
    bool Success,
    string? ProviderCaptureId,
    string? Status,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record CancelPaymentRequest(
    string ProviderPaymentId,
    string Reason,
    string IdempotencyKey);

public sealed record CancelPaymentResult(
    bool Success,
    string? Status,
    string? ErrorCode,
    string? ErrorMessage);

public sealed record PaymentStatusRequest(string ProviderPaymentId);

public sealed record PaymentStatusResult(
    bool Success,
    string? Status,
    long? ReservedAmountMinorUnits,
    long? CapturedAmountMinorUnits,
    string? ErrorCode,
    string? ErrorMessage);
```

## Fake provider
Implementér `FakePaymentProvider`, som bruges lokalt uden MobilePay/Vipps:

- `ReserveAsync` returnerer success og dummy redirect-url.
- `CaptureAsync` returnerer success.
- `CancelAsync` returnerer success.
- `GetStatusAsync` returnerer en simuleret status.

## Dependency Injection
Tilføj konfiguration:

```json
"Payments": {
  "Provider": "Fake"
}
```

Hvis `Payments:Provider = Fake`, registrér `FakePaymentProvider`.

## Tests
Tilføj unit tests for:

- DI kan resolve `IPaymentProvider`.
- Fake reserve returnerer ProviderPaymentId.
- Fake capture returnerer success.
- Fake cancel returnerer success.

## Definition of Done
- Ingen eksisterende kode kalder MobilePay/Vipps direkte.
- Fake provider virker uden secrets.
- Projektet bygger.
