# 00 — Master Prompt til Copilot Claude Sonnet 4.6

## Rolle
Du er senior C#/.NET arkitekt og payment-integration specialist. Du skal implementere MobilePay/Vipps sandbox integration i PayNSync/PayBySharePay-løsningen.

## Overordnet mål
Løsningen skal kunne testes af 3-4 testere i flere måneder med et realistisk gruppebetalingsflow:

1. En host opretter en gruppebetaling hos en dummy merchant, fx `Pizza Roma Test`.
2. Hver deltager får sit eget betalingslink.
3. Hver deltager godkender/reserverer sin egen betaling i MobilePay/Vipps testmiljø.
4. Systemet modtager webhooks/statusopdateringer.
5. Når alle relevante deltagere er reserveret, kan host klikke `Execute payment`.
6. Systemet capture'r betalingerne én ad gangen.
7. Ved fejl skal der være retry/stop/cancel-logik.
8. Test-dashboardet skal vise alle payment states, correlation ids, request/response metadata og fejl.

## Vigtige regler
- Brug aldrig tokens, payment ids eller browser-JSON fra rigtige takeaway-sites.
- Brug kun egen dummy merchant og MobilePay/Vipps Merchant Test miljø.
- MobilePay/Vipps integrationen må ikke lække ind i domænelogikken.
- Al payment-provider kode skal bag et interface.
- Alle eksterne API-kald skal være idempotente hvor det giver mening.
- Alle callbacks/webhooks skal kunne modtages flere gange uden at ødelægge state.
- Ingen secrets må hardcodes.
- Tilføj tests for vigtig state transition-logik.

## Arkitekturkrav
Implementér følgende lag:

```text
PayNSync.Application
 ├─ IPaymentProvider
 ├─ Payment orchestration services
 └─ Use cases

PayNSync.Infrastructure.Payments
 ├─ FakePaymentProvider
 ├─ MobilePaySandboxPaymentProvider
 ├─ MobilePayOptions
 └─ MobilePay API client

PayNSync.Api
 ├─ Payment endpoints
 ├─ Webhook endpoint
 ├─ Host execute endpoint
 └─ Test dashboard endpoints

PayNSync.Domain
 ├─ GroupPayment
 ├─ ParticipantPayment
 ├─ PaymentReservation
 └─ PaymentAttempt / PaymentEventLog
```

Tilpas navne og namespaces til den eksisterende løsning.

## Implementeringsstrategi
Læs først hele eksisterende solution og find relevante projekter, controllers, services, DbContext og modeller. Lav derefter ændringerne i små commits/steps efter filerne `01` til `06`.

## Definition of Done
- Projektet bygger uden fejl.
- Eksisterende tests passer stadig.
- Nye tests dækker centrale state transitions.
- MobilePay/Vipps sandbox credentials læses fra configuration/user-secrets/environment variables.
- Webhook endpoint kan kaldes lokalt.
- Test-dashboard viser status pr. gruppebetaling og pr. deltager.
- Host kan starte capture-flowet manuelt.
- Capture sker deltager-for-deltager og logger resultatet.
