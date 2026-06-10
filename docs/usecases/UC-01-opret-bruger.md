# UC-01 — Opret Bruger

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-01 |
| Navn | Opret Bruger |
| Primær aktør | Ikke-registreret bruger (Person eller Merchant) |
| Formål | Oprette en konto i PayNSync, så brugeren kan logge ind og deltage i gruppeordrer |
| Trigger | Brugeren trykker "Opret konto" på login-siden |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Person** | Privat bruger der vil deltage i gruppeordrer |
| **Merchant** | Spisested/restaurant der vil modtage gruppeordrer |
| **API** | `Api.PayBySharePay` — modtager og validerer registreringsanmodningen |
| **Database** | SQL Server via EF Core — gemmer den nye `Participant`-record |

---

## Prækonditioner

- Brugeren er ikke allerede registreret med den pågældende e-mail.
- Systemet er tilgængeligt (API + database).

---

## Postkonditioner (succes)

- En ny `Participant`-record er gemt i databasen.
- Brugeren er automatisk logget ind (JWT returneres i response).
- Sessionen er gemt i `localStorage` (`sbys_token`, `sbys_user`).
- Brugeren navigeres til `/home`.

---

## Normalforløb — Person (Privat bruger)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Åbner `/register` i Angular SPA |
| 2 | Frontend | Viser registreringsformular med to tabs: **Bruger** / **Spisested** — "Bruger" er valgt som default |
| 3 | Bruger | Udfylder: Fulde navn *, E-mail *, Telefon (valgfri), Adgangskode *, Gentag adgangskode * |
| 4 | Bruger | Trykker "Opret konto" |
| 5 | Frontend | Validerer at de to adgangskoder matcher — viser inline fejl hvis ikke |
| 6 | Frontend | `POST /api/auth/register` med body: `{ name, email, phone?, password }` |
| 7 | API | `AuthController.Register()` tjekker om e-mail allerede er i brug via `SearchParticipantsAsync()` |
| 8 | API | Kalder `ParticipantService.CreatePersonAsync()` |
| 9 | Service | Validerer at navn ikke er tomt — kaster `ArgumentException` hvis det er |
| 10 | Service | BCrypt-hasher adgangskoden — gemmer hash i `Participant.PasswordHash` |
| 11 | Service | Gemmer ny `Participant` (type = `Person`) i databasen |
| 12 | API | Genererer JWT-token via `JwtTokenService.GenerateToken(id, name)` |
| 13 | API | Returnerer `HTTP 201` med `{ token, participantId, name, expiresAt }` |
| 14 | Frontend | Gemmer token + brugerinfo i `localStorage` |
| 15 | Bruger | Navigeres automatisk til `/home` |

---

## Normalforløb — Merchant (Spisested)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Åbner `/register` og vælger tab **Spisested** |
| 2 | Bruger | Udfylder: Navn *, Firmanavn *, CVR (valgfri), Kontaktperson (valgfri), E-mail (valgfri), Telefon (valgfri), Adresse (valgfri) |
| 3 | Bruger | Trykker "Opret konto" |
| 4 | Frontend | `POST /api/auth/register-merchant` med body: `{ name, companyName, cvrNumber?, contactPerson?, contactEmail?, contactPhone?, companyAddress? }` |
| 5 | API | `AuthController.RegisterMerchant()` tjekker om `contactEmail` allerede er i brug |
| 6 | API | Kalder `ParticipantService.CreateMerchantAsync()` |
| 7 | Service | Validerer at `CompanyName` ikke er tomt — kaster `ArgumentException` hvis det er |
| 8 | Service | Gemmer ny `Participant` (type = `Merchant`) — **ingen adgangskode** |
| 9 | API | Genererer JWT og returnerer `HTTP 201` |
| 10 | Frontend | Gemmer session, navigerer til `/home` |

---

## Alternative forløb

### A1 — E-mail er allerede i brug (Person)
- **Trin 7:** `SearchParticipantsAsync()` finder en eksisterende bruger med samme e-mail.
- API returnerer `HTTP 409 Conflict` med `{ error: "En bruger med denne e-mail eksisterer allerede." }`.
- Frontend viser fejlbeskeden inline på formularen.
- Brugeren kan rette e-mail og prøve igen.

### A2 — Adgangskoderne matcher ikke
- **Trin 5 (Frontend):** `personPassword !== personPasswordConfirm`.
- Ingen API-kald foretages.
- Frontend viser: *"Adgangskoderne stemmer ikke overens."* under det andet felt.

### A3 — Merchant-e-mail allerede i brug
- **Trin 5:** Samme som A1, men med teksten *"Et spisested med denne e-mail eksisterer allerede."*

### A4 — Netværksfejl / API utilgængeligt
- Frontend `error`-handler viser: *"Noget gik galt. Prøv igen."*

---

## Undtagelsesforløb

### E1 — Navn er tomt (server-side)
- `ParticipantService.CreatePersonAsync()` kaster `ArgumentException("En person skal have et navn.")`.
- `ExceptionHandlingMiddleware` mapper til `HTTP 400 Bad Request`.
- *(Frontend validerer ikke eksplicit mod dette — `required`-attribut på feltet er den eneste sikring.)*

### E2 — Firmanavn er tomt for Merchant
- `ParticipantService.CreateMerchantAsync()` kaster `ArgumentException("En merchant skal have et firmanavn.")`.
- Returnerer `HTTP 400`.

