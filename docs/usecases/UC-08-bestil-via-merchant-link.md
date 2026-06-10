# UC-08 — Bestil via Merchant-link

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-08 |
| Navn | Bestil via Merchant-link |
| Primær aktør | Deltager (ikke nødvendigvis logget ind i PayNSync) |
| Formål | Deltager bestiller sine varer på merchant's side og indsender ordren til PayNSync |
| Trigger | Deltager klikker på bestillingslink modtaget i beskeder |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Deltager** | Klikker på link, bestiller og indsender |
| **Merchant Demo** | Statisk HTML-side (Pizzeria Roma) der viser menuen og sender bestillingen |
| **API** | `MerchantOrdersController` — modtager anonym POST |
| **MerchantOrderService** | Validerer token, opretter draft, starter reservation |
| **GroupPaymentOrchestrationService** | Starter betalingsreservation hos payment provider |

---

## Prækonditioner

- Deltager har modtaget et bestillingslink via beskedindbakken.
- Linket er gyldigt: `orderId`, `merchantId` og `participantToken` matcher en eksisterende `OrderParticipant`.
- Ordren er i status `Collecting`.

---

## Postkonditioner (succes)

- `MerchantOrderDraft` er oprettet i databasen med `Status = "Submitted"`.
- Alle ordrelinjer er tildelt deltagerens `ParticipantId`.
- `OrderParticipant.Status` er sat til `"OrderSubmitted"`.
- Betalingsreservation er startet hos payment provider.
- Hvis alle ikke-merchant deltagere nu har `OrderSubmitted` → ordre overgår til `ReadyToPay` og host modtager notifikation.

---

## Normalforløb

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Deltager | Åbner bestillingslink: `{merchant.GroupOrderUrl}?orderId=X&merchantId=Y&participantToken=Z` |
| 2 | Merchant Demo | Viser menu (Pizzeria Roma) — læser `orderId`, `merchantId`, `participantToken` fra URL |
| 3 | Deltager | Vælger varer og trykker "Betal" |
| 4 | Merchant Demo | `POST /api/merchant-orders` (anonym) med alle ordrelinjer, beløb og `participantToken` |
| 5 | API | `MerchantOrdersController.InitOrder()` → `MerchantOrderService.InitOrderAsync()` |
| 6 | Service | Slår `Order` op via `orderId` — kaster `KeyNotFoundException` hvis ikke fundet |
| 7 | Service | Slår `Merchant` op via `merchantParticipantId` — validerer type = `Merchant` |
| 8 | Service | Slår `OrderParticipant` op via `orderId` + `participantToken` — kaster `UnauthorizedAccessException` hvis ugyldigt |
| 9 | Service | Validerer at `OrderParticipant.Participant.Type != Merchant` |
| 10 | Service | Sletter eventuel eksisterende draft for samme deltager (re-submit) |
| 11 | Service | Opretter ny `MerchantOrderDraft` med `Status = "Submitted"` |
| 12 | Service | Alle `MerchantOrderLine`-records tildeles `ParticipantId = orderParticipant.ParticipantId` |
| 13 | Service | Sætter `OrderParticipant.Status = "OrderSubmitted"` |
| 14 | Service | Kalder `CheckAndSetReadyToPayAsync(orderId)` |
| 15 | Service | Kalder `ReserveParticipantPaymentAsync(...)` — starter betalingsreservation |
| 16 | API | Returnerer `HTTP 201` med `MerchantOrderDraftDto` inkl. `PaymentRedirectUrl` |
| 17 | Merchant Demo | Viser bekræftelsesbesked til deltager |

---

## Alternativt forløb A1 — Alle deltagere har nu bestilt

- **Trin 14:** `CheckAndSetReadyToPayAsync` finder at alle ikke-merchant `OrderParticipants` har `Status = "OrderSubmitted"`.
- `Order.Status` sættes til `"ReadyToPay"`.
- Systembesked sendes til host: *"Alle deltagere har bestilt — du kan nu gennemføre betalingen."*

---

## Alternativt forløb A2 — Deltager gen-indsender bestilling

- **Trin 10:** Eksisterende draft for samme `ParticipantId` + `OrderId` slettes.
- Ny draft oprettes med opdaterede ordrelinjer.
- Status sættes tilbage til `"OrderSubmitted"` (var allerede sat).
- Ny betalingsreservation startes (eksisterende returneres idempotent hvis ikke fejlet).

---

## Undtagelsesforløb

