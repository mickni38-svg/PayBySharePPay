# UC-14 — Legacy Betalingsflow

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-14 |
| Navn | Legacy Betalingsflow |
| Primær aktør | Host (ejer af ordren) |
| Formål | Alternativt, manuelt betalingsflow uden payment provider — registrér betaling direkte eller gennemfør via dummy eksternt API |
| Trigger | Host kalder `/pay`, `/complete` eller deltager kalder `POST /api/payments` |

---

## Baggrund og kontekst

Dette flow eksisterede før den provider-backed betalingsinfrastruktur (UC-09/UC-10) blev implementeret.  
Det er **stadig aktivt i kodebasen** men bruges ikke i den normale brugerrejse via Angular-frontend.  
`ExternalPaymentService.ChargeAsync()` er en **stub** der altid returnerer success.

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Host** | Ejer af ordren — kalder `/pay` og `/complete` |
| **Deltager** | Kan registrere betaling direkte via `POST /api/payments` |
| **ExternalPaymentService** | Dummy-service — simulerer eksternt betalings-API, returnerer altid success |
| **PaymentService** | Registrerer `Payment`-record og sender host-besked |
| **API** | `OrdersController` + `PaymentsController` |

---

## Prækonditioner

- Host er logget ind (JWT).
- Ordren eksisterer og er i status `ReadyToPay` (for `/complete`) eller enhver status (for `/pay`).

---

## Postkonditioner

- `Payment`-record oprettet (legacy-tabel).
- `OrderParticipant.Status = "Paid"` for berørte deltagere.
- Ordre-status sat til `Completed`.

---

## Normalforløb A — Registrér betaling for én deltager (`POST /api/payments`)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Deltager/Host | `POST /api/payments` med `{ orderId, participantId, amount }` |
| 2 | API | `PaymentService.RegisterPaymentAsync(dto)` |
| 3 | Service | Validerer beløb > 0, ordre og deltager eksisterer |
| 4 | Service | Opretter `Payment { Status = "Completed" }` |
| 5 | Service | Sætter `OrderParticipant.Status = "Paid"` for deltager |
| 6 | Service | Sender besked til host: `"✅ {navn} har betalt {beløb} kr."` |
| 7 | API | Returnerer `HTTP 201` med `PaymentDto` |

---

## Normalforløb B — Betal via eksternt API og gennemfør ordre (`POST /api/orders/{id}/pay`)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Host | `POST /api/orders/{id}/pay` med `{ requestingParticipantId, amount?, currency? }` |
| 2 | API | Henter ordreoverblik — beregner `TotalAmount` hvis `amount = 0` |
| 3 | API | `ExternalPaymentService.ChargeAsync({ orderId, amount, currency, description })` |
| 4 | `ExternalPaymentService` | Simulerer 300ms forsinkelse, returnerer altid `{ Success: true, PaymentReference: "DUMMY-..." }` |
| 5 | API | `OrderService.CompleteOrderAsync(id, requestingParticipantId)` |
| 6 | Service | Validerer host-ejerskab, status `ReadyToPay`, sætter `Order.Status = "Completed"` |
| 7 | API | Returnerer `HTTP 200` med `{ orderId, status: "Completed", paymentReference }` |

---

## Normalforløb C — Marker ordre som gennemført uden betaling (`POST /api/orders/{id}/complete`)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Host | `POST /api/orders/{id}/complete` med `{ requestingParticipantId }` |
| 2 | API | `OrderService.CompleteOrderAsync(id, requestingParticipantId)` |
| 3 | Service | Validerer host-ejerskab og status `ReadyToPay` |
| 4 | Service | Sætter `Order.Status = "Completed"` |
| 5 | API | Returnerer `HTTP 200` med `OrderDto` |

---

## Undtagelsesforløb

### E1 — Beløb ≤ 0 (Forløb A)
- `PaymentService` kaster `ArgumentException("En betaling skal have et gyldigt beløb større end 0.")`
- API returnerer `HTTP 400`

### E2 — Ordre/deltager ikke fundet
- `PaymentService` kaster `KeyNotFoundException`
- API returnerer `HTTP 404`

