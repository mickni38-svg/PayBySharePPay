# UC-07 — Se Ordrer og Ordreoverblik

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-07 |
| Navn | Se Ordrer og Ordreoverblik |
| Primær aktør | Logget-ind bruger (Host eller Deltager) |
| Formål | Se liste over sine ordrer og se det fulde overblik for én ordre |
| Trigger | Bruger navigerer til `/orders` eller `/orders/{id}` |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Host** | Ejer af ordren — ser alle detaljer og handlingsknapper |
| **Deltager** | Inviteret person — ser sine egne detaljer |
| **API** | `OrdersController` — `GET /api/orders` og `GET /api/orders/{id}/overview` |

---

## Prækonditioner

- Bruger er logget ind (JWT).

---

## Postkonditioner

- Ingen dataændringer — udelukkende læsning.

---

## Normalforløb — Se ordreliste

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Navigerer til `/orders` |
| 2 | Frontend | `GET /api/orders?participantId={currentUserId}` |
| 3 | API | `OrderService.GetOrdersByParticipantAsync(participantId)` |
| 4 | Service | Henter alle ordrer hvor bruger er `OrderParticipant` |
| 5 | API | Returnerer `IEnumerable<OrderSummaryDto>` |
| 6 | Frontend | Viser to tabs: **Aktive** (Collecting, ReadyToPay, HostApproved, Capturing, PartiallyFailed) og **Afsluttede** (Paid, Completed, Cancelled) |

---

## Normalforløb — Se ordreoverblik

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Klikker på en ordre fra listen → navigerer til `/orders/{id}` |
| 2 | Frontend | `GET /api/orders/{id}/overview` |
| 3 | API | `OrderService.GetOrderOverviewAsync(id)` |
| 4 | Service | Henter ordre med alle relationer: `OrderParticipants`, `Payments`, `Messages`, `MerchantOrderDrafts`, `ParticipantPayments` |
| 5 | Service | Bygger `ParticipantOrderLines` — ordrelinjer pr. deltager fra alle drafts |
| 6 | Service | Bygger `ParticipantPayments` — `ParticipantPaymentSummaryDto` pr. deltager |
| 7 | Service | Synkroniserer `OrderParticipant.Status` til `Paid` hvis betaling er registreret men status ikke opdateret |
| 8 | API | Returnerer `OrderOverviewDto` |
| 9 | Frontend | Viser: titel, status, merchant, deltagerliste med betalingsstatus, ordrelinjer pr. deltager, host-handlingsknapper |

---

## Datamodel

### `OrderSummaryDto`
| Felt | Indhold |
|------|---------|
| `id` | Ordre-ID |
| `title` | Titel |
| `category` | Kategori |
| `status` | `Collecting`, `ReadyToPay`, `Paid` osv. |
| `createdAt` | Oprettelsestidspunkt |
| `merchantName` | Merchants firmanavn eller navn |
| `totalAmount` | Fra første `MerchantOrderDraft.TotalAmount` |
| `participants` | Liste af `OrderParticipantDto` |

### `OrderOverviewDto`
| Felt | Indhold |
|------|---------|
| `orderId` | Ordre-ID |
| `status` | Ordrens status |
| `participants` | Liste med navn, type og status pr. deltager |
| `payments` | Legacy `Payment`-records |
| `messages` | Alle beskeder på ordren |
| `participantOrderLines` | Ordrelinjer pr. deltager (fra merchant-drafts) |
| `participantPayments` | `ParticipantPaymentSummaryDto` pr. deltager (provider-backed) |
| `totalAmount` | Fra første draft |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `GET /api/orders` | GET | JWT `[Authorize]` | 200 + `IEnumerable<OrderSummaryDto>` (alle ordrer) |
| `GET /api/orders?participantId={id}` | GET | JWT `[Authorize]` | 200 + filtreret liste |
| `GET /api/orders/{id}/overview` | GET | JWT `[Authorize]` | 200 + `OrderOverviewDto`, 404 |
| `GET /api/orders/{id}/capture-status` | GET | JWT `[Authorize]` | 200 + `CaptureStatusDto`, 404 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — ordreliste med aktiv/afsluttet tabs | ✅ | Filtreret på status client-side |
| Frontend — ordreoverblik | ✅ | Deltagere, linjer, betalingsstatus |
| API — `GET /api/orders?participantId` | ✅ | JWT-beskyttet |
| API — `GET /api/orders/{id}/overview` | ✅ | JWT-beskyttet |
| API — `GET /api/orders/{id}/capture-status` | ✅ | Returnerer `CaptureStatusDto` med status pr. deltager |
| Service — `ParticipantPayments` i overview | ✅ | Inkluderet siden payment-integration branchen |
| Service — synkronisering af `OrderParticipant.Status` | ⚠️ | Synkroniseres ved hvert overview-kald (write i GET) |
| Real-time opdatering | ❌ | Ingen polling eller WebSocket |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **Write-operation i GET-endpoint** | 🟡 Medium | `GetOrderOverviewAsync` synkroniserer `OrderParticipant.Status` og kalder `SaveChangesAsync` — en GET-metode burde ikke have sideeffekter. |
| G2 | **`totalAmount` fra første draft** | 🟡 Medium | `draft?.TotalAmount ?? 0m` tager kun første draft. Hvis der er drafts fra flere deltagere med individuelle beløb, er totalbeløbet forkert. |
| G3 | **Ingen real-time opdatering** | 🟡 Medium | Siden opdateres ikke automatisk når andre deltagere indsender bestillinger eller betalingsstatus ændres. |
| G4 | **`GET /api/orders` returnerer alle ordrer** | 🟢 Lav | Uden `participantId`-filter returnerer `GetAll()` alle ordrer i databasen — ingen adgangskontrol på tværs af brugere. |
| G5 | **`capture-status` bruges ikke af Angular-frontend** | 🟢 Lav | Endpoint eksisterer og fungerer, men Angular-frontend bruger `overview`-endpoint til betalingsstatus i stedet. |

---

## Tekniske noter

- `GetOrderOverviewAsync` laver en muterende operation (status-synkronisering) inde i et læse-kald. Dette er en bivirkning der bør flyttes til en separat operation.
- Frontend laver ingen paginering — alle ordrer hentes på én gang.

---

## Relaterede use cases

- [UC-06 — Opret Ordre](UC-06-opret-ordre.md)
- [UC-08 — Bestil via Merchant-link](UC-08-bestil-via-merchant-link.md)
- [UC-10 — Host Godkend og Capture](UC-10-godkend-og-capture.md)
- [UC-11 — Annuller Ordre](UC-11-annuller-ordre.md)
