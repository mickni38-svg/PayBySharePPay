# UC-06 — Opret Ordre

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-06 |
| Navn | Opret Ordre |
| Primær aktør | Host (logget-ind Person) |
| Formål | Oprette en gruppeordre, invitere deltagere og valgfrit tilknytte et spisested |
| Trigger | Host trykker "Opret ordre" i Angular SPA |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Host** | Logget-ind bruger der opretter og ejer ordren |
| **Deltagere** | Andre brugere der inviteres til ordren |
| **Merchant** | Valgfrit tilknyttet spisested |
| **API** | `OrdersController` + `OrderService` |
| **Database** | Opretter `Order`, `OrderParticipant`-records og `Message`-records |

---

## Prækonditioner

- Host er logget ind (JWT).
- Host kender ID'er på de deltagere der skal inviteres (kan søges via UC-05).

---

## Postkonditioner (succes)

- `Order`-record oprettet med status `Collecting`.
- Host tilføjet som `OrderParticipant` med status `Accepted`.
- Alle inviterede deltagere tilføjet som `OrderParticipant` med status `Invited`.
- Hver deltager (inkl. host) har fået et unikt `ParticipantToken` (GUID).
- Systembesked sendt til alle deltagere (bestillingslink eller generel invitation).
- Bruger navigeres til ordredetalje-siden `/orders/{id}`.

---

## Normalforløb — Med merchant

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Host | Navigerer til `/orders/create` |
| 2 | Frontend | Viser 4-trins wizard: Titel → Merchant → Deltagere → Opret |
| 3 | Host | Indtaster titel og valgfri kategori/besked |
| 4 | Host | Vælger merchant fra katalog (tab "Spisestedet" i find-participants) |
| 5 | Host | Tilføjer deltagere fra venneliste eller katalog |
| 6 | Host | Trykker "Opret ordre" |
| 7 | Frontend | `POST /api/orders` med `{ createdByParticipantId, title, category?, message?, merchantParticipantId?, participantIds[] }` |
| 8 | API | `OrdersController.CreateOrder()` kalder `OrderService.CreateOrderAsync()` |
| 9 | Service | Validerer at titel eller kategori er udfyldt |
| 10 | Service | Slår host og alle deltagere op — kaster `KeyNotFoundException` hvis nogen ikke eksisterer |
| 11 | Service | Slår merchant op — kaster `KeyNotFoundException` hvis ikke fundet |
| 12 | Service | Opretter `Order` med status `Collecting` og `JoinToken` (Guid) |
| 13 | Service | Tilføjer host som `OrderParticipant` (status `Accepted`, unikt `ParticipantToken`) |
| 14 | Service | Tilføjer alle øvrige deltagere som `OrderParticipant` (status `Invited`, unikt `ParticipantToken`) |
| 15 | Service | Gemmer ordre i databasen |
| 16 | Service | Konstruerer bestillingslink pr. deltager: `{merchant.GroupOrderUrl}?orderId={id}&merchantId={merchantId}&participantToken={token}` |
| 17 | Service | Opretter `Message` til **hver** deltager (inkl. host) med bestillingslinket |
| 18 | Service | Gemmer beskeder |
| 19 | API | Returnerer `HTTP 201` med `OrderDto` |
| 20 | Frontend | Navigerer til `/orders/{id}` |

---

## Normalforløb — Uden merchant

| Trin | Aktør | Handling |
|------|-------|----------|
| 1–6 | Host | Som ovenfor, men ingen merchant vælges |
| 7–15 | Service | Som ovenfor, men `MerchantParticipantId` er null |
| 16 | Service | Sender generel invitation til alle **inviterede** deltagere (ikke host): *"[Creator] har inviteret dig til gruppebetaling: '[Titel]'. Åbn appen for at se detaljer."* |
| 17 | API | Returnerer `HTTP 201` |

---

## Alternative forløb

### A1 — Host er eneste deltager (ingen andre inviterede)
- Ingen invitationsbeskeder sendes til andre deltagere.
- Orden oprettes med kun host som deltager.
- Status forbliver `Collecting` da `CheckAndSetReadyToPayAsync` kræver mindst én ikke-merchant deltager.

---

## Undtagelsesforløb

### E1 — Titel og kategori er begge tomme
- **Trin 9:** `ArgumentException("En ordre skal have en titel eller kategori.")`.
- `ExceptionHandlingMiddleware` → `HTTP 400`.