### E1 — Ugyldigt `participantToken`
- **Trin 8:** `OrderParticipant` ikke fundet → `UnauthorizedAccessException`.
- `ExceptionHandlingMiddleware` → `HTTP 500` *(bug: UnauthorizedAccessException er ikke mappet til 401/403)*.

### E2 — Merchant er ikke af type Merchant
- **Trin 7:** `InvalidOperationException` → `HTTP 409`.

### E3 — Deltager er en Merchant
- **Trin 9:** `InvalidOperationException` → `HTTP 409`.

### E4 — Ordre ikke fundet
- **Trin 6:** `KeyNotFoundException` → `HTTP 404`.

---

## Datamodel

### Request — `POST /api/merchant-orders`
| Felt | Type | Påkrævet |
|------|------|----------|
| `orderId` | int | ✅ |
| `merchantParticipantId` | int | ✅ |
| `participantToken` | string | ✅ |
| `subtotalAmount` | decimal | ✅ |
| `totalAmount` | decimal | ✅ |
| `currency` | string | ✅ |
| `lines` | `MerchantOrderLineDto[]` | ✅ |
| `merchantDraftReference` | string? | ❌ |
| `paymentMode` | string? | ❌ |
| `expiresAtUtc` | DateTime? | ❌ |

### Oprettet `MerchantOrderDraft`
| Kolonne | Værdi |
|---------|-------|
| `Status` | `"Submitted"` |
| `ParticipantId` | Fra valideret `OrderParticipant` |
| `Lines[].ParticipantId` | Sat til deltagerens ID |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `POST /api/merchant-orders` | POST | `[AllowAnonymous]` | 201 + `MerchantOrderDraftDto`, 400, 404 |
| `GET /api/merchant-orders/by-order/{orderId}` | GET | JWT `[Authorize]` | 200 + `MerchantOrderDraftDto`, 404 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Merchant Demo — Pizzeria Roma | ✅ | Statisk HTML med hardcodet menu |
| API — `POST /api/merchant-orders` (anonym) | ✅ | `[AllowAnonymous]` |
| Service — validering af `participantToken` | ✅ | EF Core opslag |
| Service — re-submit (slet + opret ny draft) | ✅ | |
| Service — tildel `ParticipantId` til alle linjer | ✅ | |
| Service — `CheckAndSetReadyToPayAsync` | ✅ | Auto-overgang til `ReadyToPay` |
| Service — start betalingsreservation | ✅ | `ReserveParticipantPaymentAsync` |
| `PaymentRedirectUrl` returneres | ⚠️ | URL returneres i response — frontend/demo bruger den ikke fuldt ud |
| Merchant callback ved `Paid` | ✅ | `MerchantCallbackService` sender POST til merchant |
| Kun Pizzeria Roma-menu | ⚠️ | Hardcodet — ikke konfigurerbar pr. merchant |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **`UnauthorizedAccessException` → HTTP 500** | 🔴 Høj | `ExceptionHandlingMiddleware` mapper ikke `UnauthorizedAccessException`. Ugyldigt token returnerer 500 i stedet for 401/403. |
| G2 | **Hardcodet demo-menu** | 🟡 Medium | Pizzeria Roma-menuen er statisk HTML. Ingen API til dynamisk menu pr. merchant. |
| G3 | **`PaymentRedirectUrl` bruges ikke** | 🟡 Medium | Merchant Demo viser bekræftelsesside — redirect-URL til MobilePay-app håndteres ikke. |
| G4 | **`participantToken` valideres ikke format** | 🟢 Lav | Ingen validering af at token er et gyldigt GUID-format før DB-opslag. |

---

## Tekniske noter

- `MerchantOrderService` afhænger direkte af `PayBySharePayDbContext` (ikke via repository) for `OrderParticipants` og `MerchantOrderDrafts` opslag — blander repository-mønster og direkte DB-adgang.
- Beløb i `MerchantOrderDraft` er `decimal` (kr), mens `ParticipantPayment` bruger `long AmountMinorUnits` (øre). Konvertering: `(long)(draft.TotalAmount * 100)` i `MerchantOrderService`.

---

## Relaterede use cases

- [UC-06 — Opret Ordre](UC-06-opret-ordre.md)
- [UC-09 — Reserver Betaling](UC-09-reserver-betaling.md)
- [UC-10 — Host Godkend og Capture](UC-10-godkend-og-capture.md)
- [UC-12 — Beskeder](UC-12-beskeder.md)
