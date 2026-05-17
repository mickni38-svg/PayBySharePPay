# 14 – Vigtige Kode-links

## Startup og konfiguration

- [`Program.cs`](../src/Api.PayBySharePay/Program.cs) – DI, CORS, JWT, middleware, startup
- [`appsettings.json`](../src/Api.PayBySharePay/appsettings.json) – Lokal konfiguration
- [`appsettings.Production.json`](../src/Api.PayBySharePay/appsettings.Production.json) – Produktion konfiguration
- [`ServiceLayerExtensions.cs`](../src/Service.PayBySharePay/ServiceLayerExtensions.cs) – DI for services
- [`DataStorageServiceExtensions.cs`](../src/DataStorage.PayBySharePay/DataStorageServiceExtensions.cs) – DI for repositories og DbContext

---

## Controllers

- [`AuthController.cs`](../src/Api.PayBySharePay/Controllers/AuthController.cs) – Login, register
- [`OrdersController.cs`](../src/Api.PayBySharePay/Controllers/OrdersController.cs) – Ordre CRUD
- [`ParticipantsController.cs`](../src/Api.PayBySharePay/Controllers/ParticipantsController.cs) – Deltager CRUD
- [`PaymentsController.cs`](../src/Api.PayBySharePay/Controllers/PaymentsController.cs) – Betalingsregistrering
- [`MessagesController.cs`](../src/Api.PayBySharePay/Controllers/MessagesController.cs) – Beskeder
- [`MerchantOrdersController.cs`](../src/Api.PayBySharePay/Controllers/MerchantOrdersController.cs) – Merchant-/deltagerflow
- [`DirectoryController.cs`](../src/Api.PayBySharePay/Controllers/DirectoryController.cs) – Brugersøgning
- [`FriendsController.cs`](../src/Api.PayBySharePay/Controllers/FriendsController.cs) – Venneliste

---

## Services (forretningslogik)

- [`OrderService.cs`](../src/Service.PayBySharePay/Services/OrderService.cs) – Kernelogik for ordrer og notifikationslinks
- [`PaymentService.cs`](../src/Service.PayBySharePay/Services/PaymentService.cs) – Betalingslogik
- [`MessageService.cs`](../src/Service.PayBySharePay/Services/MessageService.cs) – Beskedlogik
- [`MerchantOrderService.cs`](../src/Service.PayBySharePay/Services/MerchantOrderService.cs) – Merchant-ordrelogik
- [`ParticipantService.cs`](../src/Service.PayBySharePay/Services/ParticipantService.cs) – Deltagerlogik
- [`DirectoryService.cs`](../src/Service.PayBySharePay/Services/DirectoryService.cs) – Søgelogik

---

## Interfaces

- [`IOrderService.cs`](../src/Service.PayBySharePay/Interfaces/IOrderService.cs)
- [`IPaymentService.cs`](../src/Service.PayBySharePay/Interfaces/IPaymentService.cs)
- [`IMessageService.cs`](../src/Service.PayBySharePay/Interfaces/IMessageService.cs)
- [`IMerchantOrderService.cs`](../src/Service.PayBySharePay/Interfaces/IMerchantOrderService.cs)
- [`IParticipantService.cs`](../src/Service.PayBySharePay/Interfaces/IParticipantService.cs)

---

## Data Access

- [`PayBySharePayDbContext.cs`](../src/DataStorage.PayBySharePay/PayBySharePayDbContext.cs) – EF Core DbContext
- [`OrderRepository.cs`](../src/DataStorage.PayBySharePay/Repositories/OrderRepository.cs)
- [`ParticipantRepository.cs`](../src/DataStorage.PayBySharePay/Repositories/ParticipantRepository.cs)
- [`PaymentRepository.cs`](../src/DataStorage.PayBySharePay/Repositories/PaymentRepository.cs)
- [`MessageRepository.cs`](../src/DataStorage.PayBySharePay/Repositories/MessageRepository.cs)
- [`MerchantOrderDraftRepository.cs`](../src/DataStorage.PayBySharePay/Repositories/MerchantOrderDraftRepository.cs)

---

## Entiteter / Models

- [`Order.cs`](../src/DataStorage.PayBySharePay/Entities/Order.cs)
- [`OrderParticipant.cs`](../src/DataStorage.PayBySharePay/Entities/OrderParticipant.cs)
- [`Participant.cs`](../src/DataStorage.PayBySharePay/Entities/Participant.cs)
- [`Payment.cs`](../src/DataStorage.PayBySharePay/Entities/Payment.cs)
- [`Message.cs`](../src/DataStorage.PayBySharePay/Entities/Message.cs)
- [`MerchantOrderDraft.cs`](../src/DataStorage.PayBySharePay/Entities/MerchantOrderDraft.cs)
- [`MerchantOrderLine.cs`](../src/DataStorage.PayBySharePay/Entities/MerchantOrderLine.cs)
- [`FriendRelation.cs`](../src/DataStorage.PayBySharePay/Entities/FriendRelation.cs)

---

## DTOs

- [`OrderDto.cs`](../src/Service.PayBySharePay/DTOs/OrderDto.cs)
- [`OrderOverviewDto.cs`](../src/Service.PayBySharePay/DTOs/OrderOverviewDto.cs)
- [`ParticipantDto.cs`](../src/Service.PayBySharePay/DTOs/ParticipantDto.cs)
- [`PaymentDto.cs`](../src/Service.PayBySharePay/DTOs/PaymentDto.cs)
- [`MessageDto.cs`](../src/Service.PayBySharePay/DTOs/MessageDto.cs)

---

## Authentication

- [`JwtTokenService.cs`](../src/Api.PayBySharePay/Auth/JwtTokenService.cs) – Genererer JWT tokens

---

## Middleware

- [`ExceptionHandlingMiddleware.cs`](../src/Api.PayBySharePay/Middleware/ExceptionHandlingMiddleware.cs) – Global fejlhåndtering

---

## Frontend

- [`environment.prod.ts`](../src/Frontend.PayBySharePay/src/environments/environment.prod.ts) – Prod API URL
- [`index.html (MerchantDemo)`](../src/Frontend.MerchantDemo/index.html) – Deltager-betalingsside

---

## Deployment og tooling

- [`deploy-azure.ps1`](../deploy-azure.ps1) – Deployment script til Azure
- [`Tools.PayBySharePay/Program.cs`](../src/Tools.PayBySharePay/Program.cs) – Seed, flush, vedligehold

---

## Migrations (nyeste)

- [`20260516151034_AddMessageIsRead.cs`](../src/DataStorage.PayBySharePay/Migrations/20260516151034_AddMessageIsRead.cs) – Seneste migration
