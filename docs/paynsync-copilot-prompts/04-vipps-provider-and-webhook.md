# Prompt 04 – Vipps provider og webhook til Reserved

Overhold:

- `00-master-instructions.md`
- `architecture-updated.md`
- `business-rules-updated.md`
- `vipps-mobilepay-test-users-implementation-updated.md`
- eksisterende copilot-instructions

## Mål

Sørg for at PayNSync backend kan:

1. Oprette Vipps/MobilePay payment/reservation.
2. Returnere `redirectUrl` til Merchant Demo.
3. Modtage webhook.
4. Sætte konkret `ParticipantPayment` til `Reserved`.
5. Sætte order til `ReadyToPay`, når alle relevante participant payments er `Reserved`.

## PaymentReserveResult

Sørg for at reserve-resultatet kan returnere:

```csharp
public bool Success { get; set; }
public string? ProviderPaymentId { get; set; }
public string? RedirectUrl { get; set; }
public string? ErrorMessage { get; set; }
```

Tilpas eksisterende typer i stedet for at opfinde ny arkitektur, hvis projektet allerede har en tilsvarende model.

## MobilePaySandboxPaymentProvider

Reserve-kaldet skal:

1. Hente access token via `VippsMobilePayTokenService`.
2. Kalde Vipps/MobilePay ePayment create payment endpoint.
3. Bruge `userFlow = WEB_REDIRECT`.
4. Bruge unik og stabil `reference`.
5. Sende amount i minor units.
6. Sende `returnUrl`.
7. Sikre at webhook kan matche betaling via `reference`.
8. Returnere `ProviderPaymentId/reference`.
9. Returnere `redirectUrl`.

Reference-strategi:

```text
PNS-{OrderId}-{ParticipantId}-{ParticipantPaymentId}
```

eller eksisterende stabil strategi, hvis projektet allerede har det.

## Konfiguration

Secrets må kun læses fra backend config/user secrets/appsettings:

```text
Payments:VippsMobilePay:BaseUrl
Payments:VippsMobilePay:ClientId
Payments:VippsMobilePay:ClientSecret
Payments:VippsMobilePay:SubscriptionKey
Payments:VippsMobilePay:MerchantSerialNumber
Payments:VippsMobilePay:CallbackBaseUrl
```

Ingen secrets i frontend.

## Vipps webhook

Find `VippsCallbackController`.

Webhook må kun gøre dette:

```text
Find ParticipantPayment via ProviderPaymentId/reference
Map AUTHORIZED / RESERVE til Reserved
Map CANCELLED / ABORTED til Cancelled
Map EXPIRED / TERMINATED til ReservationFailed eller Expired efter eksisterende model
Log CAPTURED, men capture-state styres af /approve-flowet
Returnér 200 OK ved ukendt payment så Vipps ikke retrier unødigt
```

Når en payment bliver `Reserved`, skal systemet tjekke:

```text
Har alle relevante deltagere en ParticipantPayment med Status = Reserved?
```

Hvis ja:

```text
Order.Status = ReadyToPay
Send host message:
"Alle har bestilt og reserveret betaling. Du kan nu godkende den samlede ordre."
```

## Forbudt i webhook

Webhook må aldrig:

- capture betalinger
- sætte ordre `Paid`
- sende final merchant callback
- sende ordre til merchant

## Tests

Tilføj/opdater tests for:

- Reserve result indeholder redirectUrl/reference.
- AUTHORIZED webhook sætter kun payment til `Reserved`.
- AUTHORIZED webhook sender ikke merchant callback.
- AUTHORIZED webhook sætter `ReadyToPay`, når sidste deltager bliver `Reserved`.
- `ReadyToPay` sættes ikke før alle er `Reserved`.

## Output

Giv kort opsummering:

```text
Changed files
Webhook mapping
How redirectUrl/reference works
Tests added/updated
```
