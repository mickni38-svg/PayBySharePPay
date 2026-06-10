# UC-04 — Opdater Profil

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-04 |
| Navn | Opdater Profil |
| Primær aktør | Logget-ind bruger (Person) |
| Formål | Ændre navn, e-mail og/eller telefonnummer på sin egen konto |
| Trigger | Brugeren navigerer til `/profile` og redigerer sine oplysninger |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Person** | Bruger der vil opdatere sine profiloplysninger |
| **API** | `Api.PayBySharePay` — modtager og gemmer ændringer |
| **Database** | SQL Server — opdaterer `Participant`-record |

---

## Prækonditioner

- Brugeren er logget ind (JWT i `localStorage`).
- `AuthService.currentUserId()` returnerer brugerens ID.

---

## Postkonditioner (succes)

- `Participant`-record er opdateret i databasen.
- Profilsiden viser en grøn succesbesked i 3 sekunder.
- Brugerens navn, e-mail og telefon vises opdateret i formularen.

---

## Normalforløb

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Navigerer til `/profile` |
| 2 | Frontend | `ProfileComponent.ngOnInit()` henter eksisterende profil via `GET /api/participants/{id}` |
| 3 | Frontend | Formularfelter udfyldes med: navn, e-mail, telefon |
| 4 | Bruger | Redigerer et eller flere felter |
| 5 | Bruger | Trykker "Gem ændringer" |
| 6 | Frontend | Validerer at navn ikke er tomt — gør intet hvis tomt |
| 7 | Frontend | `ProfileService.updateProfile(id, { name, email?, phone? })` → `PUT /api/participants/{id}/profile` |
| 8 | API | `ParticipantsController.UpdateProfile()` validerer at navn ikke er tomt |
| 9 | API | Kalder `ParticipantService.UpdateProfileAsync()` |
| 10 | Service | Validerer navn, slår `Participant` op via ID |
| 11 | Service | Opdaterer `Name`, `Email`, `Phone` på entity |
| 12 | Service | Gemmer via `_participantRepository.UpdateAsync()` + `SaveChangesAsync()` |
| 13 | API | Returnerer `HTTP 200` med opdateret `ParticipantDto` |
| 14 | Frontend | `saveSuccess` signal sættes til `true` — grøn besked vises i 3 sekunder |

---

## Alternativt forløb — Notifikations-toggle

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Trykker på notifikations-toggle på profilsiden |
| 2 | Frontend | `toggleNotifications()` inverterer `notificationsEnabled` signal |
| 3 | Frontend | Gemmer ny værdi i `localStorage` under nøglen `sbys_notifications_enabled` |
| — | — | *Ingen API-kald — kun lokal præference* |

---

## Undtagelsesforløb

### E1 — Navn er tomt
- **Trin 6:** `if (!this.name().trim()) return;` — ingen API-kald sendes.
- **Trin 8 (server-side):** `if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(...)` — `HTTP 400` returneres.
- Frontend viser fejlbesked: `saveError` signal sættes til `true`.

### E2 — Bruger ikke fundet (ID eksisterer ikke)
- **Trin 10:** `GetByIdAsync()` returnerer null → `KeyNotFoundException` kastes.
- `ExceptionHandlingMiddleware` mapper til `HTTP 404 Not Found`.
- Frontend viser fejlbesked.

### E3 — Netværksfejl
- Frontend sætter `saveError.set(true)`.

---

## Datamodel

### Request — `PUT /api/participants/{id}/profile`
| Felt | Type | Påkrævet | Validering |
|------|------|----------|------------|
| `name` | string | ✅ | Ikke-tom (både client- og server-side) |
| `email` | string? | ❌ | Ingen format-validering i `UpdateProfileRequest` |
| `phone` | string? | ❌ | Ingen format-validering |

### Response — `ParticipantDto`
| Felt | Type |
|------|------|
| `id` | int |
| `type` | string (`"Person"`) |
| `name` | string |
| `email` | string? |
| `phone` | string? |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `GET /api/participants/{id}` | GET | Anonym | 200 + `ParticipantDto`, 404 |
| `PUT /api/participants/{id}/profile` | PUT | Anonym | 200 + `ParticipantDto`, 400, 404 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — profilformular | ✅ | Navn, e-mail, telefon + gem-knap |
| Frontend — hent profil ved load | ✅ | `GET /api/participants/{id}` via `ProfileService` |
| Frontend — gem profil | ✅ | `PUT /api/participants/{id}/profile` |
| Frontend — notifikations-toggle | ✅ | Lokal `localStorage`-præference (ingen API) |
| Frontend — succesbesked (3 sek.) | ✅ | `saveSuccess` signal + `setTimeout` |
| API — `GET /api/participants/{id}` | ✅ | Returnerer `ParticipantDto` |
| API — `PUT /api/participants/{id}/profile` | ✅ | Validering + opdatering |
| Auth-krav på endpoints | ❌ | Begge endpoints er uden `[Authorize]` — se gaps |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **Ingen `[Authorize]` på profil-endpoints** | 🔴 Høj | `GET /api/participants/{id}` og `PUT /api/participants/{id}/profile` har ingen autentificeringskrav. Enhver kan hente eller opdatere en anden brugers profil, blot ved at kende ID'et. |
| G2 | **Ingen ejerskabsvalidering** | 🔴 Høj | Selv hvis `[Authorize]` tilføjes, tjekkes det ikke at JWT'ens `sub`-claim matcher `{id}` i URL'en. En logget-ind bruger kan opdatere en andens profil. |
| G3 | **`PasswordHash` returneres i `ParticipantDto`** | 🔴 Høj | `MapToDto()` inkluderer `PasswordHash` i DTO'en. BCrypt-hash eksponeres i API-response og gemmes i frontend-model. |
| G4 | **Notifikationer gemmes kun lokalt** | 🟡 Medium | `sbys_notifications_enabled` i `localStorage` — nulstilles ved logout/ryd cache. Ingen server-side notifikationsindstillinger. |
| G5 | **Ingen e-mail-format-validering i `UpdateProfileRequest`** | 🟢 Lav | `[EmailAddress]`-attribut mangler på `UpdateProfileRequest.Email` — bruges i `RegisterPersonRequest` men ikke her. |

---

## Tekniske noter

- `ProfileService` og `ProfileComponent` er separate fra `AuthService` — bruger `participantId` fra `AuthService.currentUserId()`.
- Navn-opdatering gemmes ikke automatisk i `localStorage`-sessionen (`sbys_user.name`) — efter gem kan visningsnavnet i navigationen være forældet indtil næste login.
- `ParticipantsController` har ingen class-level `[Authorize]` — dette er et gennemgående sikkerhedsproblem for alle endpoints i controlleren.

---

## Relaterede use cases

- [UC-02 — Log ind](UC-02-log-ind.md)
- [UC-03 — Log ud](UC-03-log-ud.md)
- [UC-05 — Find Deltagere og Tilføj Ven](UC-05-find-deltagere-tilfoj-ven.md) *(ikke oprettet endnu)*
