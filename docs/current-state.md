# Current State

Statusoversigt over PayNSync pr. seneste kodegennemgang.

**Senest opdateret:** 2. september 2026 — UC-15 profil- og kontocenter er implementeret og verificeret på `main`.

**Symboler:**  
✅ Implementeret og fungerende  
⚠️ Delvist implementeret — virker men med begrænsninger  
❌ Ikke implementeret / kun planlagt

---

## Authentication & Brugere

| Feature | Status | Noter |
|---------|--------|-------|
| Login med email + password | ✅ | `POST /api/auth/login` — BCrypt-verifikation |
| Login uden password (legacy seed-brugere) | ✅ | Kun passwordløse Person-seedkonti i ASP.NET Core `Development`; afvises for konti med hash, merchants og andre miljøer |
| Registrering (person) | ✅ | `POST /api/auth/register` — email-unikhed tjekkes |
| Registrering (merchant) | ✅ | Kontocenter og API kræver vist navn, firmanavn, konto-email, password og Vipps MSN; password hashes med BCrypt |
| Password hashing med BCrypt | ✅ | `Participant.PasswordHash` + `BCrypt.Verify()` |
| JWT udstedelse (HS256) | ✅ | Claims: `sub`, `name`, `jti` |
| JWT-validering i controllers | ✅ | `OrdersController` kræver JWT; host-handlinger udleder bruger-ID fra `NameIdentifier`/`sub` og ignorerer body-ID |
| Google login (`POST /api/auth/google-login`) | ✅ | `ExternalAuthService` — validerer Google ID-token via `Google.Apis.Auth`; opretter/finder `Participant` + `ParticipantExternalLogin` — *(NYT)* |
| `ParticipantExternalLogin` (Google-tilknytning) | ✅ | Tabel til externe OAuth-logins; Provider + ProviderUserId + Email — *(NYT)* |
| Profilopdatering (navn, email, telefon) | ✅ | `PUT /api/participants/{id}/profile` |
| Merchant-login med email + password | ✅ | Fælles login-opslag for Person og Merchant; responsen indeholder participant-type |
| Token refresh | ❌ | Ikke implementeret |
| Host-autorisation via JWT-identitet | ✅ | Host-ejerskab sammenlignes med det autentificerede participant-ID fra JWT |

---

## Ordre-håndtering

| Feature | Status | Noter |
|---------|--------|-------|
| Opret ordre (titel/kategori, valgfri merchant + deltagere) | ✅ | `POST /api/orders` |
| Host tilføjes automatisk som accepteret deltager | ✅ | Status `Accepted`, unikt `ParticipantToken` |
| Deltagere tilføjes med status `Invited` + unikt `ParticipantToken` | ✅ | |
| Tilknyt merchant til ordre | ✅ | `Order.MerchantParticipantId` |
| Generer merchant-link pr. deltager + send som besked | ✅ | `{merchant.GroupOrderUrl}?orderId=X&merchantId=Y&participantToken=Z` |
| Generalbesked til deltagere uden merchant | ✅ | Sendes kun til inviterede, ikke host |
| Hent alle ordrer / ordrer for én bruger | ✅ | `GET /api/orders?participantId=X` |
| Hent ordre-overblik (merchant-logo, deltagere, linjer, betalinger, beskeder) | ✅ | `GET /api/orders/{id}/overview` |
| Ordre-statusmaskine | ✅ | `Collecting → ReadyToPay → HostApproved → Capturing → Paid / PartiallyFailed / Cancelled` |
| Auto-overgang til `ReadyToPay` når alle deltagerbetalinger er `Reserved` | ✅ | `CheckAndSetReadyToPayByReservedAsync()` — kaldes fra Vipps-webhook og FakeProvider |
| `CheckAndSetReadyToPayAsync` (OrderSubmitted-baseret) | ⚠️ | Metoden eksisterer i `IOrderService` men kaldes ingen steder i produktionskode — `ReadyToPay` sættes via den Reserved-baserede metode |
| Tilføj deltagere efter ordreoprettelse | ❌ | Ingen endpoint |
| Join ordre via `JoinToken`-link | ❌ | `JoinToken` genereres men ingen endpoint bruger det |

