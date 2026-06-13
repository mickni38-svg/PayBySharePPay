# UC-15 — Dev- og Seed-tools

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-15 |
| Navn | Dev- og Seed-tools |
| Primær aktør | Udvikler / DevOps |
| Formål | Nulstille, seede og manipulere testdata i databasen — til lokal udvikling og staging-miljø |
| Trigger | Manuelt kald fra terminal (`Tools.PayBySharePay`) eller HTTP-kald til `DevController` |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Udvikler** | Kører CLI-scripts eller kalder `DevController`-endpoints |
| **`Tools.PayBySharePay`** | .NET konsolapp med seed-scripts — direkte EF Core-adgang |
| **`DevController`** | ASP.NET Core API-controller — anonyme admin-endpoints |

---

## Prækonditioner

- Adgang til databaseforbindelsesstreng (lokalt: `DESKTOP-HNI6DDI\\SQLEXPRESS`).
- API kører (for `DevController`-endpoints).

---

## Postkonditioner

Afhænger af kommando — se flows nedenfor.

---

## Flow A — Tools CLI: `seed`

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Udvikler | `dotnet run seed [--conn "..."] [--merchant-url "..."]` |
| 2 | Tool | Opretter 50 Person-participants og 10 Merchant-participants |
| 3 | Tool | Sætter `GroupOrderUrl` på alle merchants til `merchantUrl` |

## Flow B — Tools CLI: `seed-group-orders`

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Udvikler | `dotnet run seed-group-orders [--merchant-url "..."] [--api-url "..."]` |
| 2 | Tool | Opretter to gruppeordrer med deltagere |
| 3 | Tool | Participant 1 og 2 er hosts |

## Flow C — Tools CLI: `seed-pizza`

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Udvikler | `dotnet run seed-pizza` |
| 2 | Tool | Opretter/finder Michael Nielsen og Selma Markussen |
| 3 | Tool | Opretter en pizzaorden med Pizzeria Roma som merchant |
| 4 | Tool | Opretter `MerchantOrderDraft` med bestillingslinjer |

## Flow D — Tools CLI: `flush`

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Udvikler | `dotnet run flush` |
| 2 | Tool | Sletter alle seeded participants, ordrer og relationer |

## Flow E — Tools CLI: Øvrige kommandoer

| Kommando | Handling |
|----------|----------|
| `fix-pizza-lines` | Sætter `ParticipantId` på eksisterende pizzalinjer (baseret på `LineId`-præfiks M-/S-) |
| `mark-pizza-paid` | Sætter Michael og Selma til `Paid` på ordre id=5 |
| `set-pizza-ready` | Sætter ordre id=5 og tilknyttet draft til status `Ready` |
| `check-pizza-lines` | Debugger drafts, linjer og `OrderParticipants` for ordre id=5 |
| `seed-pizza-payments` | Opretter `Payment`-records (legacy) for Michael og Selma på ordre id=5 |
| `bestillingpaid <orderId> <participantId>` | Registrerer én legacy-betaling som `Completed` for specifik deltager |
| `list-orders` | Lister alle ordrer med status og deltagerantal |

---

## Flow F — DevController: `DELETE /api/dev/reset`

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Udvikler | `DELETE /api/dev/reset` (HTTP eller via `.http`-fil) |
| 2 | API | Sletter alle `MerchantOrderLines`, `MerchantOrderDrafts`, `Payments`, `OrderParticipants`, `Messages`, `Orders` |
| 3 | API | Returnerer `HTTP 204 No Content` |

## Flow G — DevController: `POST /api/dev/seed-merchant-urls`

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Udvikler | `POST /api/dev/seed-merchant-urls?merchantDemoUrl=https://...` |
| 2 | API | Finder alle Merchant-participants med `GroupOrderUrl = null` |
| 3 | API | Sætter `GroupOrderUrl` til den angivne URL |
| 4 | API | Returnerer `HTTP 200` med `{ updated, url }` |

---

## CLI-parametre

| Parameter | Standard | Beskrivelse |
|-----------|----------|-------------|
| `--conn "..."` | Lokalt SQLEXPRESS | Custom connection string |
| `--merchant-url "..."` | `http://localhost:8081` | URL til Merchant Demo-server |
| `--api-url "..."` | `http://localhost:5071` | URL til API (bruges af seed-group-orders) |

---

## API-endpoints (DevController)

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `DELETE /api/dev/reset` | DELETE | **Anonym** | 204 |
| `POST /api/dev/seed-merchant-urls?merchantDemoUrl=...` | POST | **Anonym** | 200 + `{ updated, url }` |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| `Tools.PayBySharePay` CLI | ✅ | Alle kommandoer implementeret |
| `DevController.ResetData` | ✅ | Sletter alle ordre-relaterede data |
| `DevController.SeedMerchantUrls` | ✅ | Opdaterer merchants der mangler URL |
| `DevController` autentificering | ❌ | Begge endpoints er anonyme |
| Miljø-tjek på `DevController` | ❌ | Ingen kontrol af `IHostEnvironment` — kørsel i produktion er mulig |
| `DevController` i produktion | ❌ | Er tilgængeligt i alle miljøer (ingen env-tjek) |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **`DevController` er anonym og aktiv i alle miljøer** | 🔴 Høj | `DELETE /api/dev/reset` og `POST /api/dev/seed-merchant-urls` har ingen `[Authorize]` og intet `IHostEnvironment`-tjek. En angriber eller fejlkald i produktion kan slette alle ordrer. |
| G2 | **Hardcodet connection string i `Tools.PayBySharePay`** | 🟡 Medium | Default `localConnectionString` peger på `DESKTOP-HNI6DDI\\SQLEXPRESS`. Skal overskrives med `--conn` ved brug i andre miljøer. |
| G3 | **Hardcodede participant-navne i seed-scripts** | 🟢 Lav | `seed-pizza`, `fix-pizza-lines` osv. bruger hardcodede navne `"Michael Nielsen"` og `"Selma Markussen"` samt ordre id=5. |
| G4 | **`flush` sletter ikke `Participants` og `PaymentEventLog`** | 🟢 Lav | `flush`-kommandoen sletter seed-deltagere — men `PaymentEventLog`-records og `ParticipantPayments` bevares muligvis. |

---

## Tekniske noter

- `Tools.PayBySharePay` bruger direkte EF Core-adgang (ingen service-lag) for hastighed og enkelhed.
- `DevController` bruger direkte `PayBySharePayDbContext` — omgår alle service-lag og validering.
- Tools-projektet understøtter custom connection string via `--conn`-argument, så det kan bruges mod staging og Azure.
- `MerchantDemoHostedService` (i API-projektet) starter automatisk en lokal `npx http-server` på port 8081 i Development — dette er separat fra seed-tools.

---

## Relaterede use cases

- [UC-06 — Opret Ordre](UC-06-opret-ordre.md)
- [UC-08 — Bestil via Merchant-link](UC-08-bestil-via-merchant-link.md)
- [UC-14 — Legacy Betalingsflow](UC-14-legacy-betaling.md)
