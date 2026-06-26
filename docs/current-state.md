# Current State

Statusoversigt over PayNSync pr. seneste kode-gennemgang.

**Symboler:**  
✅ Implementeret og fungerende  
⚠️ Delvist implementeret — virker men med begrænsninger  
❌ Ikke implementeret / kun planlagt

---

## Authentication & Brugere

| Feature | Status | Noter |
|---------|--------|-------|
| Login med email + password | ✅ | `POST /api/auth/login` — BCrypt-verifikation |
| Login uden password (legacy seed-brugere) | ✅ | Springer verifikation over hvis `PasswordHash` er null |
| Registrering (person) | ✅ | `POST /api/auth/register` — email-unikhed tjekkes |
| Registrering (merchant) | ✅ | `POST /api/auth/register-merchant` |
| Password hashing med BCrypt | ✅ | `Participant.PasswordHash` + `BCrypt.Verify()` |
| JWT udstedelse (HS256) | ✅ | Claims: `sub`, `name`, `jti` |
| JWT-validering i controllers | ⚠️ | Kun `OrdersController` har `[Authorize]` — se Security-sektion |
| Profilopdatering (navn, email, telefon) | ✅ | `PUT /api/participants/{id}/profile` |
| Token refresh | ❌ | Ikke implementeret |
| Roller/claims (host vs. deltager) | ❌ | Host-tjek sker i service-laget via sammenligning af IDs, ikke JWT-claims |

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
| Hent ordre-overblik (deltagere, linjer, betalinger, beskeder) | ✅ | `GET /api/orders/{id}/overview` |
| Ordre-statusmaskine | ✅ | `Collecting → ReadyToPay → HostApproved → Capturing → Paid / PartiallyFailed / Cancelled` |
| Auto-overgang til `ReadyToPay` ved alle `OrderSubmitted` | ✅ | `CheckAndSetReadyToPayAsync()` |
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
| Merchant modtager callback ved `Paid` | ✅ | `MerchantCallbackService` sender HTTP POST |
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
| Redirect URL returneres ved reserve | ⚠️ | URL returneres i response — frontend-håndtering ikke fuldt observerbar i koden |
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
| Opret ordre (4-trins wizard) | ✅ | Titel → merchant → deltagere → opret |
| Ordredetaljer med betalingsstatus pr. deltager | ✅ | |
| Host: godkend og capture-knap | ✅ | Kalder `POST /api/orders/{id}/approve` |
| Host: annuller ordre-knap | ✅ | Kalder `POST /api/orders/{id}/cancel` |
| Dansk betalingsstatus-label (`paymentStatusLabel()`) | ✅ | |
| Afventende deltagere (beregnet client-side) | ✅ | `computePendingSummary()` — kun `Invited`-status tæller |
| Send påmindelser til afventende deltagere | ⚠️ | Knap og dialog eksisterer — `sendReminders()` logger kun til console, ingen API-kald |
| Beskedindbakke | ✅ | |
| Brugerprofil | ✅ | |
| Mobil-first responsivt design | ✅ | |
| JWT-interceptor (token + 401-håndtering) | ✅ | Omdirigerer til `/login` ved 401 |
| Session i `localStorage` (`sbys_token`, `sbys_user`) | ✅ | |
| Real-time ordreOpdateringer | ❌ | Ingen polling eller WebSocket |
| Redirect-flow til MobilePay-app (bruger-redirect) | ⚠️ | Backend returnerer URL — ikke synligt i frontend-kode at den bruges |

---

## Infrastruktur og drift

| Feature | Status | Noter |
|---------|--------|-------|
| Swagger/OpenAPI tilgængeligt på `/` | ✅ | Root redirecter til `/swagger` |
| `ExceptionHandlingMiddleware` | ✅ | `ArgumentException` → 400, `KeyNotFoundException` → 404, `InvalidOperationException` → 409 |
| SQL Server EF Core med 9 migrationer | ✅ | |
| Dev: auto-start af Merchant Demo-server | ✅ | `MerchantDemoHostedService` starter `npx http-server` på port 8081 |
| CORS konfigureret | ✅ | Hardcodet liste med localhost + Azure-URL'er |
| Azure App Service (API) | ✅ | `web.config` + IIS in-process |
| Azure Static Web Apps (Angular SPA + Merchant Demo) | ✅ | `staticwebapp.config.json` med SPA-fallback |
| `Tools.PayBySharePay` (seed-scripts) | ✅ | `seed`, `seed-group-orders`, `seed-pizza`, `flush` m.fl. |
| Container/Docker | ❌ | Ikke konfigureret |
| CI/CD pipeline | ✅ | `build.yml`: byg + test ved push/PR til main. `deploy-simply.yml`: manuel deploy til Simply.com (`workflow_dispatch`) — API, landing, frontend og merchant-demo |
| Health checks | ❌ | Ikke konfigureret |
| Rate limiting | ❌ | Ikke konfigureret |