---

## Merchant-bestillingsflow

| Feature | Status | Noter |
|---------|--------|-------|
| Merchant Demo — Pizzeria Roma (statisk HTML) | ✅ | Læser `orderId`, `merchantId`, `participantToken` fra URL |
| Indsend bestilling fra merchant-side | ✅ | `POST /api/merchant-orders` (anonymous) |
| Valider `ParticipantToken` mod database | ✅ | `MerchantOrderService.InitOrderAsync()` |
| Én draft pr. deltager pr. ordre — gen-indsendelse erstatter | ✅ | Forrige draft slettes |
| Alle ordrelinjer tildeles `ParticipantId` | ✅ | Fra den validerede `OrderParticipant` |
| Betalingsreservation startes automatisk ved draft-indsendelse | ✅ | `MerchantOrderService` kalder `ReserveParticipantPaymentAsync()` |
| Hent draft for en ordre | ✅ | `GET /api/merchant-orders/by-order/{orderId}` |
| Merchant modtager callback ved `Paid` | ✅ | `MerchantCallbackService` sender HTTP POST med `PayNSyncFinalGroupOrderDto` |
| `FinalGroupOrderDtos` (PayNSyncFinalGroupOrderDto m.fl.) | ✅ | Implementeret — standard GroupOrderPaid-payload til merchant-callback — *(NYT)* |
| `RawMerchantPayloadJson` på `MerchantOrderDraft` | ✅ | Gemmer merchantens originale JSON til audit/debugging (nullable) — *(NYT)* |
| Hardcodet Pizzeria Roma-menu i demo | ⚠️ | Kun én fast menu — ikke konfigurerbar |
| Kun én merchant per ordre | ⚠️ | Arkitekturen understøtter ikke flere merchants pr. ordre |

---

## Betalinger (Provider-backed)

| Feature | Status | Noter |
|---------|--------|-------|
| `FakePaymentProvider` — reserve, capture, cancel, status | ✅ | Synkron success uden HTTP-kald |
| `FakePaymentProvider` — fejlsimulering via konfiguration | ✅ | 6 flags: `SimulateReservation/Capture/CancelFailed`, `...Expired`, `...Exception` |
| `MobilePaySandboxPaymentProvider` — reserve | ✅ | `POST /epayment/v1/payments` mod Vipps |
| `MobilePaySandboxPaymentProvider` — capture | ✅ | `POST /epayment/v1/payments/{id}/capture` |
| `MobilePaySandboxPaymentProvider` — cancel | ✅ | `POST /epayment/v1/payments/{id}/cancel` |
| `MobilePaySandboxPaymentProvider` — status | ✅ | `GET /epayment/v1/payments/{id}` |
| Vipps OAuth2 token-caching (SemaphoreSlim) | ✅ | Fornys 5 min før udløb |
| Vipps test-user mapping (`VippsTestUserId` self-ref FK) | ✅ | `GET /api/participants/vipps-test-users` + `PATCH /api/participants/{id}/vipps-test-user` — *(NYT)* |
| Per-merchant Vipps-credentials (VippsClientId, ClientSecret, SubscriptionKey, MSN) | ✅ | Felter på `Participant` — null = brug global fra appsettings — *(NYT)* |
| Reserver betaling pr. deltager | ✅ | `POST /api/orders/{id}/reserve` |
| Idempotent reserve (returnerer eksisterende) | ✅ | Springer over hvis ikke-cancelled/failed betaling eksisterer |
| Host godkender + capture alle reserverede betalinger | ✅ | `POST /api/orders/{id}/approve` |
| Idempotent capture (springer allerede-captured over) | ✅ | |
| Retry capture efter `PartiallyFailed` | ✅ | Kræver re-kald af `/approve` |
| Annuller ordre (annuller alle reservationer) | ✅ | `POST /api/orders/{id}/cancel` |
| `PaymentEventLog` — audit trail pr. state-skift | ✅ | Skrives ved alle `ParticipantPaymentStateService`-kald |
| Alle state-machine-metoder er idempotente | ✅ | Allerede i målstatus returnerer uden fejl |
| Capture-status endpoint | ✅ | `GET /api/orders/{id}/capture-status` |
| Generisk webhook-endpoint (provider) | ✅ | `POST /api/payments/webhooks/provider` |
| MobilePay webhook-alias | ✅ | `POST /api/payments/webhooks/mobilepay` |
| Vipps callback-endpoint | ✅ | `POST /api/payments/vipps/callbacks/{reference}` |
| `RowVersion` (optimistisk concurrency på `ParticipantPayment`) | ✅ | Konfigureret i EF Core |
| Redirect til Vipps/MobilePay efter reservation | ✅ | API'et returnerer `PaymentRedirectUrl`, og Merchant Demo navigerer browseren til URL'en; Fake provider fortsætter uden redirect |
| Webhook-signatur-validering (HMAC) | ❌ | `[AllowAnonymous]` — ingen signaturcheck |
| Refund-flow | ❌ | `Refunded`-status defineret i enum og transitions — ingen service-metode implementeret |
| Produktions-Vipps-credentials | ❌ | Placeholder-værdier i `appsettings.json` |
| ngrok-opsætning til Vipps-webhooks i dev | ⚠️ | Kun placeholder i konfiguration — skal opsættes manuelt |