---

## Datamodel

### Input — Person
| Felt | Type | Påkrævet | Validering |
|------|------|----------|------------|
| `name` | string | ✅ | Ikke-tom (server-side) |
| `email` | string | ✅ | `[EmailAddress]` format, unik |
| `phone` | string? | ❌ | Ingen format-validering |
| `password` | string | ✅ | Min. 6 tegn (`[MinLength(6)]`) |

### Input — Merchant
| Felt | Type | Påkrævet | Validering |
|------|------|----------|------------|
| `name` | string | ✅ | Ikke-tom |
| `companyName` | string | ✅ | Ikke-tom (server-side) |
| `cvrNumber` | string? | ❌ | Ingen format-validering |
| `contactEmail` | string? | ❌ | `[EmailAddress]` format |
| øvrige felter | string? | ❌ | Ingen validering |

### Oprettet entity — `Participant`
| Kolonne | Værdi |
|---------|-------|
| `Type` | `Person` eller `Merchant` |
| `Name` | Fra request |
| `Email` | Fra request |
| `Phone` | Fra request |
| `PasswordHash` | BCrypt-hash (kun Person) |
| `CompanyName` | Fra request (kun Merchant) |
| `GroupOrderUrl` | `null` — sættes ikke ved registrering |

### JWT-token
| Claim | Indhold |
|-------|---------|
| `sub` | `participantId` (int → string) |
| `name` | Brugerens navn |
| `jti` | `Guid.NewGuid()` |
| Udløb | `DateTime.UtcNow + 480 min` (8 timer) |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `/api/auth/register` | POST | Anonym | 201 + JWT, 409 ved duplikat |
| `/api/auth/register-merchant` | POST | Anonym | 201 + JWT, 409 ved duplikat |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — Person-formular | ✅ | Alle felter + password-match-validering |
| Frontend — Merchant-formular | ✅ | Alle felter, tab-skift |
| Frontend — 409-fejlhåndtering | ✅ | Viser dansk fejlbesked |
| API — `POST /api/auth/register` | ✅ | Validering + oprettelse + JWT |
| API — `POST /api/auth/register-merchant` | ✅ | Validering + oprettelse + JWT |
| Service — BCrypt password-hashing | ✅ | `BCrypt.Net.BCrypt.HashPassword()` |
| Service — duplikat e-mail check | ✅ | Via `SearchParticipantsAsync()` |
| Database — gem `Participant` | ✅ | EF Core + SQL Server |
| JWT-udstedelse ved registrering | ✅ | Bruger er logget ind med det samme |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **Merchant oprettes uden adgangskode** | 🔴 Høj | `RegisterMerchantRequest` har ingen `Password`-felt. En merchant kan ikke logge ind med password — kun via seed/legacy-flow. |
| G2 | **Ingen e-mail-verifikation** | 🟡 Medium | E-mail bekræftes ikke. En bruger kan oprettes med en falsk e-mail. |
| G3 | **Merchant `GroupOrderUrl` sættes ikke** | 🟡 Medium | `Participant.GroupOrderUrl` er `null` ved oprettelse. Merchant kan ikke bruges i ordrer uden at en admin/seed opdaterer dette felt. |
| G4 | **Ingen telefon-format-validering** | 🟢 Lav | Telefonnummer gemmes som fri tekst uden format-check. |
| G5 | **Ingen CVR-validering** | 🟢 Lav | CVR-nummer gemmes som fri tekst — ingen check mod CVR-registret. |
| G6 | **JWT-udløb inkonsistens** | 🟡 Medium | `AuthController` sætter `expiresAt = UtcNow + 480 min` hardcodet. `JwtTokenService` læser fra `appsettings.json` (`43200 min`). `expiresAt` i response afspejler ikke nødvendigvis tokenets reelle udløb. |
| G7 | **Ingen `[Authorize]` på profiloprettelse via `ParticipantsController`** | 🟡 Medium | `POST /api/participants` (opret via separat endpoint) har ingen auth — kan potentielt misbruges. |
| G8 | **Token refresh ikke implementeret** | 🟡 Medium | Når JWT udløber efter 8 timer, skal brugeren logge ind igen. Ingen refresh-flow. |

---

## Tekniske noter

- **Duplikat-check**: Bruger `SearchParticipantsAsync()` som er en tekstsøgning — ikke et direkte opslag på unik e-mail. Der er ingen UNIQUE-constraint på `Email`-kolonnen i databasen, så race conditions kan teoretisk skabe dubletter.
- **Merchant login**: Da merchants ikke har `PasswordHash`, vil login-flowet springe passwordverifikation over (da `PasswordHash is null`). Dette er intentionelt til legacy seed-brugere men udokumenteret adfærd for nyoprettede merchants.
- **Session-lagring**: Token gemmes i `localStorage` (ikke `httpOnly` cookie) — eksponeret for XSS.

---

## Relaterede use cases

- [UC-02 — Log ind](UC-02-log-ind.md) *(ikke oprettet endnu)*
- [UC-03 — Opdater Profil](UC-03-opdater-profil.md) *(ikke oprettet endnu)*
- [UC-04 — Opret Ordre](UC-04-opret-ordre.md) *(ikke oprettet endnu)*
