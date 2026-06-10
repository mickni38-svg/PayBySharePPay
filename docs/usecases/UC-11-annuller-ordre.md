# UC-11 — Annuller Ordre

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-11 |
| Navn | Annuller Ordre |
| Primær aktør | Host (ejer af ordren) |
| Formål | Annullere en aktiv ordre og frigive alle reserverede betalinger |
| Trigger | Host trykker "Annuller ordre" på ordreoverblik-siden |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Host** | Eneste der kan annullere ordren |
| **API** | `OrdersController.CancelOrder()` |
| **GroupPaymentOrchestrationService** | Annullerer alle ikke-captured betalinger |
| **IPaymentProvider** | Udfører cancel pr. deltager |
| **ParticipantPaymentStateService** | Ejer state machine-overgange |

---

## Prækonditioner

- Host er logget ind (JWT).
- Ordren er **ikke** i status `Paid` (annullering er ikke mulig efter fuld gennemførelse).

---

## Postkonditioner (succes)

- Alle `ParticipantPayment`-records i status `Reserved` er annulleret hos provider (`Cancelled`).
- `Order.Status = "Cancelled"`.
- `PaymentEventLog`-records skrevet.

---

## Normalforløb

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Host | Trykker "Annuller ordre" på `/orders/{id}` |
| 2 | Frontend | `POST /api/orders/{id}/cancel` med `{ requestingParticipantId }` |
| 3 | API | `GroupPaymentOrchestrationService.CancelOrderAsync(orderId, requestingParticipantId)` |
| 4 | Orchestration | Validerer at `order.CreatedByParticipantId == requestingParticipantId` |
| 5 | Orchestration | Idempotens: returnerer success hvis ordre allerede er `Cancelled` |
| 6 | Orchestration | Henter alle `ParticipantPayment`-records for ordren |
| 7 | Orchestration | For **hver** betaling i status `Reserved` eller `CapturePending`: |
| 7a | | `stateService.SetCancelledAsync()` → status `Cancelled` lokalt |
| 7b | | `IPaymentProvider.CancelAsync(request)` |
| 7c | | Hvis provider-cancel fejler: logges men fortsættes (ordre annulleres alligevel) |
| 8 | Orchestration | `order.Status = "Cancelled"` |
| 9 | API | Returnerer `HTTP 200` med `CancelOrderResult` |
| 10 | Frontend | Viser ordren som `Cancelled` |

---

## Alternativt forløb A1 — Allerede annulleret (idempotent)

- **Trin 5:** `order.Status == "Cancelled"`.
- Returnerer `CancelOrderResult { Success = true, OrderStatus = "Cancelled" }` uden yderligere handlinger.

---

## Undtagelsesforløb

### E1 — Ikke host
- **Trin 4:** `UnauthorizedAccessException` → `HTTP 500` *(bug: ikke mappet til 403)*.

### E2 — Ordre er `Paid`
- Ingen eksplicit check i koden for `Paid` — `Paid` er ikke i listen over annullerbare statuser (de reserverede betalinger er allerede captured).
- Praktisk: ingen `Reserved`-betalinger at annullere → `CancelOrderResult` returneres med tom liste.

---

## Datamodel

### Request — `POST /api/orders/{id}/cancel`
| Felt | Type | Påkrævet |
|------|------|----------|
| `requestingParticipantId` | int | ✅ |

### `CancelOrderResult`
| Felt | Indhold |
|------|---------|
| `success` | true |
| `orderStatus` | `"Cancelled"` |
| `results` | Liste af cancel-resultater pr. betaling |

### `ParticipantPayment` state-flow ved annullering
`Reserved → Cancelled`  
`CapturePending → Cancelled`

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `POST /api/orders/{id}/cancel` | POST | JWT `[Authorize]` | 200 + `CancelOrderResult`, 400 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — annuller-knap | ✅ | Vises kun for host |
| API — `POST /api/orders/{id}/cancel` | ✅ | JWT-beskyttet |
| Orchestration — annuller alle `Reserved`-betalinger | ✅ | |
| Orchestration — idempotens (allerede cancelled) | ✅ | |
| State machine — `Reserved → Cancelled` | ✅ | |
| Provider-cancel fejl ignoreres (ordre annulleres alligevel) | ✅ | Logges men stopper ikke flowet |
| `UnauthorizedAccessException` → HTTP 403 | ❌ | Returnerer HTTP 500 |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **`requestingParticipantId` fra request-body** | 🔴 Høj | Valideres ikke mod JWT `sub`-claim. |
| G2 | **`UnauthorizedAccessException` → HTTP 500** | 🔴 Høj | Middleware mapper ikke til 403. |
| G3 | **Ingen notifikation til deltagere ved annullering** | 🟡 Medium | Ingen `Message`-record sendes til deltagere når ordren annulleres. |

---

## Tekniske noter

- Provider-cancel-fejl logges men stopper ikke flowet — ordren markeres `Cancelled` uanset om provider-cancel lykkedes. Dette er en bevidst designbeslutning for at undgå at en ekstern fejl blokerer annullering.

---

## Relaterede use cases

- [UC-10 — Host Godkend og Capture](UC-10-godkend-og-capture.md)
- [UC-07 — Se Ordrer og Ordreoverblik](UC-07-se-ordrer-og-overblik.md)
