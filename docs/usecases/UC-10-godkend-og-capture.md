# UC-10 — Host Godkend og Capture

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-10 |
| Navn | Host Godkend og Capture |
| Primær aktør | Host (ejer af ordren) |
| Formål | Host godkender at alle betalinger skal gennemføres — alle reserverede betalinger captures én ad gangen |
| Trigger | Host trykker "Godkend og betal" på ordreoverblik-siden |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Host** | Ejer af ordren — eneste der kan godkende |
| **API** | `OrdersController.ApproveOrder()` |
| **GroupPaymentOrchestrationService** | Orkestrerer capture af alle reserverede betalinger |
| **IPaymentProvider** | Udfører capture pr. deltager |
| **MerchantCallbackService** | Sender HTTP POST til merchant ved `Paid` |

---

## Prækonditioner

- Host er logget ind (JWT).
- Ordren er i status `ReadyToPay`, `HostApproved`, `Capturing` eller `PartiallyFailed` (retry tilladt).
- Mindst én `ParticipantPayment` er i status `Reserved`.

---

## Postkonditioner (succes — alle captured)

- Alle `ParticipantPayment`-records er i status `Captured`.
- `Order.Status = "Paid"`.
- `MerchantCallbackService` sender HTTP POST til merchant med ordredetaljer.
- `PaymentEventLog`-records skrevet for hvert state-skift.

---

## Postkonditioner (delvis fejl)

- Mindst én capture lykkedes, mindst én fejlede.
- `Order.Status = "PartiallyFailed"`.
- Host kan retry ved at kalde `/approve` igen.

---

## Normalforløb

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Host | Trykker "Godkend og betal" på `/orders/{id}` |
| 2 | Frontend | `POST /api/orders/{id}/approve` med `{ requestingParticipantId }` |
| 3 | API | `GroupPaymentOrchestrationService.ApproveAndCaptureAllAsync(orderId, requestingParticipantId)` |
| 4 | Orchestration | Validerer at `order.CreatedByParticipantId == requestingParticipantId` — kaster `UnauthorizedAccessException` ellers |
| 5 | Orchestration | Idempotens: returnerer success hvis ordre allerede er `Paid` |
| 6 | Orchestration | Validerer at ordre er i tilladt status |
| 7 | Orchestration | `order.Status = "HostApproved"` |
| 8 | Orchestration | Henter alle `ParticipantPayment`-records med `Status = Reserved` |
| 9 | Orchestration | `order.Status = "Capturing"` |
| 10 | Orchestration | For **hver** reserveret betaling: |
| 10a | | `stateService.SetCapturePendingAsync()` → status `CapturePending` |
| 10b | | `IPaymentProvider.CaptureAsync(request)` |
| 10c | | Hvis success: `stateService.SetCapturedAsync()` → status `Captured` |
| 10d | | Hvis fejl: `stateService.SetCaptureFailedAsync()` → status `CaptureFailed` + loop afbrydes (`PartiallyFailed`) |
| 11 | Orchestration | Hvis alle captured: `order.Status = "Paid"` |
| 12 | Orchestration | Kalder `MerchantCallbackService.SendCallbackAsync(order)` |
| 13 | API | Returnerer `HTTP 200` med `ApproveAndCaptureResult` |
| 14 | Frontend | Viser opdateret ordrestatus |

---

## Alternativt forløb A1 — Retry efter PartiallyFailed

- Host kalder `/approve` igen.
- **Trin 5:** Ordre er ikke `Paid` — fortsæt.
- **Trin 6:** Status `PartiallyFailed` er tilladt.
- **Trin 8:** Kun betalinger med `Status = Reserved` hentes (allerede `Captured` springes over — idempotent).
- Flowet fortsætter for de resterende betalinger.

---

## Undtagelsesforløb

### E1 — Ikke host
- **Trin 4:** `UnauthorizedAccessException` → `HTTP 500` *(bug: ikke mappet til 403)*.