---

## Legacy betalingsflow

| Feature | Status | Noter |
|---------|--------|-------|
| `POST /api/payments` — registrer simpel betaling | ✅ | Opretter `Payment`-record + sender host-besked |
| `POST /api/orders/{id}/pay` — dummy eksternt API | ✅ | `ExternalPaymentService` returnerer altid success |
| `POST /api/orders/{id}/complete` — sæt ordre til Completed | ✅ | Kræver host + ordre i `ReadyToPay` |

---

## Beskeder

| Feature | Status | Noter |
|---------|--------|-------|
| System-genererede invitationsbeskeder ved ordreoprettelse | ✅ | Merchant-link eller generel tekst |
| System-genereret `ReadyToPay`-notifikation til host | ✅ | Sendes med link til `/orders` |
| System-genereret betalingsbekræftelse til host (legacy) | ✅ | Sendes fra `PaymentService` |
| Manuel beskedoprettelse | ✅ | `POST /api/messages` |
| Hent beskeder for en ordre | ✅ | `GET /api/messages/order/{orderId}` |
| Hent beskeder for en deltager | ✅ | `GET /api/messages/by-participant/{participantId}` |
| Antal ulæste beskeder | ✅ | `GET /api/messages/unread-count?participantId=X` |
| Markér alle som læst | ✅ | `POST /api/messages/mark-read?participantId=X` |
| Push-notifikationer | ❌ | Ikke implementeret |
| Real-time opdateringer (SignalR/WebSocket) | ❌ | Ikke implementeret |

---

## Venner og katalog

| Feature | Status | Noter |
|---------|--------|-------|
| Tilføj ven (øjeblikkelig, ingen accept) | ✅ | `POST /api/friends` — duplikat-tjek i service |
| Hent venneliste | ✅ | `GET /api/friends/{participantId}` |
| Katalog-søgning (persons + merchants) | ✅ | `GET /api/directory/search` |
| Hent venner via directory | ✅ | `GET /api/directory/{participantId}/friends` |
| Ven-anmodning med accept-flow | ❌ | Ingen to-trins-accept |

---

## Frontend (Angular 19 SPA)

