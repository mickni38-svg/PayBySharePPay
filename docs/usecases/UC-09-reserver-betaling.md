# UC-09 — Reserver Betaling

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-09 |
| Navn | Reserver Betaling |
| Primær aktør | Deltager eller system (via UC-08) |
| Formål | Reservere en deltagers betaling hos payment provider (MobilePay/Vipps eller Fake) |
| Trigger | Deltager indsender merchant-bestilling (automatisk), eller eksplicit kald via `POST /api/orders/{id}/reserve` |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Deltager** | Den person hvis betaling reserveres |
| **API** | `OrdersController.ReservePayment()` |
| **GroupPaymentOrchestrationService** | Orkestrerer reservationsflowet |
| **IPaymentProvider** | `FakePaymentProvider` eller `MobilePaySandboxPaymentProvider` |
| **ParticipantPaymentStateService** | Ejer state machine-overgange for `ParticipantPayment` |

---

## Prækonditioner

- Ordren eksisterer.
- Deltager er `OrderParticipant` på ordren.
- Ingen eksisterende ikke-cancelled/fejlet betaling for denne deltager på denne ordre (idempotens).

---

## Postkonditioner (succes)

- `ParticipantPayment`-record oprettet med `Status = Reserved`.
- `ProviderPaymentId` sat fra payment provider.
- `PaymentEventLog`-records skrevet for hvert state-skift.
- `RedirectUrl` returneret (til Vipps-app, eller null for Fake).

---

## Normalforløb

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | System/Deltager | `POST /api/orders/{id}/reserve` med `{ participantId, merchantId?, amountMinorUnits, currency, returnUrl, callbackUrl }` |
| 2 | API | `GroupPaymentOrchestrationService.ReserveParticipantPaymentAsync()` |
| 3 | Orchestration | Idempotens-tjek: søger efter eksisterende ikke-cancelled/failed betaling for deltager+ordre — returnerer eksisterende hvis fundet |
| 4 | Orchestration | `ParticipantPaymentStateService.CreateAsync()` → opret ny `ParticipantPayment` med `Status = Created` + skriver `PaymentEventLog` |
| 5 | Orchestration | `stateService.SetReservationStartedAsync()` → status `ReservationStarted` + temp `ProviderPaymentId` + `PaymentEventLog` |
| 6 | Orchestration | `IPaymentProvider.ReserveAsync(request)` |
| 7 | Provider (Fake) | Returnerer `Success: true`, `ProviderPaymentId = "FAKE-..."`, `Status = "Reserved"` |
| 8 | Orchestration | Opdaterer `ParticipantPayment.ProviderPaymentId` med det rigtige ID |
| 9 | Orchestration | `stateService.SetReservedAsync()` → status `Reserved` + `PaymentEventLog` |
| 10 | API | Returnerer `HTTP 200` med `ReserveParticipantPaymentResult` incl. `RedirectUrl` |

---

## Alternativt forløb A1 — Vipps MobilePay provider

- **Trin 6:** `MobilePaySandboxPaymentProvider.ReserveAsync()` sender `POST /epayment/v1/payments` til Vipps API.
- Provider returnerer `RedirectUrl` til Vipps-app (bruger skal godkende i app).
- `Status` fra Vipps er ikke `Reserved` med det samme — forbliver `ReservationStarted` indtil webhook/callback bekræfter.
- Bruger redirectes til Vipps-app via `RedirectUrl`.

## Alternativt forløb A2 — Idempotent (betaling eksisterer allerede)

- **Trin 3:** Eksisterende ikke-cancelled `ParticipantPayment` fundet.
- Returnerer `Success: true` med eksisterende `ParticipantPaymentId` — ingen ny betaling oprettes.

---

## Undtagelsesforløb

### E1 — Provider returnerer fejl
- **Trin 6:** `result.Success = false`.
- `stateService.SetReservationFailedAsync()` → status `ReservationFailed` + `PaymentEventLog`.
- API returnerer `HTTP 400` med `{ ErrorCode, ErrorMessage }`.

### E2 — Provider kaster exception
- **Trin 6:** Uventet exception.
- `stateService.SetReservationFailedAsync(..., "EXCEPTION", ...)`.
- API returnerer `HTTP 400`.

---

## Datamodel

### Request — `POST /api/orders/{id}/reserve`
| Felt | Type | Påkrævet |
|------|------|----------|
| `participantId` | int | ✅ |
| `merchantId` | string? | ❌ |
| `amountMinorUnits` | long | ✅ |
| `currency` | string | ✅ |
| `returnUrl` | string | ✅ |
| `callbackUrl` | string | ✅ |

### `ParticipantPayment` state-flow ved reserve
`Created → ReservationStarted → Reserved`  
Fejl: `ReservationStarted → ReservationFailed`

### `PaymentEventLog` (audit trail)
Skrives ved hvert state-skift med: `ParticipantPaymentId`, `OrderId`, `FromStatus`, `ToStatus`, `CorrelationId`, tidsstempel.

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `POST /api/orders/{id}/reserve` | POST | JWT `[Authorize]` | 200 + `ReserveParticipantPaymentResult`, 400 |
| `GET /api/orders/{id}/capture-status` | GET | JWT `[Authorize]` | 200 + `CaptureStatusDto` |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| API — `POST /api/orders/{id}/reserve` | ✅ | JWT-beskyttet |
| Orchestration — idempotens-tjek | ✅ | Eksisterende non-failed betaling returneres |
| State machine — `Created → ReservationStarted → Reserved` | ✅ | Via `ParticipantPaymentStateService` |
| `PaymentEventLog` ved hvert skift | ✅ | Immutable audit trail |
| `FakePaymentProvider` | ✅ | Synkron success |
| `MobilePaySandboxPaymentProvider` | ✅ | Kald til Vipps API |
| `RedirectUrl` returneres | ⚠️ | Returneres i response — frontend bruger den ikke fuldt ud |
| Automatisk reserve ved merchant-bestilling | ✅ | `MerchantOrderService` kalder orchestration direkte |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **`participantId` fra request-body** | 🔴 Høj | Valideres ikke mod JWT `sub`-claim. En logget-ind bruger kan reservere på vegne af en anden deltager. |
| G2 | **`RedirectUrl` bruges ikke i frontend** | 🟡 Medium | `MobilePaySandboxPaymentProvider` returnerer en redirect-URL til Vipps-appen. Frontend-integration er ikke fuldt implementeret. |
| G3 | **Fake provider sætter `Reserved` synkront** | 🟢 Lav | I produktion (Vipps) bekræftes `Reserved`-status via webhook/callback — ikke synkront. Adfærden er forskellig mellem providers. |

---

## Tekniske noter

- `RowVersion` (optimistisk concurrency) er konfigureret på `ParticipantPayment` — EF Core kaster `DbUpdateConcurrencyException` ved parallelle opdateringer.
- `idempotencyKey = "reserve-{paymentId}-{orderId}-{participantId}"` sendes til payment provider for at undgå dobbelt-reservationer ved netværksfejl.

---

## Relaterede use cases

- [UC-08 — Bestil via Merchant-link](UC-08-bestil-via-merchant-link.md)
- [UC-10 — Host Godkend og Capture](UC-10-godkend-og-capture.md)
- [UC-13 — Payment Webhook](UC-13-payment-webhook.md)