### E2 — Forkert ordrestatus
- **Trin 6:** `InvalidOperationException` → `HTTP 409`.

### E3 — Ingen reserverede betalinger
- **Trin 8:** `reservedPayments.Count == 0`.
- Returnerer `ApproveAndCaptureResult { AllCaptured = false, OrderStatus = order.Status }`.

### E4 — Provider kaster exception ved capture
- `stateService.SetCaptureFailedAsync(..., "EXCEPTION", ...)`.
- Loop afbrydes. Ordre sættes til `PartiallyFailed`.

---

## Datamodel

### Request — `POST /api/orders/{id}/approve`
| Felt | Type | Påkrævet |
|------|------|----------|
| `requestingParticipantId` | int | ✅ |

### `ApproveAndCaptureResult`
| Felt | Indhold |
|------|---------|
| `allCaptured` | true hvis alle betalinger er captured |
| `orderStatus` | `"Paid"` eller `"PartiallyFailed"` |
| `results` | Liste af capture-resultater pr. deltager |

### `ParticipantPayment` state-flow ved capture
`Reserved → CapturePending → Captured`  
Fejl: `CapturePending → CaptureFailed`

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `POST /api/orders/{id}/approve` | POST | JWT `[Authorize]` | 200 + `ApproveAndCaptureResult`, 400, 409 |
| `GET /api/orders/{id}/capture-status` | GET | JWT `[Authorize]` | 200 + `CaptureStatusDto` |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — godkend-knap | ✅ | Vises kun for host, kalder `/approve` |
| API — `POST /api/orders/{id}/approve` | ✅ | JWT-beskyttet |
| Orchestration — sekventiel capture pr. deltager | ✅ | |
| Orchestration — idempotens (skip already-captured) | ✅ | |
| Orchestration — retry efter `PartiallyFailed` | ✅ | |
| State machine — `Reserved → CapturePending → Captured` | ✅ | |
| `MerchantCallbackService` ved `Paid` | ✅ | HTTP POST til merchant |
| `PaymentEventLog` pr. state-skift | ✅ | |
| API — `GET /api/orders/{id}/capture-status` | ✅ | Polling-endpoint — returnerer `CaptureStatusDto` med status pr. deltager |
| Frontend — capture-status polling | ❌ | Angular bruger `overview`-endpoint, ikke `capture-status` |
| `UnauthorizedAccessException` → HTTP 403 | ❌ | Returnerer HTTP 500 pga. middleware-gap |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **`requestingParticipantId` fra request-body** | 🔴 Høj | Host-tjek sker ved at sammenligne body-ID med `order.CreatedByParticipantId`. JWT `sub`-claim bruges ikke. |
| G2 | **`UnauthorizedAccessException` → HTTP 500** | 🔴 Høj | `ExceptionHandlingMiddleware` mapper ikke `UnauthorizedAccessException` — returnerer 500 i stedet for 403. |
| G3 | **Sekventiel capture stopper ved første fejl** | 🟡 Medium | Loop afbrydes ved første `CaptureFailed` — de resterende deltageres betalinger forsøges ikke i samme kald. |
| G4 | **Merchant callback fejl ignoreres** | 🟢 Lav | Hvis `MerchantCallbackService.SendCallbackAsync` fejler, ignoreres fejlen og ordren er stadig `Paid`. |

---

## Tekniske noter

- `ApproveAndCaptureAllAsync` sætter `order.Status = "HostApproved"` og `"Capturing"` undervejs — disse er transiente statuser der giver mulighed for idempotent retry.
- Sekventiel capture (ikke parallel) for at undgå race conditions på `ParticipantPayment.RowVersion`.
- `idempotencyKey = "capture-{paymentId}-{orderId}"` sendes til provider.

---

## Relaterede use cases

- [UC-09 — Reserver Betaling](UC-09-reserver-betaling.md)
- [UC-11 — Annuller Ordre](UC-11-annuller-ordre.md)
- [UC-13 — Payment Webhook](UC-13-payment-webhook.md)
