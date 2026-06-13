# UC-02 — Log ind

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-02 |
| Navn | Log ind |
| Primær aktør | Registreret bruger (Person) |
| Formål | Autentificere sig i PayNSync og modtage en JWT-session |
| Trigger | Brugeren åbner `/login` eller bliver omdirigeret hertil ved 401 |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Person** | Registreret bruger der vil logge ind |
| **API** | `Api.PayBySharePay` — validerer credentials og udsteder JWT |
| **Database** | SQL Server — opslag af `Participant` via e-mail |

---

## Prækonditioner

- Brugeren har en registreret konto (type `Person`) med den pågældende e-mail.
- Systemet er tilgængeligt.

---

## Postkonditioner (succes)

- JWT-token er gemt i `localStorage` (`sbys_token`).
- Brugerinfo er gemt i `localStorage` (`sbys_user`).
- `AuthService.isLoggedIn` signal er `true`.
- Brugeren er navigeret til `/home` via `window.location.href = '/home'` (hard reload).

---

## Normalforløb

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Åbner `/login` i Angular SPA |
| 2 | Frontend | Viser login-formular med e-mail-felt og "Log ind"-knap |
| 3 | Bruger | Indtaster sin e-mail og trykker "Log ind" |
| 4 | Frontend | Kalder `AuthService.login(email)` → `POST /api/auth/login` med `{ email }` |
| 5 | API | `AuthController.Login()` søger efter `Person` med den givne e-mail via `SearchParticipantsAsync()` |
| 6 | API | Henter entity med `GetByEmailAsync()` for at få adgang til `PasswordHash` |
| 7 | API | Da `password` ikke er sendt i request, springes BCrypt-verifikation over |
| 8 | API | Genererer JWT via `JwtTokenService.GenerateToken(id, name)` |
| 9 | API | Returnerer `HTTP 200` med `{ token, participantId, name, expiresAt }` |
| 10 | Frontend | `AuthService._storeSession()` gemmer token + brugerinfo i `localStorage` |
| 11 | Frontend | `window.location.href = '/home'` — hard reload for frisk app-state |

---

## Alternative forløb

### A1 — Login med password (når `PasswordHash` er sat)
- **Trin 3:** Bruger sender både e-mail og password i body.
- **Trin 7:** `!string.IsNullOrEmpty(request.Password) && personWithHash.PasswordHash is not null` → BCrypt.Verify() køres.
- Hvis korrekt: forløbet fortsætter fra trin 8.
- Hvis forkert: se E2.

### A2 — Token udløbet, bruger omdirigeres til login
- `apiInterceptor` modtager `HTTP 401` på et API-kald.
- `auth.logout()` kaldes → `localStorage` ryddes.
- Bruger navigeres til `/login`.
- Normalforløbet starter fra trin 1.

---

## Undtagelsesforløb

### E1 — E-mail ikke fundet
- **Trin 5:** `SearchParticipantsAsync()` returnerer ingen `Person` med e-mailen.
- API returnerer `HTTP 401` med `{ error: "Ingen bruger fundet med denne email." }`.
- Frontend viser: *"Ingen konto fundet med den e-mail."*

### E2 — Forkert adgangskode
- **Trin 7:** `BCrypt.Verify()` returnerer `false`.
- API returnerer `HTTP 401` med `{ error: "Forkert adgangskode." }`.
- Frontend viser: *"Ingen konto fundet med den e-mail."* *(samme generiske fejlbesked som E1 — ingen skelnen i frontend)*

### E3 — Netværksfejl
- Frontend viser: *"Noget gik galt. Prøv igen."*

---

## Datamodel

### Request
| Felt | Type | Påkrævet | Beskrivelse |
|------|------|----------|-------------|
| `email` | string | ✅ | Brugerens e-mail |
| `password` | string? | ❌ | Valgfri — kun brugt hvis `PasswordHash` er sat |

### Response (HTTP 200)
| Felt | Type | Beskrivelse |
|------|------|-------------|
| `token` | string | JWT Bearer token |
| `participantId` | int | Brugerens ID |
| `name` | string | Brugerens navn |
| `expiresAt` | DateTime | `UtcNow + 480 min` (8 timer) |

### JWT-claims
| Claim | Indhold |
|-------|---------|
| `sub` | `participantId` |
| `name` | Brugerens navn |
| `jti` | Nyt `Guid` pr. token |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `/api/auth/login` | POST | Anonym | 200 + JWT, 401 ved ukendt e-mail eller forkert password |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — login-formular | ✅ | E-mail-felt + adgangskode-felt + submit-knap |
| Frontend — 401-håndtering | ✅ | Viser dansk fejlbesked |
| Frontend — session-lagring | ✅ | `localStorage` via `AuthService._storeSession()` |
| Frontend — hard reload ved login | ✅ | `window.location.href = '/home'` |
| API — `POST /api/auth/login` | ✅ | E-mail-opslag + BCrypt-verifikation |
| API — JWT-udstedelse | ✅ | `JwtTokenService.GenerateToken()` |
| Frontend — password-felt i login | ✅ | Password-felt tilføjet med vis/skjul-knap — sendes med til API |
| JWT-interceptor — 401 → logout | ✅ | `apiInterceptor` kalder `auth.logout()` |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | ~~**Password-felt manglede i login-formular**~~ | ~~🔴 Høj~~ | **Implementeret** — password-felt tilføjet med vis/skjul-knap. `AuthService.login()` sender nu password med når det er udfyldt. |
| G2 | **Merchant kan ikke logge ind** | 🔴 Høj | `AuthController.Login()` søger kun efter `Person`-type. En merchant kan ikke logge ind via login-flowet. |
| G3 | **Fejlbesked skelner ikke mellem ukendt e-mail og forkert password** | 🟡 Medium | Frontend viser samme besked for E1 og E2 — brugeren kan ikke se om det er e-mail eller password der er forkert. |
| G4 | **Token refresh ikke implementeret** | 🟡 Medium | Når token udløber efter 8 timer (eller reelt 43200 min jf. config — se G5 i UC-01), skal brugeren logge ind igen manuelt. |
| G5 | **Hard reload ved login** | 🟢 Lav | `window.location.href = '/home'` foretager et fuldt side-reload. Angular Router kunne bruges i stedet for en mere flydende UX. |

---

## Tekniske noter

- **Ingen CSRF-beskyttelse**: Login sker via JSON POST uden CSRF-token.
- **Merchant-login via legacy seed**: Merchants oprettet via seed-scripts har ingen `PasswordHash` — de kan tilgås via e-mail alene da password-tjekket springes over når hash er null.
- **`LoginRequest.Password` er defineret men bruges ikke i frontend**: Server-siden er klar til password-validering, men frontend sender det aldrig.

---

## Relaterede use cases

- [UC-01 — Opret Bruger](UC-01-opret-bruger.md)
- [UC-03 — Log ud](UC-03-log-ud.md) *(ikke oprettet endnu)*
- [UC-04 — Opdater Profil](UC-04-opdater-profil.md) *(ikke oprettet endnu)*