| Feature | Status | Noter |
|---------|--------|-------|
| Login og registrering | ✅ | |
| Ordreoversigt med aktiv/afsluttet tabs | ✅ | |
| Opret ordre (3-trins wizard, UC-03–UC-05) | ✅ | Trin 1: låst merchant og dynamiske deltagere. Trin 2: titel (80 tegn) og valgfri besked (500 tegn). Trin 3: dynamisk kontrolside, idempotent oprettelse og navigation til den nye ordres detaljeside. Ændringer foretages via Tilbage-knappen; ingen direkte Redigér-genveje. |
| Ordredetaljer med betalingsstatus pr. deltager | ✅ | |
| Host: godkend og capture-knap | ✅ | Kalder `POST /api/orders/{id}/approve` |
| Host: annuller ordre-knap | ✅ | Kalder `POST /api/orders/{id}/cancel` |
| Dansk betalingsstatus-label (`paymentStatusLabel()`) | ✅ | |
| Afventende deltagere (beregnet client-side) | ✅ | `computePendingSummary()` — kun `Invited`-status tæller |
| Send påmindelser til afventende deltagere | ⚠️ | Knap og dialog eksisterer — `sendReminders()` logger kun til console, ingen API-kald |
| Beskedindbakke | ✅ | |
| Profil- og kontocenter (UC-15) | ✅ | `/profile` samler profil, login, person-/merchantoprettelse, rollebeskyttet Vipps-test og Development-only værktøjer i lazy-loadede faner; `/login` og `/register` redirecter hertil |
| Merchant-logo i database/API og statisk demo-katalog (UC-01) | ✅ | Logo-data og metadata på merchant, logo-endpoint, validering/fallback samt visning i carousel, venneliste og ordreoverblik. |
| Merchant-søgning og carousel på forsiden (UC-02) | ✅ | Dynamiske merchant-venner, maks. 8, søgning, logo-fallback, senest anvendt-sortering, tastaturnavigation og låst wizard-state |
| Dark theme navigation (UC-07) | ✅ | Mørkt tema skjuler Deltagere+Profil-kort, giver neon-glow border på kort, forenkler bottom nav til Hjem/Deltagere/Mere |
| PayNSync hero-logo på forsiden (dark mode) | ✅ | SVG-kreditkortsillustration + Pay/NSync branding øverst på HomeComponent |
| Mobil-first responsivt design | ✅ | |
| JWT-interceptor (token + 401-håndtering) | ✅ | Omdirigerer til `/login` ved 401 |
| Session i `localStorage` (`sbys_token`, `sbys_user`) | ✅ | |
| Real-time ordreOpdateringer | ❌ | Ingen polling eller WebSocket |
| Redirect-flow til MobilePay-app (bruger-redirect) | ⚠️ | Backend returnerer URL — ikke synligt i frontend-kode at den bruges |

---

## Infrastruktur og drift

| Feature | Status | Noter |
|---------|--------|-------|
| Swagger/OpenAPI tilgængeligt på `/` | ✅ | Root redirecter til `/swagger`; `DevController` opdages og vises kun i `Development` |
| `ExceptionHandlingMiddleware` | ✅ | `ArgumentException` → 400, `UnauthorizedAccessException` → 403, `KeyNotFoundException` → 404, `InvalidOperationException` → 409 |
| SQL Server EF Core med 15 migrationer | ✅ | Seneste migration: `20260815173756_AddMerchantLogo` |
| Dev: auto-start af Merchant Demo-server | ✅ | `MerchantDemoHostedService` starter `npx http-server` på port 8081 |
| CORS konfigureret | ✅ | Hardcodet liste med localhost + Azure-URL'er |
| Simply.com API (IIS) | ✅ | Self-contained Windows x64 publish til `api.paynsync.dk` |
| Simply.com webklienter | ✅ | Landing, Angular SPA og Merchant Demo deployes til deres respektive domæner |
| `Tools.PayBySharePay` (seed-scripts) | ✅ | `seed`, `seed-group-orders`, `seed-pizza`, `flush` m.fl. |
| Container/Docker | ❌ | Ikke konfigureret |
| CI: Build & Test | ✅ | `.github/workflows/build.yml` kører automatisk ved push/PR til `main`: .NET build/tests samt Angular tests og Simply-build |
| CD: Simply.com deploy | ✅ | `.github/workflows/deploy-simply.yml` bygger og deployer manuelt via `workflow_dispatch`; ingen automatisk produktionsdeploy ved push |
| Health checks | ❌ | Ikke konfigureret |
| Rate limiting | ❌ | Ikke konfigureret |

