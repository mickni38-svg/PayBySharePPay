# Architecture

## Lagdeling – overblik

```
┌──────────────────────────────────────────────────────────────────────┐
│  Frontend.PayBySharePay  (Angular 19 SPA, port 4200)                 │
│  Frontend.MerchantDemo   (statisk HTML/JS, port 8081)                │
└────────────────────────┬─────────────────────────────────────────────┘
						 │ HTTP/JSON  (JWT Bearer)
┌────────────────────────▼─────────────────────────────────────────────┐
│  Api.PayBySharePay  (ASP.NET Core 9, port 5071 / HTTPS 7007)        │
│  Controllers · DTOs · JwtTokenService · ExceptionHandlingMiddleware  │
│  MerchantCallbackService · MerchantDemoHostedService (dev only)      │
└────────────────────────┬─────────────────────────────────────────────┘
						 │ kun interfaces
┌────────────────────────▼─────────────────────────────────────────────┐
│  Service.PayBySharePay  (forretningslogik)                           │
│  OrderService · GroupPaymentOrchestrationService                     │
│  MerchantOrderService · PaymentService (legacy)                      │
│  ParticipantService · MessageService · DirectoryService              │
│  ParticipantPaymentStateService                                       │
└──────────────┬──────────────────────────────┬────────────────────────┘
			   │ repositories                  │ IPaymentProvider
┌──────────────▼───────────────┐  ┌────────────▼────────────────────────┐
│  DataStorage.PayBySharePay   │  │  Infrastructure.Payments             │
│  EF Core + SQL Server        │  │  FakePaymentProvider                 │
│  Entities · Repositories     │  │  MobilePaySandboxPaymentProvider     │
│  Migrations                  │  │  VippsMobilePayTokenService          │
└──────────────────────────────┘  └─────────────────────────────────────┘
													  │ HTTPS
										 ┌────────────▼──────────────────┐
										 │  Vipps MobilePay ePayment API │
										 │  (apitest.vipps.no / prod)    │
										 └───────────────────────────────┘
```

---

## Produktarkitektur: modulær PayNSync-platform

PayNSync skal arkitektonisk behandles som en **modulær platform**. Gruppebetaling er en selvstændig capability/plugin-feature og må ikke være teknisk afhængig af PayNSync Order Hub, Merchant Website eller en bestemt merchant-klient.

Der skelnes mellem følgende produktområder:

| Produktområde | Rolle | Obligatorisk for GroupOrder? |
|---|---|---|
| **PayNSync GroupOrder / Group Payment** | Kerne-capability der orkestrerer gruppeordre, deltagere, individuelle ordrekladder, reservationer, capture og opbygning af én færdig merchant-ordre | Ja — dette er kerneproduktet |
| **PayNSync Order Hub** | Merchant-side ordreplatform/backend, der kan modtage, persistére og styre færdige merchant-ordrer | Nej |
| **Merchant Order App** | Tablet-/webklient, fx PWA på iPad, der viser og behandler ordrer fra Order Hub | Nej |
| **Merchant Website** | Kundevendt menu/webshop, som kan starte almindelige ordrer eller PayNSync GroupOrder-flow | Nej |
| **Ekstern merchant-integration** | Adapter til et eksisterende ordresystem/POS/platform, fx en fremtidig OrderYOYO-integration | Nej |

### To primære merchant-scenarier

**Merchant med eksisterende ordresystem**

```text
Merchant Website / eksisterende platform
        |
        +--> Normal bestilling ------------------> eksisterende ordresystem
        |
        +--> "Bestil sammen" / PayNSync GroupOrder
                         |
                         v
                 PayNSync Group Payment
                         |
                  final merchant order
                         |
                         v
                 Merchant adapter
                         |
                         v
              eksisterende ordresystem/POS
```

Her skal merchant som udgangspunkt kun bruge PayNSyncs GroupOrder/Group Payment-feature. PayNSync Order Hub er ikke en dependency.

**Merchant uden eget ordresystem**

```text
Merchant Website eller PayNSync entry point
        |
        v
PayNSync GroupOrder / almindelig ordre
        |
        v
PayNSync Order Hub
        |
        +--> Merchant Order App / iPad PWA
        +--> Merchant Web UI
        +--> senere andre interne klienter
```

Her kan PayNSync tilbyde den komplette merchant-løsning: ordre-backend, ordrekø og merchant-klient.

### Order Hub betyder backend/service — ikke kun iPad-UI

Begrebet **PayNSync Order Hub** refererer arkitektonisk til merchantens ordreplatform/backend-capability. En iPad/PWA er en klient ovenpå Order Hub og må ikke være selve definitionen af Order Hub.

Order Hub har som target-ansvar at:

- modtage én færdig merchant-ordre efter successful group-payment capture
- persistére merchant-ordren permanent og idempotent
- eje merchantens ordrestatus og ordrekø
- eksponere API'er til merchant-klienter
- understøtte genindlæsning/recovery efter forbindelsestab
- kunne modtage ordrer fra flere PayNSync-kilder, ikke kun GroupOrder

Order Hub må **ikke** eje:

- deltagerinvitationer
- participant payment state
- Vipps/MobilePay reservation/capture
- GroupOrder-orkestrering
- logikken der afgør hvornår alle deltagere er klar til capture

Disse ansvar forbliver i GroupOrder/Group Payment-domænet.

### Order destination / adapter boundary

Når PayNSync har opbygget en færdig merchant-ordre, skal downstream-levering behandles som en separat integrationsgrænse.

Konceptuelt target:

```text
GroupOrder / Group Payment
        |
        v
PayNSyncFinalGroupOrder
        |
        v
Order Destination boundary
        |
        +--> PayNSync Order Hub
        +--> Generic merchant webhook
        +--> ekstern POS-/platform-adapter
        +--> e-mail/fallback
        +--> fremtidige destinationer
```

