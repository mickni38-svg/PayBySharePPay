# Session 2026-07-03 – Vigtige beslutninger og ændringer

## Overblik

Denne session dækkede fire overordnede emner:
1. Google Login (UC-03 Phase 1)
2. Vipps test-bruger mapping på profilsiden
3. PWA / "Tilføj til hjemmeskærm"
4. Fejlrettelser og lokal udviklingsopsætning

---

## 1. Google Login (Phase 1)

### Beslutninger
- **Google Identity Services (GIS)** bruges til token-baseret login – ikke OAuth redirect-flow
- Google ID-token valideres server-side via `Google.Apis.Auth` (`GoogleJsonWebSignature.ValidateAsync`)
- Ekstern login-kobling gemmes i ny tabel `ParticipantExternalLogins` (Provider + ProviderUserId)
- Eksisterende email/password-login er uberørt
- Apple Login er **Phase 2** – ikke implementeret endnu

### Ny infrastruktur
| Komponent | Detalje |
|---|---|
| `ParticipantExternalLogin` entity | Ny EF-entity med `Provider`, `ProviderUserId`, `ParticipantId` |
| `IExternalAuthService` / `ExternalAuthService` | Validerer Google token, finder/opretter lokal bruger |
| `POST /api/auth/google-login` | Ny endpoint, `[AllowAnonymous]`, returnerer JWT |
| `GoogleClientId` | Konfigureres via `appsettings` / `appsettings.Local.json` |
| Migration | `AddParticipantExternalLogin` |

### Valg begrundelse
- Token-flow (frem for redirect) passer bedre til SPA og giver fuld kontrol over JWT-udstedelse
- `ClientSetNull` delete-behavior på alle self-referencing FKs (se sektion 2) for at undgå SQL Server cascade-fejl

---

## 2. Vipps Test-bruger Mapping (profil)

### Baggrund
Vipps sandbox-brugere har andre navne end app-brugere. Mapping-feature lader en logget ind bruger "pege på" en Vipps testperson, så betalingen vises korrekt i MobilePay-appen.

### Beslutninger
- **Midlertidig feature** – kun relevant i sandboxmiljø
- Mapping gemmes som `VippsTestUserId` (self-reference FK) på `Participant`-entiteten
- En Vipps-testperson kan kun mappes til **én** app-bruger ad gangen (exclusive selection)
- Allerede-mappede test-brugere vises som **disabled** i dropdown for andre brugere

### Ny infrastruktur
| Komponent | Detalje |
|---|---|
| `Participant.VippsTestUserId` | Nullable self-referencing FK |
| `DeleteBehavior.ClientSetNull` | Valgt i stedet for `SetNull`/`Cascade` pga. SQL Server multi-cascade-path fejl |
| `GetAllPersonsAsync()` | Ny repository-metode til at hente alle Person-type deltagere |
| `GetVippsTestPersonsAsync()` | Service-metode returnerer liste med `IsMapped`-flag |
| `SetVippsTestUserAsync()` | Service-metode, sætter/fjerner mapping |
| `GET /api/participants/vipps-test-users` | Ny endpoint |
| `PATCH /api/participants/{id}/vipps-test-user` | Ny endpoint |
| Migration | `AddVippsTestUserId` (regenereret efter FK-fix) |

---

## 3. PWA / "Tilføj til hjemmeskærm"

### Beslutninger
- `manifest.webmanifest` udfyldt med alle påkrævede felter (`name`, `short_name`, `display: standalone`, `theme_color`, `background_color`, `start_url`, `icons`)
- `@angular/service-worker` tilføjet til `package.json` og pinnet til **19.2.21** (match med resten af Angular 19)
- `provideServiceWorker()` registreret én gang i `app.config.ts` (duplikat fjernet)
- `ngsw-config.json` eksisterer allerede

### Rettelser i `index.html`
| Før | Efter |
|---|---|
| Duplikat `<link rel="manifest">` | Kun én manifest-link |
| `<title>PayBySharePay</title>` | `<title>PayNSync</title>` |
| Manglede `apple-mobile-web-app-title` | Tilføjet |
| Manglede `apple-mobile-web-app-status-bar-style` | Tilføjet (`black-translucent`) |
| Manglede `<link rel="apple-touch-icon">` | Tilføjet med `icon-152x152.png` |
| Manglede `<meta name="theme-color">` | Tilføjet (`#0f0f12`) |

### Angular assets (angular.json)
- `public/**/*`, `src/favicon.ico`, `src/assets`, `src/manifest.webmanifest` alle kopieres til build-output

---

## 4. Lokal udviklingsopsætning

### HTTPS lokalt (Angular)
- **Problem**: `ERR_CERT_AUTHORITY_INVALID` med Angular's selvsignerede cert / `origin_mismatch` hos Google
- **Løsning**: `mkcert` installeret og betroet root CA registreret i Windows trust store
- Certifikat genereret: `ssl/localhost+1.pem` + `ssl/localhost+1-key.pem` (gyldigt til okt. 2028)
- `ssl/` tilføjet til `.gitignore` – private nøgler committes ikke
- `angular.json` opdateret med `"ssl": true`, `"sslCert"` og `"sslKey"` for development-konfigurationen

### Google Cloud Console
- Registrerede origins: `https://localhost:4200` (lokal) + `https://mobil.paynsync.dk` (produktion)

### MerchantDemo (Pizzeria Roma)
- `MerchantDemoHostedService` udvidet til at starte i **både** `Development` og `Local`-miljøer (ikke kun `Development`)
- `API_BASE` i `index.html` rettet fra `http://localhost:5071` → `https://localhost:7007` (eliminerer CORS-fejl ved HTTP→HTTPS-redirect)

### Vipps token-logging
- `VippsMobilePayTokenService` logger nu det præcise HTTP-svar (statuskode + body) ved fejl, inden `EnsureSuccessStatusCode()` kastes

---

## 5. SQL-migrations

| Migration | Indhold |
|---|---|
| `AddParticipantExternalLogin` | Ny tabel `ParticipantExternalLogins` med unik index på `(Provider, ProviderUserId)` |
| `AddVippsTestUserId` | Nullable self-ref FK `VippsTestUserId` på `Participants`, `ClientSetNull` delete-behavior |

`migrations_remote.sql` regenereret (idempotent script til brug på Simply.com produktionsdatabasen).

---

## 6. Hvad mangler stadig (kendte gaps)

| Emne | Prioritet |
|---|---|
| Apple Login (Phase 2) | 🟡 Medium |
| "Kobl Google til eksisterende password-konto" flow | 🟡 Medium |
| Webhook signatur-validering (Vipps) | 🔴 Høj |
| Vipps test-bruger mapping fjernes når sandbox ikke længere bruges | 🟢 Lav |
| PWA-installabilitet kræver HTTPS i produktion | ✅ Allerede opfyldt på `mobil.paynsync.dk` |