---

## Tests

| Suite | Status | Noter |
|-------|--------|-------|
| `FakePaymentProviderTests` | ✅ | DI-opløsning, reserve success/fejl, capture success/fejl, cancel, alle simulate-flags |
| `GroupPaymentOrchestrationServiceTests` | ✅ | Reserve, capture, cancel, idempotens, fejlscenarier — in-memory fakes |
| `ParticipantPaymentStateServiceTests` | ✅ | State machine-transitioner, ugyldige overgange, event log-skrivning |
| `OrdersControllerAuthorizationTests` | ✅ | JWT-identitet, manipuleret body-ID, ugyldige claims, host/non-host `/pay`, `[Authorize]` og middleware 403 |
| `DevelopmentOnlyControllerFeatureProviderTests` | ✅ | DevController registreres i Development, fjernes i Simply/Production/Local/andre miljøer, mens almindelige controllers bevares |
| `UnitTest1` | ⚠️ | Tom testklasse — placeholder |
| Integrationstests | ❌ | Intet integrationstestprojekt |
| Frontend-tests | ✅ | 42 Karma/Jasmine-tests, herunder UC-01–UC-05 samt UC-15 auth-session, kontotype, merchantpayload, navigation og lazy fanedata |

---

## Planlagte use cases

| Use case | Status | Formål |
|----------|--------|--------|
| UC-08 – JWT-identitet og host-autorisation | ✅ Implementeret | Host-handlinger bruger JWT-identitet; ugyldig identitet giver 401 og manglende ejerskab 403 |
| UC-09 – Beskyt dev-endpoints | ✅ Implementeret | Hele `DevController` fjernes fra routing og Swagger uden for `Development` |
| UC-10 – Vipps webhook HMAC | ❌ Planlagt | Verificér webhook-secret, body-hash og HMAC før stateændring |
| UC-11 – Ens JWT-udløbstid | ❌ Planlagt | Ensret tokenets `exp` og auth-responsens `ExpiresAt` |
| UC-12 – Send påmindelser | ❌ Planlagt | Erstat frontend-placeholder med eksisterende Message-flow |
| UC-13 – Join med token | ❌ Planlagt | Aktivér eksisterende `JoinToken` efter Product Owner-beslutninger |
| UC-14 – Refund captured betaling | ❌ Planlagt | Idempotent refund gennem provider/state machine efter betalingsbeslutning |
| UC-15 – Profil- og kontocenter | ✅ Implementeret | Faner til profil/login/oprettelse, person- og merchantkonto, Vipps-testmapping og development-only værktøjer |

Se `docs/usecases/00-IMPLEMENTATION-ORDER.md` for anbefalet rækkefølge og modelprofil.

---

## Kendte begrænsninger

| Begrænsning | Beskrivelse |
|-------------|-------------|
| Én merchant pr. ordre | Arkitekturen tillader kun ét `MerchantParticipantId` pr. `Order` |
| Hardcodet merchant-menu | Pizzeria Roma-menuen er statisk HTML — ikke konfigurerbar per merchant |
| Ingen post-oprettelse deltager-tilføjelse | Deltagere kan kun tilføjes ved ordreoprettelse |
| Hardcodet CORS-liste | Nye frontend-URL'er kræver kodeændring i `Program.cs` |
| Capture stopper ved første fejl | `PartiallyFailed` sættes og loop afbrydes — de øvrige deltageres betalinger fanges ikke |
| Én `ParticipantToken` per deltager | Token kan ikke regenereres — kompromitteret token kræver ny `OrderParticipant` |