### E2 — Deltager-ID eksisterer ikke
- **Trin 10:** `KeyNotFoundException`.
- `HTTP 404`.

### E3 — Merchant-ID eksisterer ikke
- **Trin 11:** `KeyNotFoundException`.
- `HTTP 404`.

---

## Datamodel

### Request — `POST /api/orders`
| Felt | Type | Påkrævet | Beskrivelse |
|------|------|----------|-------------|
| `createdByParticipantId` | int | ✅ | Host's participant-ID |
| `title` | string | ⚠️ | Påkrævet hvis `category` ikke er udfyldt |
| `category` | string? | ⚠️ | Påkrævet hvis `title` ikke er udfyldt |
| `message` | string? | ❌ | Valgfri besked til deltagere |
| `merchantParticipantId` | int? | ❌ | Tilknyt merchant til ordren |
| `participantIds` | int[] | ❌ | Liste af deltager-IDs (ekskl. host) |

### Oprettede entities

**`Order`**
| Kolonne | Værdi |
|---------|-------|
| `Status` | `"Collecting"` |
| `JoinToken` | Nyt GUID |
| `CreatedByParticipantId` | Host's ID |
| `MerchantParticipantId` | Merchant-ID eller null |

**`OrderParticipant`** (én pr. deltager inkl. host)
| Kolonne | Værdi |
|---------|-------|
| `Status` | `"Accepted"` (host) / `"Invited"` (øvrige) |
| `ParticipantToken` | Nyt GUID (unikt) |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `POST /api/orders` | POST | JWT `[Authorize]` | 201 + `OrderDto`, 400, 404 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — opret-ordre wizard (4 trin) | ✅ | Titel → Merchant → Deltagere → Opret |
| API — `POST /api/orders` | ✅ | JWT-beskyttet |
| Service — opret ordre + deltagere | ✅ | Host auto-`Accepted`, øvrige `Invited` |
| Service — unikt `ParticipantToken` pr. deltager | ✅ | `Guid.NewGuid().ToString("N")` |
| Service — bestillingslink pr. deltager (med merchant) | ✅ | `{GroupOrderUrl}?orderId=...&participantToken=...` |
| Service — generel invitation (uden merchant) | ✅ | Sendes til inviterede, ikke host |
| Service — `JoinToken` genereres | ⚠️ | Genereres men bruges ikke — ingen join-endpoint |
| Tilføj deltagere efter oprettelse | ❌ | Ingen endpoint |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **`JoinToken` genereres men bruges ikke** | 🟡 Medium | `JoinToken` er et GUID på `Order`, men der eksisterer intet endpoint til at joigne via dette token. |
| G2 | **Ingen tilføjelse af deltagere efter oprettelse** | 🟡 Medium | Deltagere kan kun tilføjes ved oprettelse. Ingen `POST /api/orders/{id}/participants` endpoint. |
| G3 | **`createdByParticipantId` fra request-body** | 🔴 Høj | Host-ID sendes i body, ikke udledt fra JWT `sub`-claim. En bruger kan oprette en ordre på vegne af en anden. |
| G4 | **`GroupOrderUrl` kan være null på merchant** | 🟡 Medium | Hvis merchant ikke har en `GroupOrderUrl`, bruges `_merchantDemoUrl` (localhost:8081 fallback). I produktion peger link til demo-serveren. |
| G5 | **Host modtager eget bestillingslink** | 🟢 Lav | Systemet sender bestillingslink til alle inkl. host. Host bestiller typisk ikke fra merchant-siden. |

---

## Tekniske noter

- `OrderService` injicerer `IConfiguration` for at hente `AppSettings:MerchantDemoUrl` og `AppSettings:FrontendUrl`.
- Bestillingslinket konstrueres server-side og sendes som `Message`-record — ingen push-notification.
- `JoinToken` er genereret men ikke eksponeret i nogen DTO eller endpoint.

---

## Relaterede use cases

- [UC-05 — Find Deltagere og Tilføj Ven](UC-05-find-deltagere-tilfoj-ven.md)
- [UC-07 — Se Ordrer og Ordreoverblik](UC-07-se-ordrer-og-overblik.md)
- [UC-08 — Bestil via Merchant-link](UC-08-bestil-via-merchant-link.md)
