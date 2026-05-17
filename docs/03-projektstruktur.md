# 03 – Projektstruktur

## Solution-oversigt

```
PayBySharePPay/
├── src/
│   ├── Api.PayBySharePay/          ← ASP.NET Core Web API
│   ├── Service.PayBySharePay/      ← Forretningslogik (services + interfaces + DTOs)
│   ├── DataStorage.PayBySharePay/  ← EF Core DbContext, entities, repositories, migrations
│   ├── Frontend.PayBySharePay/     ← Angular SPA (primær bruger-app)
│   ├── Frontend.MerchantDemo/      ← Vanilla HTML/JS deltager-betalingsside
│   └── Tools.PayBySharePay/        ← Konsolapp til seed, flush og vedligehold
├── tests/
│   └── Tests.PayBySharePay/        ← Unit tests
├── docs/                           ← Dokumentation (denne mappe)
├── deploy-azure.ps1                ← Deployment script til Azure
├── PayBySharePPay.sln              ← Solution-fil
└── README.md
```

---

## Projektbeskrivelse

| Projekt | Formål | Vigtige filer |
|---|---|---|
| `Api.PayBySharePay` | Web API – controllers, auth, middleware, Swagger | [Program.cs](../src/Api.PayBySharePay/Program.cs) |
| `Service.PayBySharePay` | Forretningslogik, interfaces, DTOs | [OrderService.cs](../src/Service.PayBySharePay/Services/OrderService.cs) |
| `DataStorage.PayBySharePay` | EF Core DbContext, entities, repositories, migrations | [PayBySharePayDbContext.cs](../src/DataStorage.PayBySharePay/PayBySharePayDbContext.cs) |
| `Frontend.PayBySharePay` | Angular SPA til login, ordrestyring, beskeder | [app.component.ts](../src/Frontend.PayBySharePay/src/app/app.component.ts) |
| `Frontend.MerchantDemo` | Simpel HTML/JS side til deltagers betalingsvisning | [index.html](../src/Frontend.MerchantDemo/index.html) |
| `Tools.PayBySharePay` | Konsolapp: seed, flush, prod-maintenance | [Program.cs](../src/Tools.PayBySharePay/Program.cs) |
| `Tests.PayBySharePay` | Unit tests | [Tests.PayBySharePay/](../tests/Tests.PayBySharePay/) |

---

## Api.PayBySharePay – struktur

```
Api.PayBySharePay/
├── Auth/
│   └── JwtTokenService.cs
├── Controllers/
│   ├── AuthController.cs
│   ├── DirectoryController.cs
│   ├── FriendsController.cs
│   ├── MerchantOrdersController.cs
│   ├── MessagesController.cs
│   ├── OrdersController.cs
│   ├── ParticipantsController.cs
│   └── PaymentsController.cs
├── DTOs/
│   ├── CreateOrderRequest.cs
│   ├── CreatePersonRequest.cs
│   ├── LoginRequest.cs
│   └── ... (øvrige request-DTOs)
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Services/
│   └── MerchantDemoHostedService.cs
├── appsettings.json
├── appsettings.Production.json
└── Program.cs
```

---

## Service.PayBySharePay – struktur

```
Service.PayBySharePay/
├── DTOs/
│   ├── OrderDto.cs
│   ├── OrderOverviewDto.cs
│   ├── ParticipantDto.cs
│   ├── PaymentDto.cs
│   └── ...
├── Interfaces/
│   ├── IOrderService.cs
│   ├── IPaymentService.cs
│   └── ...
├── Services/
│   ├── OrderService.cs
│   ├── PaymentService.cs
│   ├── MessageService.cs
│   └── ...
└── ServiceLayerExtensions.cs
```

---

## DataStorage.PayBySharePay – struktur

```
DataStorage.PayBySharePay/
├── Entities/
│   ├── Order.cs
│   ├── OrderParticipant.cs
│   ├── Participant.cs
│   ├── Payment.cs
│   ├── Message.cs
│   ├── MerchantOrderDraft.cs
│   ├── MerchantOrderLine.cs
│   └── FriendRelation.cs
├── Migrations/
│   └── (EF Core migrations)
├── Repositories/
│   ├── OrderRepository.cs
│   ├── ParticipantRepository.cs
│   └── ...
├── PayBySharePayDbContext.cs
└── DataStorageServiceExtensions.cs
```

---

## Frontend.PayBySharePay – Angular features

```
Frontend.PayBySharePay/src/app/features/
├── activity/           ← Seneste aktivitet
├── create-order/       ← Opret ordre
├── find-participants/  ← Søg/tilføj deltagere
├── home/               ← Dashboard/forside
├── login/              ← Login-side
├── messages/           ← Beskeder
├── order-detail/       ← Ordredetaljer
├── orders/             ← Ordreoversigt
├── pending-participants/ ← Afventende deltagere
└── register/           ← Registrering
```

---

## Vigtigste filer en ny udvikler bør kende

| Fil | Hvorfor |
|---|---|
| [Program.cs](../src/Api.PayBySharePay/Program.cs) | Startup, DI, CORS, JWT, middleware |
| [OrderService.cs](../src/Service.PayBySharePay/Services/OrderService.cs) | Kernelogik for ordrer og notifikationer |
| [PayBySharePayDbContext.cs](../src/DataStorage.PayBySharePay/PayBySharePayDbContext.cs) | Database-model og EF Core konfiguration |
| [deploy-azure.ps1](../deploy-azure.ps1) | Deployment til Azure |
| [appsettings.json](../src/Api.PayBySharePay/appsettings.json) | Konfiguration (lokal) |
| [appsettings.Production.json](../src/Api.PayBySharePay/appsettings.Production.json) | Konfiguration (prod) |
| [index.html (MerchantDemo)](../src/Frontend.MerchantDemo/index.html) | Deltager-betalingsside |

---

## Se også

- [Backend](04-backend.md)
- [Frontend](05-frontend.md)
- [Database](07-database.md)