Eksisterende `IMerchantOrderSender`/merchant-callback-mønster er den nuværende begyndelse på denne grænse. Fremtidige use cases må udvide den uden at gøre GroupOrder afhængig af Order Hub.

En konkret adapter til fx OrderYOYO er **ikke implementeret** og må ikke dokumenteres som eksisterende integration. Arkitekturreglen er alene, at en sådan integration skal kunne tilføjes som destination/adapter uden at ændre Group Payment-kernen.

### Implementeret Order Hub boundary (UC-22)

UC-22 implementerer Order Hub som en logisk backend-capability i den eksisterende solution:

```text
MerchantOrder (permanent, Paid)
        |
        v
OrderHubService
  - merchant isolation
  - OrderHubEnabled
  - status state machine
        |
        v
OrderHubController /api/order-hub
        |
        v
Merchant Order App /order-hub
  - Angular/PWA
  - polling
  - local alarm sound
```

`MerchantOrder` er source of truth. Group Payment opretter fortsat den permanente ordre efter full capture; Order Hub læser denne direkte og kræver derfor ikke et eksternt merchant-callback. UC-21's `MerchantOrderUrl`-adapterflow eksisterer fortsat parallelt for merchants med eksternt ordresystem.

Order Hub-status er separat fra payment-status og må kun følge `New → Accepted → Preparing → Ready → Completed`.

### Ikke en microservice-krav

Den logiske produktgrænse betyder **ikke**, at Order Hub straks skal være en separat .NET solution, database eller deployment.

I første version må Group Payment og Order Hub godt leve i den eksisterende PayBySharePay solution og samme deployede API, hvis de holdes adskilt gennem klare services, contracts og dependencies.

En senere fysisk opsplitning til selvstændig service er en deployment-/skaleringsbeslutning og må ikke være nødvendig for at opnå den logiske modulgrænse.

### Arkitekturregler for nye use cases

1. GroupOrder/Group Payment må ikke kræve Order Hub, medmindre use casen eksplicit handler om Order Hub.
2. Merchant Order App og Merchant Website må ikke indeholde Group Payment-forretningslogik.
3. Eksterne merchant-systemer skal integreres gennem en adapter/destination boundary frem for speciallogik inde i Group Payment.
4. Order Hub må konsumere den standardiserede færdige merchant-ordre på samme princip som en ekstern destination.
5. En merchant skal kunne vælge mellem eksisterende ordresystem og PayNSync Order Hub uden at ændre selve Group Payment-flowet.
6. Færdig ordre må fortsat først frigives downstream efter de gældende capture- og betalingsregler.
7. Target-arkitektur og implementeret current state skal dokumenteres separat; planlagte adapters eller Order Hub-funktioner må ikke beskrives som implementerede.

## Afhængighedsregler

| Fra | Til | Hvordan |
|-----|-----|---------|
| `Api` | `Service` | Injicerer interfaces fra `Service.PayBySharePay.Interfaces` |
| `Api` | `DataStorage` | Kun DI-registrering via `AddDataStorage()` i `Program.cs` |
| `Api` | `Infrastructure` | Kun DI-registrering via `AddPaymentInfrastructure()` |
| `Service` | `DataStorage` | Bruger repositories og entities direkte |
| `Infrastructure` | `Service` | Implementerer `IPaymentProvider` fra `Service.PayBySharePay.Interfaces` |
| `DataStorage` | — | Ingen afhængigheder udover EF Core |
| `Tests` | `Service` + `DataStorage` + `Infrastructure` | Fakes erstatter repositories; FakePaymentProvider bruges direkte |

---

## Frontend-teknologi (`Frontend.PayBySharePay`)

**Framework:** Angular 19 (`@angular/core ^19.0.0`)  
**Pattern:** Standalone components (ingen NgModule), lazy-loaded routes, zoneless-klar (`eventCoalescing: true`)  
**State:** Angular Signals (`signal()`, `computed()`) — ingen NgRx eller anden global state manager  
**HTTP:** `HttpClient` med én funktionel interceptor (`apiInterceptor`)  
**Styling:** SCSS + CSS (per komponent)

### JWT-interceptor

`apiInterceptor` kører på alle udgående kald:
- Sætter `Content-Type: application/json` og `Accept: application/json`
- Vedhæfter `Authorization: Bearer {token}` på alle kald undtagen `/api/auth/login`
- Håndterer HTTP 401 → kalder `auth.logout()` + navigerer til `/login`

### Session-håndtering

Token og brugerinfo gemmes i `localStorage` under nøglerne `sbys_token` og `sbys_user`.  
`AuthService` eksponerer `isLoggedIn`, `currentUserId` og `currentUserName` som computed signals.

### Environment-konfiguration

| Miljø | API base URL |
|-------|-------------|
| Development (`ng serve`) | `https://localhost:7007` (`environment.ts`) |
| Simply / Prod (`ng build --configuration simply`) | `https://api.paynsync.dk` (`environment.simply.ts`) |
| Test (`ng build --configuration test`) | se `environment.test.ts` |

### Angular-routes

| Route | Komponent | Formål |
|-------|-----------|--------|
| `/home` | `HomeComponent` | Dashboard med action-cards og status-oversigt |
| `/orders` | `OrdersComponent` | Ordreoversigt (aktiv/afsluttet tabs) |
| `/orders/create` | `CreateOrderComponent` | 3-trins wizard: låst merchant + deltagere → detaljer → kontrol/opret. Trin 1 følger UC-03; trin 2-3 afventer UC-04/UC-05. |
| `/orders/:id` | `OrderDetailComponent` | Ordredetaljer + betalingsstatus + Host-handlinger |
| `/messages` | `MessagesComponent` | Beskedindbakke |
| `/pending-participants` | `PendingParticipantsComponent` | Host-view: deltagere der mangler at bestille |
| `/find-participants` | `FindParticipantsComponent` | Søg + tilføj venner |
| `/profile` | `ProfileComponent` | Kanonisk profil- og kontocenter med Konto, rollebeskyttet Vipps-test og Development-only Udvikler |
| `/login` | Redirect | Viderestiller til `/profile?mode=login` |
| `/register` | Redirect | Viderestiller til `/profile?mode=register` |

