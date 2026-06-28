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
| `/orders/create` | `CreateOrderComponent` | 4-trins wizard: titel → merchant → deltagere → opret |
| `/orders/:id` | `OrderDetailComponent` | Ordredetaljer + betalingsstatus + Host-handlinger |
| `/messages` | `MessagesComponent` | Beskedindbakke |
| `/pending-participants` | `PendingParticipantsComponent` | Host-view: deltagere der mangler at bestille |
| `/find-participants` | `FindParticipantsComponent` | Søg + tilføj venner |
| `/profile` | `ProfileComponent` | Brugerprofil |
| `/login` | `LoginComponent` | Login |
| `/register` | `RegisterComponent` | Registrering |

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

Token-levetid: konfigurerbar via `Jwt:ExpiresInMinutes`. Default i `appsettings.json`: **43200 min (30 dage)**. `AuthController.Login()` returnerer hardkodet `expiresAt = now + 480 min` i response-body — dette er **ikke** den faktiske token-levetid (se Open Questions #2).  
Validering: issuer, audience, levetid og signatur valideres.

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
| `UnauthorizedAccessException` | 500 Internal Server Error (fanges af generisk handler — se note) |
| Alle andre | 500 Internal Server Error |

> Note: `UnauthorizedAccessException` fanges **ikke** eksplicit — den fanges af den generiske `Exception`-handler og returnerer HTTP 500 med JSON-body `{ "error": "Der opstod en uventet fejl.", "detail": "<ex.Message>", "type": "UnauthorizedAccessException" }`. Host-tjek i services kaster denne exception (f.eks. `"Kun ordrevært (host) kan godkende og capture."`), og `detail` eksponerer dermed interne beskeder i 500-svaret.

### Controllers

| Controller | Route | Auth | Formål |
|-----------|-------|------|--------|
| `AuthController` | `/api/auth` | Anonymous | Login, register (person + merchant) |
| `OrdersController` | `/api/orders` | `[Authorize]` på klassen | CRUD + reserve/approve/cancel/pay |
| `PaymentsController` | `/api/payments` | Ingen klasse-attr. | Register betaling, webhooks |
| `MerchantOrdersController` | `/api/merchant-orders` | `[Authorize]` på klassen, `[AllowAnonymous]` kun på `POST InitOrder` | Merchant draft-indsendelse (anonym) + `GET by-order/{id}` (kræver JWT) |
| `ParticipantsController` | `/api/participants` | Ingen | Søg, opret, opdatér profil |
| `FriendsController` | `/api/friends` | Ingen | Venneliste-håndtering |
| `DirectoryController` | `/api/directory` | Ingen | Tværgående søgning (person + merchant) |
| `MessagesController` | `/api/messages` | Ingen | Ordrebeskeder + ulæst-count |
| `VippsCallbackController` | `/api/payments/vipps` | `[AllowAnonymous]` | Vipps MobilePay webhook-modtagelse |
| `DevController` | `/api/dev` | Ingen | `DELETE /reset` + `POST /seed-merchant-urls` (test only) |

### Hosted Services

- `MerchantDemoHostedService` — starter kun i `Development`. Kører `npx http-server` i `Frontend.MerchantDemo`-mappen på port 8081.

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

### Nøgle-services

**`OrderService`**  
Opretter ordrer, tildeler deltagere, genererer `JoinToken` + `ParticipantToken` pr. `OrderParticipant`, sender merchant-links via `Message`-records.  
Ejer `CheckAndSetReadyToPayAsync`: sætter ordre til `ReadyToPay` når alle ikke-merchant deltagere har status `OrderSubmitted`.

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
Modtager merchant draft (anonym). Validerer `ParticipantToken`, opretter `MerchantOrderDraft` + linjer, sætter `OrderParticipant.Status = "OrderSubmitted"`, kalder `CheckAndSetReadyToPayAsync`.

**`PaymentService`** (legacy)  
Opretter `Payment`-record og sender host-notifikation. Bruges af det gamle manuelle betalingsflow.

**`MerchantCallbackService`** (implementeret i Api-laget)  
Sender HTTP POST til merchant's `GroupOrderUrl` når alle betalinger er captured. Fejl stopper ikke flowet.

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
  └── ParticipantPayment ──► Order  (provider-backed, RowVersion for concurrency)

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
Webhook URL konstrueres som: `{CallbackBaseUrl}/api/payments/vipps/callbacks/{ParticipantPaymentId}`.

---

## Eksterne integrationer

### Vipps MobilePay ePayment API

| Detalje | Værdi |
|---------|-------|
| Sandbox base URL | `https://apitest.vipps.no` |
| Prod base URL | `https://api.vipps.no` (konfigurabel) |
| Auth | OAuth2 client credentials (client_id + client_secret + subscription key) |
| Betaling endpoint | `/epayment/v1/payments` |
| Token endpoint | `/accesstoken/get` |
| Webhook ind mod os | `POST /api/payments/vipps/callbacks/{reference}` |
| Signatur-validering | ❌ Ikke implementeret |

### Merchant callback (udgående)

Efter alle betalinger er captured sender `MerchantCallbackService` en HTTP POST til `Merchant.GroupOrderUrl`:
```json
{
  "orderId": 1,
  "merchantId": "2",
  "status": "Paid",
  "participantOrders": [
	{ "participantId": 3, "status": "Paid", "providerTransactionId": "..." }
  ]
}
```
Fejl ignoreres (betalingerne er allerede gennemført).

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

- Enkelt `index.html` deployet som statisk site via FTP til `merchant.paynsync.dk`
- Auto-detekterer API base URL fra `window.location.hostname`:
  - `localhost` / `127.0.0.1` → `http://localhost:5071`
  - Andre → `https://api.paynsync.dk`

### CI/CD

GitHub Actions workflow: `.github/workflows/deploy-simply.yml` (manuel trigger — `workflow_dispatch`).

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
"AppSettings:ApiBaseUrl"               // http://localhost:5071
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
```

---

## Data flows gennem systemet

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

### Flow 2: Deltager bestiller via merchant

```
Merchant Demo (index.html)
  → POST /api/merchant-orders  [Anonymous]
  → MerchantOrdersController.InitOrder()
  → MerchantOrderService.InitOrderAsync()
	→ OrderRepository.GetByIdWithDetailsAsync()       [SQL]
	→ ParticipantRepository.GetByIdAsync()            [SQL]
	→ DbContext.OrderParticipants (ParticipantToken)  [SQL]
	→ MerchantOrderDraftRepository.AddAsync()         [SQL]
	→ OrderService.CheckAndSetReadyToPayAsync()
	  → Hvis alle OrderSubmitted: Order.Status = "ReadyToPay"  [SQL]
	  → Opretter notifikations-Message til host                 [SQL]
  ← MerchantOrderDraftDto
```

### Flow 3: Host godkender betaling

```
Angular (OrderDetailComponent)
  → POST /api/orders/{id}/approve  [JWT]
  → OrdersController.ApproveOrder()
  → GroupPaymentOrchestrationService.ApproveAndCaptureAllAsync()
	→ OrderRepository.GetByIdWithDetailsAsync()           [SQL]
	→ ParticipantPaymentRepository.GetByOrderIdAsync()    [SQL]
	→ ParticipantPaymentStateService.SetCapturePendingAsync()
	  → PaymentEventLogRepository.AddAsync()              [SQL]
	→ Order.Status = "Capturing"                          [SQL]
	FOR EACH reserved payment:
	  → IPaymentProvider.CaptureAsync()
		[Fake: synkron success]
		[Vipps: POST https://apitest.vipps.no/epayment/v1/payments/{id}/capture]
	  → ParticipantPaymentStateService.SetCapturedAsync() [SQL]
	→ Order.Status = "Paid"                               [SQL]
	→ MerchantCallbackService.SendPaidCallbackAsync()
	  → POST {merchant.GroupOrderUrl}  [HTTP udgående]
  ← ApproveAndCaptureResult
```

### Flow 4: Vipps webhook

```
Vipps MobilePay
  → POST /api/payments/vipps/callbacks/{participantPaymentId}  [Anonymous]
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

1. **`UnauthorizedAccessException` → 500** — Host-tjek i services kaster `UnauthorizedAccessException`, men middleware mapper det ikke til 403. Det resulterer i HTTP 500. Er dette bevidst?
2. **`Jwt:ExpiresInMinutes` i appsettings vs. kode** — `appsettings.json` indeholder `43200` (30 dage), men `JwtTokenService` bruger værdien direkte og `AuthController` bruger `AddMinutes(480)` hardcodet. Hvad er den faktiske udløbstid?
3. **CI/CD** — `deploy-simply.yml` er konfigureret til manuel deploy via `workflow_dispatch`. Ingen automatisk deploy ved push til `main`.
4. **Angular environment-filer** — Tre filer: `environment.ts` (dev → `localhost:7007`), `environment.simply.ts` (prod → `https://api.paynsync.dk`), `environment.test.ts` (test). Konfigureres i `angular.json` via `fileReplacements`.
5. **`DevController` i prod** — `DELETE /api/dev/reset` og `POST /api/dev/seed-merchant-urls` er deployet til produktion uden auth-beskyttelse.
