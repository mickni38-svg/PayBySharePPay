# 04 – Backend

## Teknologi

- **ASP.NET Core 9** Web API
- **C# 13**
- **Entity Framework Core** (Code First)
- **JWT Bearer** authentication
- **Swagger/OpenAPI** dokumentation

---

## Vigtige backend-komponenter

| Komponent | Beskrivelse | Link |
|---|---|---|
| `Program.cs` | Startup, DI-registrering, CORS, JWT, middleware | [Program.cs](../src/Api.PayBySharePay/Program.cs) |
| `AuthController` | Login og registrering | [AuthController.cs](../src/Api.PayBySharePay/Controllers/AuthController.cs) |
| `OrdersController` | CRUD for ordrer | [OrdersController.cs](../src/Api.PayBySharePay/Controllers/OrdersController.cs) |
| `ParticipantsController` | Deltagerstyring | [ParticipantsController.cs](../src/Api.PayBySharePay/Controllers/ParticipantsController.cs) |
| `PaymentsController` | Betalingsregistrering | [PaymentsController.cs](../src/Api.PayBySharePay/Controllers/PaymentsController.cs) |
| `MessagesController` | Besked-endpoints | [MessagesController.cs](../src/Api.PayBySharePay/Controllers/MessagesController.cs) |
| `MerchantOrdersController` | Merchant-/deltager-flow (ingen auth) | [MerchantOrdersController.cs](../src/Api.PayBySharePay/Controllers/MerchantOrdersController.cs) |
| `DirectoryController` | Søg i brugermappe | [DirectoryController.cs](../src/Api.PayBySharePay/Controllers/DirectoryController.cs) |
| `FriendsController` | Venneliste | [FriendsController.cs](../src/Api.PayBySharePay/Controllers/FriendsController.cs) |
| `OrderService` | Forretningslogik for ordrer og notifikationer | [OrderService.cs](../src/Service.PayBySharePay/Services/OrderService.cs) |
| `PaymentService` | Betalingslogik | [PaymentService.cs](../src/Service.PayBySharePay/Services/PaymentService.cs) |
| `MessageService` | Beskedlogik | [MessageService.cs](../src/Service.PayBySharePay/Services/MessageService.cs) |
| `MerchantOrderService` | Merchant-ordrelogik | [MerchantOrderService.cs](../src/Service.PayBySharePay/Services/MerchantOrderService.cs) |
| `JwtTokenService` | Genererer JWT-tokens | [JwtTokenService.cs](../src/Api.PayBySharePay/Auth/JwtTokenService.cs) |
| `ExceptionHandlingMiddleware` | Global fejlhåndtering | [ExceptionHandlingMiddleware.cs](../src/Api.PayBySharePay/Middleware/ExceptionHandlingMiddleware.cs) |
| `MerchantDemoHostedService` | Lokal dev-server for MerchantDemo | [MerchantDemoHostedService.cs](../src/Api.PayBySharePay/Services/MerchantDemoHostedService.cs) |

---

## Services

### OrderService
Kernen i løsningen. Håndterer:
- `CreateOrderAsync(dto)` – Opretter ordre, tilknytter deltagere, sender beskedlinks
- `GetOrderOverviewAsync(id)` – Henter overblik over ordre
- `GetOrdersByParticipantAsync(participantId)` – Filtrerer ordrer pr. deltager
- `GetAllOrdersAsync()` – Henter alle ordrer

**Vigtigt:** Merchant-notifikationslinks sammensættes dynamisk:
```
{merchant.GroupOrderUrl}?token={participant.Token}&api={_apiBaseUrl}
```
URL'erne hentes fra `appsettings.json` → `AppSettings:ApiBaseUrl` og `AppSettings:MerchantDemoUrl`.

### PaymentService
- `RegisterPaymentAsync(dto)` – Registrerer en betaling
- `GetPaymentsByOrderAsync(orderId)` – Henter betalinger for en ordre

### MessageService
- `GetMessagesAsync(participantId)` – Henter beskeder for en deltager
- `MarkAsReadAsync(messageId)` – Markerer besked som læst

---

## Repositories

Alle repositories følger interface-mønsteret:

| Repository | Interface | Entitet |
|---|---|---|
| `OrderRepository` | `IOrderRepository` | `Order` |
| `ParticipantRepository` | `IParticipantRepository` | `Participant` |
| `PaymentRepository` | `IPaymentRepository` | `Payment` |
| `MessageRepository` | `IMessageRepository` | `Message` |
| `MerchantOrderDraftRepository` | `IMerchantOrderDraftRepository` | `MerchantOrderDraft` |
| `FriendRelationRepository` | `IFriendRelationRepository` | `FriendRelation` |

---

## Dependency Injection

DI registreres i:
- [`ServiceLayerExtensions.cs`](../src/Service.PayBySharePay/ServiceLayerExtensions.cs) – services
- [`DataStorageServiceExtensions.cs`](../src/DataStorage.PayBySharePay/DataStorageServiceExtensions.cs) – repositories og DbContext
- [`Program.cs`](../src/Api.PayBySharePay/Program.cs) – auth, CORS, controllers, Swagger

---

## Middleware

- **ExceptionHandlingMiddleware** – fanger uventede exceptions og returnerer en struktureret fejlrespons
- **CORS** – `Frontend`-policy konfigureret med whitelist af origins
- **Authentication/Authorization** – JWT Bearer middleware

---

## Background Services

- **MerchantDemoHostedService** – startes kun i Development. Starter en lokal webserver til MerchantDemo-siden (via `http-server` / npm start).

---

## Validering og fejlhåndtering

- Controller-niveau: `[FromBody]` med modelvalidering
- Service-niveau: exception kastes ved fejl (fanges af middleware)
- Globale HTTP-fejl returneres som JSON via `ExceptionHandlingMiddleware`

---

## Swagger

Swagger/OpenAPI er tilgængeligt lokalt:
```
https://localhost:7071/swagger
```

I produktion er Swagger **deaktiveret** (kun Development).

---

## Se også

- [API endpoints](06-api-endpoints.md)
- [Database](07-database.md)
- [Authentication og security](08-authentication-security.md)
- [Konfiguration](09-konfiguration.md)