### Azure Static Web Apps routing

`staticwebapp.config.json` definerer SPA-fallback: alle routes rewriter til `/index.html`, undtagen statiske filer (`.css`, `.js`, `.ico`, billeder).

---

## Backend-teknologi (`Api.PayBySharePay`)

**Framework:** ASP.NET Core 9  
**Hosting (prod):** Simply.com Windows hosting — self-contained `win-x64` publish, IIS in-process via `web.config` + `AspNetCoreModuleV2`. Deploy via FTP til `api.paynsync.dk`.  
**Hosting (dev):** Kestrel, HTTP port 5071 / HTTPS port 7007
**API-dokumentation:** Swagger/OpenAPI tilgængeligt på `/` (root redirecter til `/swagger`)

### JWT-auth

`JwtTokenService` udsteder HS256-tokens med claims:
- `sub` = `participantId` (integer som string)
- `name` = deltagerens navn
- `jti` = unikt token-id (GUID)

Token-levetid: konfigurerbar via `Jwt:ExpiresInMinutes`. Default i `appsettings.json`: **43200 min (30 dage)**. `AuthController.Login()` returnerer hardkodet `expiresAt = now + 480 min` i response-body — dette er **ikke** den faktiske token-levetid (se Open Questions #1).  
Validering: issuer, audience, levetid og signatur valideres.

På host-only ordrehandlinger (`/approve`, `/cancel`, `/complete` og legacy `/pay`) udleder `OrdersController` det aktuelle participant-ID fra det validerede `NameIdentifier`/`sub`-claim. Et eventuelt `requestingParticipantId` i body bevares kun for bagudkompatibilitet og bruges ikke til autorisation. Manglende eller ugyldigt ID-claim giver 401.

### Middleware-pipeline (rækkefølge)

```
Swagger → ExceptionHandlingMiddleware → CORS → (HTTPS redirect i prod) → Authentication → Authorization → Controllers
```

### ExceptionHandlingMiddleware

| Exception-type | HTTP status |
|----------------|-------------|
| `ArgumentException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| `InvalidOperationException` | 409 Conflict |
| `UnauthorizedAccessException` | 403 Forbidden |
| Alle andre | 500 Internal Server Error |

> `UnauthorizedAccessException` mappes til et generisk 403-svar. Den interne exception-besked logges, men eksponeres ikke til klienten.

### Controllers

| Controller | Route | Auth | Formål |
|-----------|-------|------|--------|
| `AuthController` | `/api/auth` | Anonymous | Login, register (person + merchant) |
| `OrdersController` | `/api/orders` | `[Authorize]` på klassen | CRUD + reserve/approve/cancel/pay |
| `PaymentsController` | `/api/payments` | Ingen klasse-attr. | Register betaling, webhooks |
| `MerchantOrdersController` | `/api/merchant-orders` | `[Authorize]` på klassen, `[AllowAnonymous]` kun på `POST InitOrder` | Merchant draft-indsendelse (anonym) + `GET by-order/{id}` (kræver JWT) |
| `ParticipantsController` | `/api/participants` | Ingen | Søg, opret, opdatér profil, Vipps test-user mapping *(NYT)* |
| `FriendsController` | `/api/friends` | Ingen | Venneliste-håndtering |
| `DirectoryController` | `/api/directory` | Ingen | Tværgående søgning (person + merchant) |
| `MessagesController` | `/api/messages` | Ingen | Ordrebeskeder + ulæst-count |
| `VippsCallbackController` | `/api/payments/vipps` | `[AllowAnonymous]` | Vipps MobilePay webhook-modtagelse |
| `DevController` | `/api/dev` | Kun registreret i `Development` | Reset, seed merchant-URL'er, simuleret autorisation og callback-inspektion |

`DevelopmentOnlyControllerFeatureProvider` fjerner hele `DevController` fra MVC controller discovery i alle andre environments. Dermed registreres ingen `/api/dev/*`-routes, og ApiExplorer/Swagger eksponerer dem ikke i Simply, Production, Local eller ukendte miljøer.

**Nye endpoints i eksisterende controllere** *(NYT)*:

| Controller | Endpoint | Beskrivelse |
|-----------|----------|-------------|
| `AuthController` | `POST /api/auth/google-login` | Google ID-token validering → returnerer JWT |
| `ParticipantsController` | `GET /api/participants/vipps-test-users` | Hent alle Vipps sandbox-testpersoner med mapping-status |
| `ParticipantsController` | `PATCH /api/participants/{id}/vipps-test-user` | Sæt Vipps sandbox test-user mapping for en deltager |

### Hosted Services

- `MerchantDemoHostedService` — starter i `Development` **og** `Local` *(ÆNDRET)*. Kører `npx http-server` i `Frontend.MerchantDemo`-mappen på port 8081.

### CORS

Basisliste hardcodet i `Program.cs` (dev-origins + gamle Azure-origins der stadig er i koden):
- `http/https://localhost:4200` og `:4201` (Angular dev)
- `http/https://localhost:8081` (Merchant Demo dev)
- `https://purple-coast-0d01c1003.7.azurestaticapps.net` *(legacy Azure — stadig i kode)*
- `https://brave-flower-0026a7503.7.azurestaticapps.net` *(legacy Azure — stadig i kode)*

Produktions-origins injiceres fra `AppSettings:CorsOrigins` i `appsettings.Simply.json`:
- `https://mobil.paynsync.dk` (Angular prod)
- `https://paynsync.dk` og `https://www.paynsync.dk` (Landing page)
- `https://merchant.paynsync.dk` (Merchant Demo prod)

---

## Service-lag (`Service.PayBySharePay`)

Alle services registreres som `Scoped` via `AddServiceLayer()`.

### Repository-mønster (tilføjelse)

Nyt repository *(NYT)*: `IParticipantExternalLoginRepository` + `ParticipantExternalLoginRepository` — `GetByProviderAsync(provider, providerUserId)`, `AddAsync`, `SaveChangesAsync`.

### Nøgle-services

**`OrderService`**  
Opretter ordrer, tildeler deltagere, genererer `JoinToken` + `ParticipantToken` pr. `OrderParticipant`, sender merchant-links via `Message`-records.  
`OrderSubmitted` betyder kun, at deltagerens ordrelinjer er gemt hos PayNSync. Det må **ikke** alene sætte ordren til `ReadyToPay`.  
`ReadyToPay` må først sættes, når alle ikke-merchant deltagere har en `ParticipantPayment` med status `Reserved`, dvs. alle har swipet/godkendt deres MobilePay/Vipps-reservation.

**`GroupPaymentOrchestrationService`**  
Central betalingsorkestrering. Alle `IPaymentProvider`-kald sker herfra. Håndterer idempotens, ordre-statusmaskine og fejlhåndtering pr. capture.

**`ParticipantPaymentStateService`**  
Ejer alle `ParticipantPayment`-statusskift. Enforcer tilladte overgange via en statisk transition-tabel og skriver `PaymentEventLog` for hvert skift.

Tilladte overgange:
```
Created            → ReservationStarted | Cancelled
ReservationStarted → Reserved | ReservationFailed | Cancelled
Reserved           → CapturePending | Cancelled
CapturePending     → Captured | CaptureFailed
CaptureFailed      → CapturePending  (retry)
Captured           → Refunded
ReservationFailed, Cancelled, Expired, Refunded → (terminal)
```

**`MerchantOrderService`**  
Modtager merchant draft (anonym). Validerer `ParticipantToken`, opretter `MerchantOrderDraft` + linjer og sætter `OrderParticipant.Status = "OrderSubmitted"`. Derefter starter PayNSync reservationsflowet for deltageren via `ReserveParticipantPaymentAsync()` / `IPaymentProvider.ReserveAsync()`.

Vigtigt: `MerchantOrderService` må ikke frigive ordren til merchant, og `OrderSubmitted` må ikke gøre gruppeordren `ReadyToPay`. Det sker først, når alle deltageres betalinger er `Reserved`.

**`ExternalAuthService`** *(NY SERVICE)*  
Implementerer `IExternalAuthService`. Validerer Google ID-tokens via `GoogleJsonWebSignature.ValidateAsync()` (pakken `Google.Apis.Auth`). Finder eller opretter `Participant` og `ParticipantExternalLogin`. Kaster `ExternalLoginEmailConflictException` hvis en e-mail allerede er registreret med adgangskode — ingen automatisk sammenslåning af konto.

Konfigurationsnøgle: `Google:ClientId` — skal sættes i `appsettings.json` / `appsettings.Simply.json`.

**`PaymentService`** (legacy)  
Opretter `Payment`-record og sender host-notifikation. Bruges af det gamle manuelle betalingsflow.

**`MerchantCallbackService`** (implementeret i Api-laget)  
Efter full capture mapper `SquareInspiredMerchantOrderAdapter` den permanente final group order til merchant-format og sender HTTP POST til merchantens separate `MerchantOrderUrl`. `GroupOrderUrl` forbliver kundens menu-/bestillings-URL. Merchantens svar og eksterne ordrenummer gemmes på `MerchantOrder`. Leveringsfejl ruller ikke betalingerne tilbage.

---

## Databaselag (`DataStorage.PayBySharePay`)

**ORM:** Entity Framework Core + SQL Server  
**Connection string:** `Server=...\SQLEXPRESS;Database=PayBySharePay;Trusted_Connection=True` (dev)

### Entiteter og relationer

```
Participant (Person | Merchant)
  ├── FriendRelation (selv-ref. many-to-many via InitiatorId / ReceiverId)
  ├── OrderParticipant ──► Order  (unik ParticipantToken-index)
  ├── Payment ──► Order           (legacy)
  ├── ParticipantPayment ──► Order  (provider-backed, RowVersion for concurrency)
  ├── ParticipantExternalLogin[]  (Provider + ProviderUserId — fx Google)  *(NYT)*
  └── VippsTestUser? ──► Participant  (self-ref nullable FK — sandbox-mapping)  *(NYT)*

ParticipantExternalLogin  *(NY ENTITET)*
  ├── ParticipantId ──► Participant  (Cascade delete)
  ├── Provider  (fx "Google" | "Apple")
  ├── ProviderUserId  (subject claim fra udbyderen)
  ├── Email?  (e-mail returneret af udbyderen)
  └── CreatedAtUtc

Order
  ├── CreatedBy ──► Participant   (Restrict delete)
  ├── MerchantParticipant? ──► Participant  (nullable, Restrict delete)
  ├── OrderParticipant[]  (Cascade delete)
  ├── Payment[]           (Cascade delete)
  ├── Message[]
  └── MerchantOrderDraft[]
		└── MerchantOrderLine[]

PaymentEventLog  (immutable audit trail — ingen navigationsegenskaber fra Order)
```

**Nye Participant-felter (Merchant-specific)** *(NYT)*:
- `VippsMerchantSerialNumber` — per-merchant MSN; null = brug global fra appsettings
- `VippsClientId`, `VippsClientSecret`, `VippsSubscriptionKey` — per-merchant Vipps API-credentials; null = brug global fra appsettings
- `VippsTestUserId` — self-ref FK til sandbox-testperson (kun dev/sandbox-brug)

**Ny MerchantOrderDraft-felt** *(NYT)*:
- `RawMerchantPayloadJson` — valgfri nullable string; gemmer merchantens originale JSON til audit/debugging

### Repository-mønster

Hvert entity har sit eget `IXxxRepository`-interface og en `XxxRepository`-implementering. Intet generisk basisrepository. Alle registreres som `Scoped`.

### Migrationer

| Migration | Dato | Ændring |
|-----------|------|---------|
| `InitialCreate` | 2026-05-05 | Alle basistabeller |
| `AddOrderCreatedBy` | 2026-05-06 | `Order.CreatedByParticipantId` FK |
| `AddMerchantOrderDraft` | 2026-05-06 | `MerchantOrderDraft` + `MerchantOrderLine` |
| `AddMerchantToOrder` | 2026-05-08 | `Order.MerchantParticipantId` FK |
| `AddParticipantToOrderLine` | 2026-05-15 | `MerchantOrderLine.ParticipantId` |
| `AddParticipantTokenAndDraftParticipant` | 2026-05-16 | `OrderParticipant.ParticipantToken` (unikt index), `MerchantOrderDraft.ParticipantId` |
| `AddMessageIsRead` | 2026-05-16 | `Message.IsRead` flag |
| `AddParticipantPasswordHash` | 2026-05-24 | `Participant.PasswordHash` |
| `AddParticipantPaymentAndEventLog` | 2026-05-25 | `ParticipantPayment` + `PaymentEventLog` tabeller |
| `AddRawMerchantPayloadJson` | 2026-06-28 | `MerchantOrderDraft.RawMerchantPayloadJson` (nullable) — *(NYT)* |
| `AddVippsMerchantSerialNumber` | 2026-06-28 | `Participant.VippsMerchantSerialNumber` — *(NYT)* |
| `AddMerchantVippsCredentials` | 2026-06-28 | `Participant.VippsClientId/ClientSecret/SubscriptionKey` — *(NYT)* |
| `AddParticipantExternalLogin` | 2026-07-03 | Ny tabel `ParticipantExternalLogins` til OAuth-logins (Google) — *(NYT)* |
| `AddVippsTestUserId` | 2026-07-03 | `Participant.VippsTestUserId` (self-ref FK til sandbox test-user) — *(NYT)* |
| `AddMerchantAdapterDelivery` | 2026-09-05 | `Participant.MerchantOrderUrl`, ekstern ordre-reference/svar og strukturerede modifiers på final merchant order |

---

## Infrastrukturlaget – betalinger (`Infrastructure.Payments.PayBySharePay`)

### IPaymentProvider

Abstraherer alle udbyder-kald. Tre operationer:
- `ReserveAsync` — reserver/autoriser betalingen hos udbyderen
- `CaptureAsync` — gennemfør (træk penge)
- `CancelAsync` — annuller reservationen
- `GetStatusAsync` — forespørg status

### FakePaymentProvider

Bruges i dev og tests (`Payments:Provider = "Fake"`). Returnerer success synkront (ingen HTTP-kald). Adfærd styres via `FakePaymentProviderOptions` i `appsettings.json`:

| Flag | Effekt |
|------|--------|
| `SimulateReservationFailed` | Reserve returnerer fejl |
| `SimulateReservationExpired` | Reserve returnerer expired |
| `SimulateCaptureFailed` | Capture returnerer fejl |
| `SimulateCancelFailed` | Cancel returnerer fejl |
| `SimulateReserveException` | Reserve kaster exception |
| `SimulateCaptureException` | Capture kaster exception |

### MobilePaySandboxPaymentProvider (Vipps)

Bruges med `Payments:Provider = "MobilePay"`. Kommunikerer med Vipps MobilePay ePayment API.

**Token-håndtering (`VippsMobilePayTokenService`):**  
Cacher OAuth2 access token i hukommelsen. Fornyer det 5 minutter før udløb. Trådsikker via `SemaphoreSlim`.  
Token-endpoint: `{BaseUrl}/accesstoken/get` (headers: `client_id`, `client_secret`, `Ocp-Apim-Subscription-Key`, `Merchant-Serial-Number`).

**Vipps Reserve-kald:**  
`POST {BaseUrl}/epayment/v1/payments`  
Body: `amount`, `paymentMethod: { type: "WALLET" }`, `reference`, `userFlow: "WEB_REDIRECT"`, `returnUrl`, `webhookUrl`, `paymentDescription`.  
Webhook URL bør pege på PayNSyncs offentlige webhook endpoint, fx `{CallbackBaseUrl}/api/payments/vipps/callbacks`. Payment lookup bør ske via Vipps `reference` / `ProviderPaymentId` i payloaden frem for telefonnummer.

---

## Eksterne integrationer


## Merchant Demo og MobilePay/Vipps redirect-flow

Merchant Demo er en statisk HTML/JS-demo og skal ikke indeholde Vipps/MobilePay credentials eller kalde Vipps API direkte.

Ansvarsdeling:

```text
Frontend.MerchantDemo
  - viser menu/kurv
  - læser orderId + participantToken fra link
  - sender draft-ordre til PayNSync API
  - modtager redirectUrl
  - redirecter brugeren til Vipps/MobilePay approval flow

Api/Service/Infrastructure
  - gemmer merchant draft
  - opretter ParticipantPayment
  - kalder Vipps/MobilePay ePayment API
  - returnerer redirectUrl
  - modtager webhook
  - opdaterer status til Reserved
  - capturer senere via host approve
```

Merchant Demo må ikke:

```text
- hente access token
- kalde /accesstoken/get
- kalde /epayment/v1/payments direkte
- gemme client_id/client_secret/subscription key
- håndtere webhook-signaturer
- capture betalinger
```

### Merchant Demo reservation-start flow

```
Deltager klikker "Bekræft ordre og reservér med MobilePay"
  → Merchant Demo POST /api/merchant-orders
  → MerchantOrderService.InitOrderAsync()
  → MerchantOrderDraft + MerchantOrderLine gemmes
  → ParticipantPayment oprettes/sættes ReservationStarted
  → MobilePaySandboxPaymentProvider.ReserveAsync()
  → Vipps/MobilePay create payment returnerer redirectUrl
  ← PayNSync API returnerer redirectUrl til Merchant Demo
  → Merchant Demo: window.location.href = redirectUrl
  → Deltager swiper/godkender i MobilePay/Vipps app/test flow
  → Vipps webhook til PayNSync
  → ParticipantPayment.Status = Reserved
```

Response fra `POST /api/merchant-orders` bør derfor indeholde betalings-/redirect-information:

```json
{
  "status": "ReservationStarted",
  "orderId": 123,
  "participantPaymentId": 456,
  "providerPaymentId": "PNS-123-7-456",
  "redirectUrl": "https://...",
  "message": "Ordren er gemt. Godkend reservationen i MobilePay."
}
```

### Testtelefonnummer

I sandbox kan Merchant Demo midlertidigt sende et testtelefonnummer til PayNSync, eller PayNSync kan gemme testtelefonnummer på deltageren.

Telefonnummeret må kun bruges ved oprettelse/start af payment i testflowet. Capture-loopet må aldrig bruge telefonnummer/MobilePay-id. Capture sker altid via `ParticipantPayment.ProviderPaymentId` / Vipps reference.


### Vipps MobilePay ePayment API

| Detalje | Værdi |
|---------|-------|
| Sandbox base URL | `https://apitest.vipps.no` |
| Prod base URL | `https://api.vipps.no` (konfigurabel) |
| Auth | OAuth2 client credentials (client_id + client_secret + subscription key) |
| Betaling endpoint | `/epayment/v1/payments` |
| Token endpoint | `/accesstoken/get` |
| Webhook ind mod os | `POST /api/payments/vipps/callbacks` eller legacy `POST /api/payments/vipps/callbacks/{reference}`. Foretrukket lookup: Vipps `reference` / `ProviderPaymentId` i payload. |
| Signatur-validering | ❌ Ikke implementeret |

### PayNSync Merchant Integration Contract v1

PayNSync har en privacy-safe, standardiseret final order (`PayNSyncFinalGroupOrderDto`) som intern kontrakt mellem Group Payment og downstream order destinations.

UC-21 implementerer den første konkrete adapter:

```text
PayNSyncFinalGroupOrderDto
  → SquareInspiredMerchantOrderAdapter
  → SquareInspiredMerchantOrderRequest
  → POST Merchant.MerchantOrderUrl
  → merchant response
  → MerchantOrder.ExternalOrderNumber + ExternalResponseJson
```

URL-ansvar er adskilt:

- `Merchant.GroupOrderUrl` = kundevendt menu-/bestillingsside, som deltagerens personlige GroupOrder-link peger på.
- `Merchant.MerchantOrderUrl` = backend-endpoint til den færdige merchant-ordre efter successful capture.

Adapteren mapper produkter, priser, strukturerede tilvalg, host-contact snapshot og levering. Beløb sendes som minor units. Participant-identitet og betalingsprovider-referencer må ikke sendes til merchantens ordresystem.

Den simulerede endpoint `POST /api/dev/simulated-merchant/orders` findes kun i Development og returnerer deterministisk samme eksterne ordre for samme idempotency key. Det er et test-/proof-of-integration endpoint, ikke en produktionsintegration til Square.

Eksisterende `MerchantOrder.SourceOrderId` er unik, og et allerede gemt `ExternalOrderNumber` overskrives ikke. Samme PayNSync-ordre må derfor ikke skabe flere eksterne merchant-ordrer via normal retry.

Senere adapters (fx OrderYOYO/POS) skal implementeres ved denne boundary uden ændringer i Group Payment-kernen.

---

## Deployment

### API (Simply.com — `api.paynsync.dk`)

- Self-contained publish (`win-x64`) — ingen ekstern .NET-runtime kræves på serveren
- `web.config` konfigurerer IIS in-process hosting via `AspNetCoreModuleV2`
- `ASPNETCORE_ENVIRONMENT = Simply` sættes via `web.config` (bruger `appsettings.Simply.json`)
- HTTPS redirect aktiveret i prod (deaktiveret i dev for at undgå problemer med http://localhost:8081)
- FTP-server: `nt31.unoeuro.com` → `/api.paynsync.dk/`

### Frontend Angular (Simply.com — `mobil.paynsync.dk`)

- `ng build --configuration simply` producerer statiske filer (`environment.simply.ts` bruges → `https://api.paynsync.dk`)
- Deploy via FTP til `mobil.paynsync.dk`

### Landing Page (Simply.com — `paynsync.dk`)

- `src/Landing.PayBySharePay/` deployet via FTP til `/public_html/`

### Merchant Demo (Simply.com — `merchant.paynsync.dk`)

- Statisk HTML/CSS/JavaScript-app deployet via FTP til `merchant.paynsync.dk`.
- `order-model.js` ejer det faste demo-katalog, prisberegning og draft-mapping; `app.js` ejer DOM, kurv og API-kald.
- Produktets stabile ID sendes som `lineId`. Valgte tilvalg gemmes læsbart i linjenavnet og struktureret i `RawMerchantPayloadJson`.
- Auto-detekterer API base URL fra `window.location.hostname`:
  - `localhost` / `127.0.0.1` → `https://localhost:7007`
  - Andre → `https://api.paynsync.dk`

### CI/CD *(OPDATERET)*

To GitHub Actions workflows:

| Workflow | Trigger | Formål |
|----------|---------|--------|
| `.github/workflows/build.yml` | Auto ved push til `main` + PR mod `main` | Build + test (.NET + Angular) |
| `.github/workflows/deploy-simply.yml` | Manuel (`workflow_dispatch`) | Byg + deploy til Simply.com |

**GitHub Secrets der kræves for deploy:** `SIMPLY_FTP_USERNAME`, `SIMPLY_FTP_PASSWORD`, `SIMPLY_DB_CONNECTION_STRING`, `SIMPLY_JWT_KEY`, `SIMPLY_VIPPS_CLIENT_ID`, `SIMPLY_VIPPS_CLIENT_SECRET`, `SIMPLY_VIPPS_SUB_KEY`, `SIMPLY_VIPPS_MSN` *(de 4 Vipps-secrets er NYE)*

| Trin | Beskrivelse |
|------|-------------|
| Publish API | `dotnet publish --self-contained --runtime win-x64` |
| Injicer secrets | `jq` patcher `appsettings.Simply.json` med DB-connection + JWT-key fra GitHub Secrets |
| Byg Angular | `ng build --configuration simply` |
| Tag API offline | Upload `app_offline.htm` via FTP (frigør fillåse under deploy) |
| Deploy API | FTP → `nt31.unoeuro.com/api.paynsync.dk/` |
| Tag API online | Slet `app_offline.htm` |
| Deploy Landing | FTP → `nt31.unoeuro.com/public_html/` |
| Deploy Frontend | FTP → `nt31.unoeuro.com/mobil.paynsync.dk/` |
| Deploy MerchantDemo | FTP → `nt31.unoeuro.com/merchant.paynsync.dk/` |

**GitHub Secrets der kræves:** `SIMPLY_FTP_USERNAME`, `SIMPLY_FTP_PASSWORD`, `SIMPLY_DB_CONNECTION_STRING`, `SIMPLY_JWT_KEY`

---

## Konfiguration

```jsonc
// appsettings.json (nøglestier)
"AppSettings:ApiBaseUrl"               // https://localhost:7007  (ÆNDRET fra http://localhost:5071)
"AppSettings:MerchantDemoUrl"          // http://localhost:8081
"AppSettings:FrontendUrl"              // http://localhost:4200
"Payments:Provider"                    // "Fake" | "MobilePay"
"Payments:Fake:SimulateReservationFailed"   // bool
"Payments:Fake:SimulateReservationExpired"  // bool
"Payments:Fake:SimulateCaptureFailed"       // bool
"Payments:Fake:SimulateCancelFailed"        // bool
"Payments:Fake:SimulateReserveException"    // bool
"Payments:Fake:SimulateCaptureException"    // bool
"Payments:VippsMobilePay:BaseUrl"           // https://apitest.vipps.no
"Payments:VippsMobilePay:ClientId"
"Payments:VippsMobilePay:ClientSecret"
"Payments:VippsMobilePay:SubscriptionKey"
"Payments:VippsMobilePay:MerchantSerialNumber"
"Payments:VippsMobilePay:CallbackBaseUrl"   // din ngrok URL i dev, Azure URL i prod
"ConnectionStrings:PayBySharePayDb"
"Jwt:Key"                              // min. 32 tegn
"Jwt:Issuer"                           // sbys-api
"Jwt:Audience"                         // sbys-frontend
"Jwt:ExpiresInMinutes"                 // 43200 (i appsettings; 480 bruges i kode)
"Google:ClientId"                      // Google OAuth2 client ID — nødvendig for /api/auth/google-login  *(NYT)*
"Payments:VippsMobilePay:ClientId"     // Global Vipps client ID (override per merchant muligt)  *(NYT i simply)*
"Payments:VippsMobilePay:ClientSecret" // Global Vipps client secret  *(NYT i simply)*
"Payments:VippsMobilePay:SubscriptionKey" // Global Vipps Ocp-Apim-Subscription-Key  *(NYT i simply)*
"Payments:VippsMobilePay:MerchantSerialNumber" // Global Vipps MSN  *(NYT i simply)*
```

---

## Data flows gennem systemet

## Besluttet betalingsarkitektur for PayNSync v1

PayNSync v1 følger model D / hybridmodellen:

```text
Merchant = menu, kurv, priser og ordrelinjer
PayNSync = gruppeordre, participant tokens, reservation, status, capture og endelig accept
Vipps/MobilePay = deltagerens betalingsgodkendelse
```

Merchant opretter **ikke** MobilePay/Vipps-betalingen i v1. Merchant sender en ordre-draft til PayNSync via `POST /api/merchant-orders`. PayNSync gemmer ordrelinjerne, opretter en `ParticipantPayment`, kalder `IPaymentProvider.ReserveAsync()` og sender deltageren videre til MobilePay/Vipps for at godkende reservationen.

En MobilePay/Vipps-godkendelse fra én deltager må kun opdatere deltagerens betaling til `Reserved`. Den må aldrig sende/frigive den samlede ordre til merchant.

Merchant-callback sker først efter dette samlede flow:

```text
Alle deltagere har Reserved
  → Order.Status = ReadyToPay
  → Host klikker Godkend samlet ordre
  → PayNSync capturer alle reservationer
  → Alle betalinger er Captured
  → Order.Status = Paid
  → MerchantCallbackService sender Paid callback til merchant
```

### Vigtig capture-regel

Host-godkendelse starter **ikke** nye betalinger og bruger **ikke** deltagerens telefonnummer/MobilePay-id.

Ved host-godkendelse looper PayNSync gennem eksisterende `ParticipantPayment`-records med status `Reserved` og kalder capture på hver betaling via dens `ProviderPaymentId` / Vipps reference.

```text
1 deltager = 1 MerchantOrderDraft
1 deltager = 1 ParticipantPayment
1 deltager = 1 Vipps/MobilePay reservation/reference
1 gruppeordre = flere individuelle captures + 1 samlet merchant-callback
```

Eksempel:

```text
Michael 168 kr. → ProviderPaymentId PNS-123-7 → capture 168 kr.
Anna    224 kr. → ProviderPaymentId PNS-123-8 → capture 224 kr.
Peter    89 kr. → ProviderPaymentId PNS-123-9 → capture  89 kr.
```

Det er altså ikke ét samlet Vipps/MobilePay-beløb. PayNSync sender dog én samlet `Paid` callback til merchant, når alle individuelle captures er gennemført.

---

### Flow 1: Bruger opretter gruppeordre

```
Angular (CreateOrderComponent)
  → POST /api/orders  [JWT]
  → OrdersController.CreateOrder()
  → OrderService.CreateOrderAsync()
	→ ParticipantRepository.GetByIdAsync()    [SQL]
	→ OrderRepository.AddAsync()              [SQL]
	→ OrderRepository.SaveChangesAsync()      [SQL]
	→ Opretter Message-records pr. deltager   [SQL]
  ← OrderDto (id, status, ...)
Angular navigerer til /orders/{id}
```

### Flow 2: Deltager bestiller via merchant og reserverer betaling

```
Merchant Demo / rigtig merchant
  → Deltager klikker "Bekræft min ordre" / "Gem ordre og reservér betaling"
  → POST /api/merchant-orders  [Anonymous]
  → MerchantOrdersController.InitOrder()
  → MerchantOrderService.InitOrderAsync()
	→ OrderRepository.GetByIdWithDetailsAsync()       [SQL]
	→ ParticipantRepository.GetByIdAsync()            [SQL]
	→ DbContext.OrderParticipants (ParticipantToken)  [SQL]
	→ MerchantOrderDraftRepository.AddAsync()         [SQL]
	→ OrderParticipant.Status = "OrderSubmitted"     [SQL]
	→ ReserveParticipantPaymentAsync()
	  → Opret ParticipantPayment = Created            [SQL]
	  → Status → ReservationStarted                   [SQL + PaymentEventLog]
	  → IPaymentProvider.ReserveAsync()
	    [Vipps: POST /epayment/v1/payments]
	  → Returnér redirectUrl / MobilePay-flow til deltager
  ← MerchantOrderDraftDto + betalings-/redirect-information

Deltager swiper/godkender i MobilePay/Vipps
  → Vipps webhook mod PayNSync
  → ParticipantPayment.Status = Reserved
  → PayNSync tjekker om alle ikke-merchant deltagere har ParticipantPayment.Status = Reserved
  → Hvis alle Reserved: Order.Status = "ReadyToPay" + besked til host
  → Hvis blot alle har OrderSubmitted, men ikke alle har Reserved: ordren forbliver Collecting/afventende

Vigtigt: Merchant får ikke endelig ordre endnu. Merchant må kun vise, at deltagerens ordre er gemt og afventer resten af gruppen.
```

### Flow 3: Host godkender samlet ordre og PayNSync capturer betalinger

```
Angular (OrderDetailComponent)
  → POST /api/orders/{id}/approve  [JWT]
  → OrdersController.ApproveOrder()
    → Udled currentParticipantId fra JWT NameIdentifier/sub; ugyldigt claim → 401
  → GroupPaymentOrchestrationService.ApproveAndCaptureAllAsync(currentParticipantId)
	→ OrderRepository.GetByIdWithDetailsAsync()           [SQL]
	→ ParticipantPaymentRepository.GetByOrderIdAsync()    [SQL]
	→ ParticipantPaymentStateService.SetCapturePendingAsync()
	  → PaymentEventLogRepository.AddAsync()              [SQL]
	→ Order.Status = "Capturing"                          [SQL]
	FOR EACH reserved ParticipantPayment:
	  → Brug payment.ProviderPaymentId / Vipps reference
	  → IPaymentProvider.CaptureAsync(payment.ProviderPaymentId, payment.AmountMinorUnits)
		[Fake: synkron success]
		[Vipps: POST https://apitest.vipps.no/epayment/v1/payments/{reference}/capture]
	  → ParticipantPaymentStateService.SetCapturedAsync() [SQL]
	→ Order.Status = "Paid"                               [SQL]
	→ MerchantCallbackService.SendPaidCallbackAsync()
	  → POST {merchant.GroupOrderUrl} med samlet Paid/Accepted payload [HTTP udgående]
  ← ApproveAndCaptureResult

Først her må merchant lave/frigive den samlede ordre.
```

### Flow 4: Vipps webhook

```
Vipps MobilePay
  → POST /api/payments/vipps/callbacks  [Anonymous]
     eller legacy POST /api/payments/vipps/callbacks/{reference}
  → VippsCallbackController.VippsCallback()
	→ ParticipantPaymentRepository.GetByProviderPaymentIdAsync()  [SQL]
	Mapper Vipps event-navn:
	  AUTHORIZED / RESERVE → ParticipantPaymentStateService.SetReservedAsync()                     [SQL]
	  CAPTURED             → Logger og ignorerer (ingen state-ændring — capture sker via /approve)
	  CANCELLED / ABORTED  → SetCancelledAsync()                                                    [SQL]
	  EXPIRED / TERMINATED → SetReservationFailedAsync() (sætter ReservationFailed, ikke Expired)   [SQL]
	  Andet                → Logger og ignorerer
  ← 200 OK (altid — Vipps skal ikke retry)
```

---

## Open Questions

1. **`Jwt:ExpiresInMinutes` i appsettings vs. kode** — `appsettings.json` indeholder `43200` (30 dage), men `JwtTokenService` bruger værdien direkte og `AuthController` bruger `AddMinutes(480)` hardcodet. Hvad er den faktiske udløbstid?
2. **CI/CD** — `deploy-simply.yml` er konfigureret til manuel deploy via `workflow_dispatch`. Ingen automatisk deploy ved push til `main`.
3. **Angular environment-filer** — Tre filer: `environment.ts` (dev → `localhost:7007`), `environment.simply.ts` (prod → `https://api.paynsync.dk`), `environment.test.ts` (test). Konfigureres i `angular.json` via `fileReplacements`.
4. **ReadyToPay-implementering** — Hvis eksisterende kode stadig sætter `ReadyToPay` på baggrund af `OrderSubmitted`, skal den ændres. Fremadrettet må `ReadyToPay` kun sættes, når alle relevante `ParticipantPayment`-records er `Reserved`.