---

## Kendte tekniske gældspunkter

| Gældspunkt | Beskrivelse |
|-----------|-------------|
| `ExternalPaymentService` er en stub | Har `TODO`-kommentarer. `ChargeAsync()` simulerer 300ms forsinkelse og returnerer altid success. Bruges stadig af `/pay`-endpoint. |
| Login-password kan omgås | `AuthController.Login` verificerer kun password, når requesten indeholder et ikke-tomt password. En klient kan derfor undlade password og få JWT for en fundet Person. UC-15 kræver 401 uden password uden for eksplicit Development seed-login. |
| Merchant auth-kontrakt er ufuldstændig | Frontend mangler påkrævet Vipps MSN; merchant oprettes uden `Participant.Email`/`PasswordHash`, og login tillader kun `Person`. Planlagt i UC-15. |
| Udviklerpanel vises i production-frontend | UC-09 beskytter backend med 404, men profilen environment-skjuler endnu ikke udviklerpanelet. Planlagt i UC-15. |
| JWT-udløbstid-inkonsistens | `appsettings.json` har `Jwt:ExpiresInMinutes = 43200` (30 dage). `AuthController` bruger `AddMinutes(480)` (8 timer) hardcodet. `JwtTokenService` læser konfigurationsværdien. Hvilken værdi der reelt bruges afhænger af kodestien. |
| `FriendRelation` race condition | `RelationExistsAsync` tjekker for duplikat i service, men der er inget unikt DB-constraint. Samtidige kald kan oprette dubletter. |
| `UnitTest1` er tom | Testklassen eksisterer med en tom `Test1()`-metode — placeholder uden indhold. |
| `MerchantOrderDraft.Status` bruges ikke aktivt | Entiteten har `Status = "Draft"` som default, men `MerchantOrderService` sætter `"Submitted"`. Systemets `ReadyToPay`-logik tjekker `OrderParticipant.Status`, ikke `MerchantOrderDraft.Status`. |
| `Google:ClientId` tom i `appsettings.json` | `appsettings.json` har `"Google": {"ClientId": ""}`. Google-login virker kun, når `ClientId` er sat i `appsettings.Local.json` eller `appsettings.Simply.json`. — *(NYT)* |
| Per-merchant Vipps-credentials i database | `VippsClientId`, `VippsClientSecret`, `VippsSubscriptionKey` gemmes som klartekst på `Participant`-entiteten. Ingen kryptering. — *(NYT)* |

---

## Open Questions

1. **`ParticipantsController` uden auth** — Ingen `[Authorize]` attribut. Er det bevidst (public søgning for ikke-loggede brugere), eller en forglemmelse?
2. **JoinToken-endpoint** — `JoinToken` genereres på alle ordrer men ingen endpoint accepterer det. Er dette en planlagt feature?
3. **`Completed` vs. `Paid`** — To separate terminal-statuser eksisterer. `Paid` = provider-capture-flow afsluttet. `Completed` = det gamle manuelle flow. Skal de samles til én?
4. **Webhook-signatur** — Alle webhooks er `[AllowAnonymous]` uden HMAC-validering. En angriber kan sende falske status-opdateringer. Skal dette implementeres inden produktion?
5. **JWT-udløbstid** — Hvad er den korrekte udløbstid: 480 minutter (hardcodet i `AuthController`) eller 43200 (i `appsettings.json`)?
6. **`Declined`-status** — Defineret i frontend-enum og `participantStatusLabel()`. Planlægges der backend-logik til at håndtere dette?
7. **`Refunded`-status** — Transition `Captured → Refunded` er tilladt i state machine. Planlægges der et refund-endpoint?
