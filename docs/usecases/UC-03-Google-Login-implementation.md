# UC-03 – Google Login: Implementeringsstatus

## Overblik

Implementerer "Log ind med Google" (OAuth 2.0 / OIDC) som første fase af use case UC-03.
Apple login er Phase 2 og er ikke implementeret endnu.

---

## Implementerede dele

### ✅ DataStorage – ny entitet og migration

| Fil | Beskrivelse |
|-----|-------------|
| `Entities/ParticipantExternalLogin.cs` | Ny entitet: Id, ParticipantId (FK), Provider, ProviderUserId, Email, CreatedAtUtc |
| `Entities/Participant.cs` | Navigation-property `ExternalLogins` tilføjet |
| `Context/PayBySharePayDbContext.cs` | `DbSet<ParticipantExternalLogin>` + OnModelCreating konfiguration med unikt index på `(Provider, ProviderUserId)` og Cascade-sletning |
| `Repositories/IParticipantExternalLoginRepository.cs` | Interface: `GetByProviderAsync`, `AddAsync`, `SaveChangesAsync` |
| `Repositories/ParticipantExternalLoginRepository.cs` | EF Core implementering |
| `Extensions/DataStorageServiceExtensions.cs` | Repository registreret i DI |
| `Migrations/AddParticipantExternalLogin` | EF Core migration oprettet og kørt |

---

### ✅ Service – Google token-validering og find-eller-opret logik

| Fil | Beskrivelse |
|-----|-------------|
| `Interfaces/IExternalAuthService.cs` | Interface med `GoogleLoginAsync(idToken)` |
| `Services/ExternalAuthService.cs` | Implementering via `Google.Apis.Auth` (v1.75.0) |
| `ExternalLoginEmailConflictException.cs` | Custom exception når e-mail allerede er i brug med kodeord |
| `Extensions/ServiceLayerExtensions.cs` | Service registreret i DI |

**Flow i `ExternalAuthService.GoogleLoginAsync`:**
1. Validér Google ID-token via `GoogleJsonWebSignature.ValidateAsync` (verificerer signatur + audience)
2. Slå op på `(Provider="Google", ProviderUserId)` i `ParticipantExternalLogin`
3. Hvis fundet → returner den tilknyttede `Participant`
4. Hvis ikke fundet og e-mail allerede har en konto med kodeord → kast `ExternalLoginEmailConflictException` (409)
5. Ellers → opret ny `Participant` + `ParticipantExternalLogin`

---

### ✅ API – ny endpoint

| Fil | Beskrivelse |
|-----|-------------|
| `DTOs/ExternalLoginRequest.cs` | Record med `IdToken` |
| `Controllers/AuthController.cs` | `POST /api/auth/google-login` tilføjet |

**Endpoint-adfærd:**

| Scenarie | HTTP-svar |
|----------|-----------|
| Gyldigt token, bruger kendes | 200 OK + JWT |
| Gyldigt token, ny bruger | 200 OK + JWT (oprettet automatisk) |
| E-mail-konflikt med kodeordskonto | 409 Conflict + fejlbesked |
| Ugyldigt/udløbet token | 400 Bad Request |

---

### ✅ Konfiguration

| Fil | Ændring |
|-----|---------|
| `appsettings.json` | `"Google": { "ClientId": "REPLACE-WITH-GOOGLE-CLIENT-ID..." }` tilføjet |
| `appsettings.Simply.json` | `"Google": { "ClientId": "" }` placeholder tilføjet |
| `appsettings.Local.json` | `"Google": { "ClientId": "..." }` placeholder til lokal override |

---

### ✅ Frontend (Angular 19)

| Fil | Ændring |
|-----|---------|
| `src/index.html` | Google Identity Services SDK script-tag tilføjet (HTTPS) |
| `environments/environment.ts` | `googleClientId` tilføjet |
| `environments/environment.simply.ts` | `googleClientId` tilføjet |
| `environments/environment.test.ts` | `googleClientId` tilføjet |
| `core/services/auth.service.ts` | `googleLogin(idToken)` + `ExternalLoginRequest` interface tilføjet |
| `features/login/login.component.ts` | `OnInit` med `google.accounts.id.initialize` + `renderButton` + `_handleGoogleResponse` |
| `features/login/login.component.html` | `#google-signin-btn` div + "eller"-separator tilføjet over email/password-formen |
| `features/login/login.component.scss` | `.login__social` og `.login__divider` styles tilføjet |

---

## NuGet-pakker tilføjet

| Pakke | Version | Projekt |
|-------|---------|---------|
| `Google.Apis.Auth` | 1.75.0 | `Service.PayBySharePay` |

---

## Forretningsregler – opfyldelsesstatus

| Regel | Status | Note |
|-------|--------|------|
| BR-001: Intern UserId altid primær nøgle | ✅ | `Participant.Id` bruges overalt |
| BR-002: Provider kun til identitetsverifikation | ✅ | `ParticipantExternalLogin` er separat tabel |
| BR-003: Flere providers per bruger | ✅ | Arkitektur understøtter det via `ParticipantExternalLogin` |
| BR-004: Forbliv logget ind | ✅ | JWT gemmes i localStorage (eksisterende mekanisme) |
| BR-005: Genbrugeligt backend-flow | ✅ | `POST /api/auth/google-login` er platform-agnostisk |

---

## E-mail-konflikt håndtering

Hvis en bruger forsøger at logge ind med Google og e-mailen allerede eksisterer med et kodeord:
- Backend returnerer **409 Conflict** med besked:  
  *"E-mailen er allerede tilknyttet en konto oprettet med adgangskode. Log ind med adgangskode for at tilknytte Google til din konto."*
- Der sker **ingen automatisk merge** (jf. use case: "Require explicit verification")
- Fremtidig feature: "Link Google til eksisterende konto" (kræver separat endpoint)

---

## Krævet konfiguration inden ibrugtagning

### Google Cloud Console
1. Gå til [https://console.cloud.google.com/](https://console.cloud.google.com/)
2. Opret et OAuth 2.0 Client ID (Web application)
3. Tilføj **Authorized JavaScript origins**:
   - `https://mobil.paynsync.dk` (produktion)
   - `https://localhost:4200` (lokal udvikling)
4. Kopier Client ID og indsæt i:
   - Backend: `appsettings.Local.json` → `Google:ClientId`
   - Backend: `appsettings.Simply.json` → `Google:ClientId` (via deploy/secret)
   - Frontend: `environments/environment.ts` → `googleClientId`
   - Frontend: `environments/environment.simply.ts` → `googleClientId`

---

## Kendte mangler og gaps

| Gap | Prioritet | Note |
|-----|-----------|------|
| Apple Login | 🟡 Medium | Phase 2 – kræver JWKS-validering + Apple-konfiguration |
| Link Google til eksisterende kodeordskonto | 🟡 Medium | Kræver separat "link provider"-endpoint med verifikation |
| Refresh Token / session-fornyelse | 🟢 Lav | Eksisterende JWT-mekanisme er tilstrækkelig til MVP |
| Google One Tap (auto-prompt) | 🟢 Lav | Kan aktiveres med én linje i `ngOnInit` |

---

## Relaterede use cases

- `UC-02-log-ind.md` – email/password-login (uændret)
- `UC-01-opret-bruger.md` – brugeroprettelse (uændret)
- `UC-03-Login-with-Google-or-Apple.md` – den overordnede use case
