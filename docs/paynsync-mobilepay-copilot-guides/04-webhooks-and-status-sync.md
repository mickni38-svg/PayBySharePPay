# 04 — Webhooks og Status Sync

## Mål
Implementér webhook endpoint, så MobilePay/Vipps sandbox kan opdatere PayNSync, når betalinger reserveres, captures, cancels eller fejler.

## Endpoint
Opret endpoint i API-projektet, fx:

```http
POST /api/payments/mobilepay/webhook
```

eller brug eksisterende controller-struktur.

## Krav
Webhook endpoint skal:

- acceptere provider webhook payload
- validere signature/authentication hvis MobilePay/Vipps dokumentationen kræver det
- aldrig logge secrets
- parse provider payment id/reference
- finde `ParticipantPayment`
- mappe provider event til intern `ParticipantPaymentStatus`
- være idempotent
- gemme rå payload i `PaymentEventLog`
- returnere hurtigt med 2xx ved korrekt modtagelse

## Event mapping
Implementér mapping fra provider events til interne states.

Eksempel:

```text
payment.reserved/authorized -> Reserved
payment.captured -> Captured
payment.cancelled -> Cancelled
payment.failed -> ReservationFailed eller CaptureFailed afhængigt af nuværende state
```

Brug de eksakte event-navne fra Vipps MobilePay dokumentationen.

## Idempotency
Webhook kan komme flere gange. Derfor:

- Hvis samme event allerede er behandlet, må state ikke ændres forkert.
- Hvis nuværende status allerede er `Captured`, må en gammel `Reserved` event ikke rulle status tilbage.
- Alle events må gerne logges, men state transition skal valideres.

## Lokal test
Tilføj mulighed for lokal webhook-test:

- enten via ngrok/dev tunnel
- eller via intern test endpoint, der simulerer webhook payload

## Controller/service design
Lav controller tynd:

```csharp
[ApiController]
[Route("api/payments/mobilepay/webhook")]
public sealed class MobilePayWebhookController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
}
```

Forretningslogikken skal ligge i service:

```csharp
public interface IMobilePayWebhookService
{
    Task HandleAsync(string payloadJson, IHeaderDictionary headers, CancellationToken cancellationToken);
}
```

## Logging
Log:

- CorrelationId
- ProviderPaymentId
- EventType
- ParticipantPaymentId
- OldStatus
- NewStatus
- ErrorCode hvis noget fejler

## Tests
Tilføj tests for:

- webhook reserved opdaterer status til Reserved
- webhook captured opdaterer status til Captured
- samme webhook to gange er ok
- gammel event må ikke rulle Captured tilbage til Reserved
- ukendt provider payment id logges og returnerer passende respons

## Definition of Done
- Webhook endpoint findes.
- Webhook-state-sync er idempotent.
- PaymentEventLog gemmer payload og statusændringer.
- Tests dækker dubletter og out-of-order events.
