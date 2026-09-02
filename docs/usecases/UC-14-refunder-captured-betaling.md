# UC-14: Refundér en captured deltagerbetaling

## Implementeringsprofil

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- **Opgavetype:** NEW_USE_CASE / payment
- **Størrelse:** Én betalings-slice med obligatorisk approval gate

## Mål

En autoriseret host kan refundere en allerede captured deltagerbetaling gennem den eksisterende `IPaymentProvider`-arkitektur, hvorefter PayNSync registrerer status og audit trail idempotent.

## Beslutningsgate

Dette ændrer betalingsadfærd og må ikke implementeres før Product Owner har godkendt:

1. kun fuld refund eller også delvis refund — **fuld refund anbefales til første version**;
2. refund pr. deltager eller hele gruppeordren;
3. hvilke ordrestatusser UI/API viser efter én eller flere refunds;
4. om host alene må refundere, eller om merchant/admin-rolle kræves.

## Forudsætninger

Læs den aktuelle officielle Vipps ePayment refund-dokumentation ved implementering. Bekræft endpoint, request-contract, idempotency-header og mulige states; kopier ikke antagelser fra use casen, hvis leverandørens dokumentation er ændret.

## Scope efter beslutning

- Udvid eksisterende provider-interface med en fokuseret refund-operation.
- Fake provider understøtter success, fejl og idempotent gentagelse.
- Vipps-provider kalder officielt refund-endpoint med `ProviderPaymentId`, korrekt beløb/currency og idempotency key.
- Kun `Captured` betaling kan refunderes.
- Statusændring går gennem `ParticipantPaymentStateService`; `PaymentEventLog` skrives.
- Provider-success og lokal persistence skal håndteres sikkert; planen skal beskrive recovery ved “provider success, database failure”.
- Endpoint udleder aktuel bruger fra JWT og validerer den godkendte rolle.
- UI viser behandling, resultat og fejl uden at kunne dobbeltklikke.

## Acceptkriterier

### AC1 – Gyldig refund

En godkendt refund hos provideren ændrer betalingen fra `Captured` til `Refunded` og skriver audit event.

### AC2 – Ugyldig status

Reserved, failed, cancelled eller allerede refunded betaling udløser intet nyt provider-kald.

### AC3 – Idempotens

Samme idempotency key kan gentages uden dobbelt refund.

### AC4 – Autorisation

Ikke-autoriseret bruger får 401/403, og provider kaldes ikke.

### AC5 – Delvis fejl

Providerfejl efterlader ikke betalingen som `Refunded`. Fejlen logges uden secrets og kan forsøges igen efter reglerne.

## Test

- Fake provider og orchestration: success, providerfejl, exception, gentagelse og ugyldige states.
- State transition og PaymentEventLog.
- Controller 401/403.
- Frontend HTTP mocks.
- Ingen live Vipps-kald i automatiske tests.

## Ikke en del af use casen

- Chargeback/dispute.
- Refund af ikke-captured reservation.
- Automatisk refund ved cancel.
- Ændring af merchant-callback-contract uden særskilt beslutning.
- Delvis refund, medmindre det vælges eksplicit i beslutningsgaten.
