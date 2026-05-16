# Arkitektur

## Overordnet arkitektur

PayBySharePay er en klassisk **3-lags webapplikation** med separat frontend, API og datalager.

```
[Angular Frontend]  →  [ASP.NET Core API]  →  [Service Layer]  →  [EF Core / SQL Server]
```

## Projekter i solution

| Projekt | Type | Ansvar |
|---------|------|--------|
| `Api.PayBySharePay` | ASP.NET Core Web API | REST API, auth, controllers, middleware |
| `Service.PayBySharePay` | .NET Class Library | Forretningslogik, DTOs, interfaces |
| `DataStorage.PayBySharePay` | .NET Class Library | EF Core DbContext, entities, repositories, migrationer |
| `Frontend.PayBySharePay` | Angular 18 SPA | Brugergrænsefladen |
| `Tests.PayBySharePay` | xUnit | Testprojekt (næsten tomt) |
| `Tools.PayBySharePay` | .NET Console App | CLI-værktøj til seed, flush, dev-hjælpekommandoer |

## Lagdeling

### API-lag (`Api.PayBySharePay`)
- Controllers modtager HTTP-requests og mapper til service-DTOs
- `JwtTokenService` udsteder JWT-tokens
- `ExceptionHandlingMiddleware` fanger uhåndterede exceptions og returnerer JSON-fejl
- Swagger/OpenAPI aktiveret

### Service-lag (`Service.PayBySharePay`)
- Indeholder al forretningslogik
- Interfaces (`IOrderService`, `IPaymentService` osv.) bruges af API'et via DI
- Mapper mellem entities og DTOs

### Datalager (`DataStorage.PayBySharePay`)
- `PayBySharePayDbContext` er EF Core context med alle DbSets
- Repository pattern med interfaces og implementeringer
- `DataStorageServiceExtensions.AddDataStorage()` registrerer alt i DI

### Frontend (`Frontend.PayBySharePay`)
- Angular 18 standalone components
- Angular signals til state management
- Lazy-loaded routes
- HTTP-interceptor tilføjer JWT Bearer token på alle requests
- `environment.ts` styrer API-base-URL

## Centrale klasser og services

### Backend
| Klasse | Placering | Ansvar |
|--------|-----------|--------|
| `PayBySharePayDbContext` | DataStorage/Context | EF Core context |
| `OrderService` | Service/Services | CRUD på ordrer, deltagere |
| `PaymentService` | Service/Services | Registrering af betalinger |
| `MerchantOrderService` | Service/Services | Merchant draft-system |
| `ParticipantService` | Service/Services | Opret/søg deltagere |
| `DirectoryService` | Service/Services | Telefonbog/brugersøgning |
| `MessageService` | Service/Services | Beskeder på ordrer |
| `JwtTokenService` | Api/Auth | Generér JWT tokens |

### Frontend
| Service | Ansvar |
|---------|--------|
| `AuthService` | Login, token, localStorage |
| `OrderService` | API-kald til ordrer |
| `PaymentService` | API-kald til betalinger |
| `ActivityService` | Bygger aktivitetsfeed fra ordredata (client-side) |
| `DirectoryService` | Søg deltagere |
| `MessageService` | Beskeder |
| `FriendService` | Venneliste |

## Dependency Injection

### API bootstrap (`Api/Program.cs`)
```csharp
services.AddDataStorage(connectionString);  // fra DataStorage
services.AddServiceLayer();                  // fra Service
services.AddSingleton<JwtTokenService>();
```

### Frontend
Alle Angular services er `providedIn: 'root'` (singleton).

## Dataflow – eksempel: Opret ordre

```
Angular CreateOrderComponent
  → POST /api/orders (med JWT)
	→ OrdersController.CreateOrder()
	  → IOrderService.CreateOrderAsync(dto)
		→ OrderRepository.AddAsync(entity)
		  → EF Core → SQL Server
```

## CORS

API tillader kald fra:
- `localhost:4200` / `localhost:4201` (lokalt)
- `icy-water-0750d2703.7.azurestaticapps.net` (Azure Static Web App)
- `paybysharepay.dk` / `www.paybysharepay.dk`

## Eventuelle arkitekturmæssige svagheder

- `ActivityService` på frontend bygger aktivitetsfeed client-side ved at mappe ordredata – der er ingen dedikeret aktivitetslog i backend
- Auth er "email-kun" MVP – ingen password-validering
- `appsettings.json` indeholder en lokal connection string som default
- JWT-nøgle i `appsettings.json` er en dev-placeholder (`SBYS-DEV-SECRET-...`) og skal overskrives i prod via miljøvariabel eller Azure Key Vault
- Ingen rate limiting eller brute-force-beskyttelse på login
