# UC-13 — Payment Webhook og Vipps Callback

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-13 |
| Navn | Payment Webhook og Vipps Callback |
| Primær aktør | Payment provider (Vipps MobilePay eller Fake) |
| Formål | Modtage async betalingsstatus-opdateringer fra provider og opdatere `ParticipantPayment` |
| Trigger | Payment provider sender HTTP POST til callback-URL |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Vipps MobilePay** | Sender status-callbacks til registreret webhook-URL |
| **FakePaymentProvider** | Simulerer i test — sender ingen rigtige callbacks |
| **API** | `PaymentsController` (generisk webhook + MobilePay alias) og `VippsCallbackController` |
| **ParticipantPaymentStateService** | Opdaterer `ParticipantPayment`-status |

---

## Prækonditioner

- `ParticipantPayment`-record eksisterer med matchende `ProviderPaymentId` eller `reference`.
- Callback-URL er registreret hos provider ved `ReserveAsync`.

---

## Postkonditioner

- `ParticipantPayment.Status` er opdateret.
- `PaymentEventLog`-record er skrevet.
- Provider modtager HTTP 200 (undgår retry fra providers side).

---

## Normalforløb — Generisk webhook (`/webhooks/provider`)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Provider | `POST /api/payments/webhooks/provider` med `{ providerPaymentId, status }` |
| 2 | API | Slår `ParticipantPayment` op via `ProviderPaymentId` |
| 3 | API | Mapper `status` til state machine-kald: |
| | | `RESERVED` / `AUTHORIZED` → `SetReservedAsync()` |
| | | `CANCELLED` → `SetCancelledAsync()` |
| | | `FAILED` → `SetReservationFailedAsync()` |
| 4 | API | Returnerer `HTTP 200 { Accepted: true }` |

---

## Normalforløb — Vipps callback (`/vipps/callbacks/{reference}`)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Vipps | `POST /api/payments/vipps/callbacks/{reference}` med `VippsCallbackPayload { name, pspReference, amount, ... }` |
| 2 | API | `reference` = vores `ParticipantPaymentId` — slår op via `GetByProviderPaymentIdAsync(reference)` |
| 3 | API | Mapper `payload.Name` til state machine-kald: |
| | | `AUTHORIZED` / `RESERVE` → `SetReservedAsync()` |
| | | `CAPTURED` → ingen ændring (logges) |
| | | `CANCELLED` / `ABORTED` → `SetCancelledAsync()` |
| | | `TERMINATED` / `EXPIRED` → `SetReservationFailedAsync()` |
| 4 | API | Returnerer `HTTP 200` (altid — for at undgå Vipps-retry) |

---

## Alternativt forløb A1 — MobilePay webhook alias

- `POST /api/payments/webhooks/mobilepay` — identisk med `/webhooks/provider`.
- Samme `HandleWebhook()` implementation.

---

## Undtagelsesforløb

### E1 — `ProviderPaymentId` ikke fundet (generisk webhook)
- **Trin 2:** `GetByProviderPaymentIdAsync()` returnerer null.
- API returnerer `HTTP 404`.

### E2 — Reference ikke fundet (Vipps callback)
- **Trin 2:** Payment ikke fundet.
- API logger warning og returnerer `HTTP 200` *(returnerer 200 for at undgå at Vipps retrier uendeligt)*.

### E3 — Ukendt status-streng
- Generisk webhook: returnerer `HTTP 200 { Accepted: false, Message: "Ukendt status — ignoreret." }`.
- Vipps callback: logges og ignoreres.

---

## Datamodel

### Generisk webhook request
| Felt | Type | Påkrævet |
|------|------|----------|
| `providerPaymentId` | string | ✅ |
| `status` | string | ✅ | (`RESERVED`, `AUTHORIZED`, `CANCELLED`, `FAILED`) |

### Vipps callback payload (`VippsCallbackPayload`)
| Felt | Type | Beskrivelse |
|------|------|-------------|
| `reference` | string | Vores `ParticipantPaymentId` |
| `pspReference` | string? | Vipps' interne reference |
| `name` | string | Event: `CREATED`, `AUTHORIZED`, `CAPTURED`, `CANCELLED`, `ABORTED`, `EXPIRED`, `TERMINATED` |
| `amount.value` | long | Beløb i øre |
| `timestamp` | string? | |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `POST /api/payments/webhooks/provider` | POST | `[AllowAnonymous]` | 200, 400, 404 |
| `POST /api/payments/webhooks/mobilepay` | POST | `[AllowAnonymous]` | 200, 400, 404 |
| `POST /api/payments/vipps/callbacks/{reference}` | POST | `[AllowAnonymous]` | 200, 404 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Generisk webhook `POST /webhooks/provider` | ✅ | Mapper RESERVED/CANCELLED/FAILED |
| MobilePay webhook alias `POST /webhooks/mobilepay` | ✅ | Samme implementation |
| Vipps callback `POST /vipps/callbacks/{reference}` | ✅ | Mapper alle Vipps event-navne |
| `ParticipantPaymentStateService` integration | ✅ | State machine opdateres |
| `PaymentEventLog` ved state-skift | ✅ | |
| Webhook-signatur validering (HMAC) | ❌ | `[AllowAnonymous]` — ingen signaturcheck |
| ngrok/tunnel til Vipps i dev | ⚠️ | Placeholder i konfiguration — skal opsættes manuelt |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **Ingen webhook-signatur validering** | 🔴 Høj | Alle tre endpoints er `[AllowAnonymous]` uden HMAC/signaturcheck. Enhver kan sende falske status-opdateringer. |
| G2 | **Vipps `reference` = `ParticipantPaymentId`** | 🟡 Medium | `GetByProviderPaymentIdAsync(reference)` slår op på `ProviderPaymentId`-kolonnen, men Vipps sender `reference` = vores `ParticipantPaymentId`. Disse er ikke det samme felt — opslag kan fejle i produktion. |
| G3 | **`CAPTURED` fra Vipps ignoreres** | 🟡 Medium | Vipps sender `CAPTURED` event, men state-ændring håndteres ikke (kun logges). Capture styres udelukkende af vores eget flow. |
| G4 | **Ingen idempotens på webhook** | 🟢 Lav | Samme webhook kan modtages flere gange (Vipps retrier). State machine er idempotent, men der sker dobbelt event log-skrivning. |

---

## Tekniske noter

- Vipps retrier webhook-kald ved HTTP-fejl — derfor returnerer `VippsCallbackController` altid `200 OK` selv ved ukendt reference.
- `FakePaymentProvider` sender ingen rigtige webhooks — state-overgang sker synkront i `GroupPaymentOrchestrationService`.
- For at modtage Vipps-callbacks i dev kræves en public URL — typisk via ngrok-tunnel (placeholder i `appsettings.json`).

---

## Relaterede use cases

- [UC-09 — Reserver Betaling](UC-09-reserver-betaling.md)
- [UC-10 — Host Godkend og Capture](UC-10-godkend-og-capture.md)