### E3 — Eksternt API fejler (Forløb B)
- `ExternalPaymentService` returnerer `{ Success: false }`
- API returnerer `HTTP 402 Payment Required`

### E4 — Ikke host / ordre ikke i ReadyToPay (Forløb B og C)
- `OrderService.CompleteOrderAsync` kaster `UnauthorizedAccessException` eller `InvalidOperationException`
- `ExceptionHandlingMiddleware` mapper `InvalidOperationException` → 409, men **ikke** `UnauthorizedAccessException` → returnerer 500

---

## Datamodel

### `RegisterPaymentRequest`
| Felt | Type | Beskrivelse |
|------|------|-------------|
| `orderId` | int | Ordren |
| `participantId` | int | Deltager der betaler |
| `amount` | decimal | Beløb i kr. |

### `PayOrderRequest`
| Felt | Type | Beskrivelse |
|------|------|-------------|
| `requestingParticipantId` | int | Skal matche ordre-host |
| `amount` | decimal | 0 = brug `TotalAmount` fra ordre |
| `currency` | string | Valuta, fx `"DKK"` |

### `PaymentDto` (response)
| Felt | Indhold |
|------|---------|
| `id` | Payment ID |
| `participantId` | Deltager |
| `participantName` | Navn |
| `amount` | Beløb i kr. |
| `status` | `"Completed"` (altid) |
| `createdAt` | Tidsstempel |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `POST /api/payments` | POST | Anonym | 201 + `PaymentDto`, 400, 404 |
| `POST /api/orders/{id}/pay` | POST | JWT `[Authorize]` | 200 + `PayOrderResponse`, 402, 403, 404 |
| `POST /api/orders/{id}/complete` | POST | JWT `[Authorize]` | 200 + `OrderDto`, 400, 403, 404 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| `POST /api/payments` — registrér betaling | ✅ | Anonym adgang |
| `POST /api/orders/{id}/pay` — dummy eksternt API | ✅ | `ExternalPaymentService` stub |
| `POST /api/orders/{id}/complete` — manuelt complete | ✅ | Kræver host + `ReadyToPay` |
| `ExternalPaymentService.ChargeAsync` — rigtig impl. | ❌ | Stub med `TODO`-kommentarer |
| `POST /api/payments` — autentificering | ❌ | Endpoint er anonymt — ingen JWT-validering |
| `UnauthorizedAccessException` → HTTP 403 mapping | ❌ | Middleware mapper det ikke — resulterer i 500 |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **`ExternalPaymentService` er en stub** | 🔴 Høj | `ChargeAsync()` simulerer 300ms og returnerer altid success. Bruges stadig af `/pay`-endpoint. |
| G2 | **`POST /api/payments` er anonym** | 🔴 Høj | Ingen autentificering — enhver kan registrere betalinger på ordrer de ikke ejer. |
| G3 | **`UnauthorizedAccessException` → HTTP 500** | 🔴 Høj | `ExceptionHandlingMiddleware` mapper ikke `UnauthorizedAccessException`. Host-tjek kaster 500 i stedet for 403. |
| G4 | **`Completed` vs. `Paid` — to terminal-statuser** | 🟡 Medium | `Paid` = provider-flow afsluttet. `Completed` = dette legacy flow. Frontend skal håndtere begge statuser som afsluttede. |
| G5 | **Legacy tabel `Payment` og provider `ParticipantPayment` sameksisterer** | 🟡 Medium | To separate betalingstabeller med forskellig semantik. `Payment` bruges kun i dette legacy flow. |

---

## Tekniske noter

- `Payment`-entiteten (legacy) er adskilt fra `ParticipantPayment` (provider-backed).
- `Payment.Status` sættes altid til `"Completed"` som streng — ingen enum.
- `ExternalPaymentService` genererer en `DUMMY-{orderId}-{guid}` reference — bruges ikke videre i systemet.
- `OrderService.CompleteOrderAsync` validerer `requestingParticipantId` fra request-body — ikke fra JWT `sub`.

---

## Relaterede use cases

- [UC-09 — Reserver Betaling](UC-09-reserver-betaling.md)
- [UC-10 — Host Godkend og Capture](UC-10-godkend-og-capture.md)