---

## Tests

| Suite | Status | Noter |
|-------|--------|-------|
| `FakePaymentProviderTests` | ✅ | DI-opløsning, reserve success/fejl, capture success/fejl, cancel, alle simulate-flags |
| `GroupPaymentOrchestrationServiceTests` | ✅ | Reserve, capture, cancel, idempotens, fejlscenarier — in-memory fakes |
| `ParticipantPaymentStateServiceTests` | ✅ | State machine-transitioner, ugyldige overgange, event log-skrivning |
| `UnitTest1` | ⚠️ | Tom testklasse — placeholder |
| Integrationstests | ❌ | Intet integrationstestprojekt |
| Frontend-tests | ❌ | Ingen Angular-testfiler (Karma/Jasmine konfigureret men ingen specs) |

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
| `UnauthorizedAccessException` → HTTP 500 | `ExceptionHandlingMiddleware` mapper ikke denne exception-type. Host-tjek i service-laget kaster `UnauthorizedAccessException`, som resulterer i 500 i stedet for 403. |
| `requestingParticipantId` fra request-body | Host-ejerskab valideres mod `requestingParticipantId` sendt af klienten i body — ikke mod JWT-claimet `sub`. En klient kan sende en anden brugers ID. |
| `DevController` uden auth i prod | `DELETE /api/dev/reset` sletter alle ordrer og `POST /api/dev/seed-merchant-urls` ændrer data. Begge endpoints har ingen authentication-krav. |
| JWT-udløbstid-inkonsistens | `appsettings.json` har `Jwt:ExpiresInMinutes = 43200` (30 dage). `AuthController` bruger `AddMinutes(480)` (8 timer) hardcodet. `JwtTokenService` læser konfigurationsværdien. Hvilken værdi der reelt bruges afhænger af kodestien. |
| Ingen Angular production environment-fil | `environment.ts` peger på `https://localhost:7007`. Der er ingen separat `environment.production.ts`. Det er uklart hvordan prod-API-URL'en sættes ved Angular-build. |
| `FriendRelation` race condition | `RelationExistsAsync` tjekker for duplikat i service, men der er inget unikt DB-constraint. Samtidige kald kan oprette dubletter. |
| `UnitTest1` er tom | Testklassen eksisterer med en tom `Test1()`-metode — placeholder uden indhold. |
| `MerchantOrderDraft.Status` bruges ikke aktivt | Entiteten har `Status = "Draft"` som default, men `MerchantOrderService` sætter `"Submitted"`. Systemets `ReadyToPay`-logik tjekker `OrderParticipant.Status`, ikke `MerchantOrderDraft.Status`. |

---

## Open Questions

1. **`ParticipantsController` uden auth** — Ingen `[Authorize]` attribut. Er det bevidst (public søgning for ikke-loggede brugere), eller en forglemmelse?
2. **JoinToken-endpoint** — `JoinToken` genereres på alle ordrer men ingen endpoint accepterer det. Er dette en planlagt feature?
3. **`Completed` vs. `Paid`** — To separate terminal-statuser eksisterer. `Paid` = provider-capture-flow afsluttet. `Completed` = det gamle manuelle flow. Skal de samles til én?
4. **Webhook-signatur** — Alle webhooks er `[AllowAnonymous]` uden HMAC-validering. En angriber kan sende falske status-opdateringer. Skal dette implementeres inden produktion?
5. **`requestingParticipantId` validering** — Bør det valideres mod `User.FindFirst(ClaimTypes.NameIdentifier)` fra JWT i stedet for at stole på request-body?
6. **`DevController` i prod** — Skal den beskyttes (fx `[Authorize]` + env-check) eller fjernes helt fra produktionsmiljøet?
7. **JWT-udløbstid** — Hvad er den korrekte udløbstid: 480 minutter (hardcodet i `AuthController`) eller 43200 (i `appsettings.json`)?
8. **Angular prod-build** — Hvordan sættes prod-API-URL'en? Via Angular build-konfiguration, environment-fil-swap, eller andet?
9. **`Declined`-status** — Defineret i frontend-enum og `participantStatusLabel()`. Planlægges der backend-logik til at håndtere dette?
10. **`Refunded`-status** — Transition `Captured → Refunded` er tilladt i state machine. Planlægges der et refund-endpoint?
