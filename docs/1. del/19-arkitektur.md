# Arkitektur – PayBySharePay

## Oversigt

PayBySharePay er en lagdelt .NET 9 Web API-løsning med en Angular-frontend. Afhængighedsretningen er ensrettet:

```
Frontend.PayBySharePay (Angular)
        │  HTTP/REST
        ▼
Api.PayBySharePay (.NET 9 Web API)
        │  Interface-kald
        ▼
Service.PayBySharePay (.NET 9 Class Library)
        │  Interface-kald
        ▼
DataStorage.PayBySharePay (.NET 9 Class Library)
        │  EF Core
        ▼
SQL Server
```

Frontend kommunikerer udelukkende via HTTP med backend. Frontend refererer ikke direkte til backend-projekterne.

---

## Projekt: Api.PayBySharePay

**Ansvar:** HTTP endpoints, request/response DTO'er, inputvalidering, DI-opsætning.

### Mappestruktur

```
Api.PayBySharePay/
├── Controllers/
│   ├── ParticipantsController.cs   # Søg, opret person, opret merchant
│   ├── FriendsController.cs        # Tilføj venrelation
│   ├── OrdersController.cs         # Opret ordre, hent overblik
│   ├── PaymentsController.cs       # Registrér betaling
│   └── MessagesController.cs       # Hent og opret beskeder
├── DTOs/
│   ├── CreatePersonRequest.cs
│   ├── CreateMerchantRequest.cs
│   ├── AddFriendRequest.cs
│   ├── CreateOrderRequest.cs
│   ├── RegisterPaymentRequest.cs
│   └── CreateMessageRequest.cs
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── appsettings.json
└── Program.cs
```

### Program.cs

Registrerer Swagger, CORS, data storage-lag og service-lag via extensions-metoder. `ExceptionHandlingMiddleware` håndterer fejl centralt.

### Fejlhåndtering

`ExceptionHandlingMiddleware` fanger exceptions og returnerer strukturerede JSON-fejlsvar:

| Exception | HTTP |
|-----------|------|
| `ArgumentException` | 400 |
| `KeyNotFoundException` | 404 |
| `InvalidOperationException` | 409 |
| Øvrige | 500 |

---

## Projekt: Service.PayBySharePay

**Ansvar:** Forretningsregler, use cases, mapping mellem entiteter og DTO'er.

### Interfaces

| Interface | Implementering |
|-----------|---------------|
| `IParticipantService` | `ParticipantService` |
| `IOrderService` | `OrderService` |
| `IPaymentService` | `PaymentService` |
| `IMessageService` | `MessageService` |

### DTO'er

Service-laget eksponerer egne DTO'er som API-laget mapper til/fra request-objekter:

- `ParticipantDto`
- `CreatePersonDto`
- `CreateMerchantDto`
- `AddFriendDto`
- `OrderDto`
- `OrderOverviewDto`
- `CreateOrderDto`
- `RegisterPaymentDto`
- `CreateMessageDto`

---

## Projekt: DataStorage.PayBySharePay

**Ansvar:** EF Core entiteter, DbContext, repositories og queries.

### Entiteter

| Entitet | Beskrivelse |
|---------|-------------|
| `Participant` | Person eller merchant (bestemt af `ParticipantType`) |
| `FriendRelation` | Relation mellem to deltagere |
| `Order` | Fælles bestilling |
| `OrderParticipant` | Kobling mellem ordre og deltager |
| `Payment` | Betaling registreret på en ordre |
| `Message` | Besked knyttet til en ordre |

### ParticipantType enum

```csharp
public enum ParticipantType
{
    Person,
    Merchant
}
```

### Repositories

| Interface | Implementering |
|-----------|---------------|
| `IParticipantRepository` | `ParticipantRepository` |
| `IFriendRelationRepository` | `FriendRelationRepository` |
| `IOrderRepository` | `OrderRepository` |
| `IPaymentRepository` | `PaymentRepository` |
| `IMessageRepository` | `MessageRepository` |

### DbContext

`PayBySharePayDbContext` konfigurerer relationer:

- `FriendRelation` → `Initiator` og `Receiver` (begge `Restrict` sletning)
- `OrderParticipant` → `Order` (`Cascade`) og `Participant` (`Restrict`)
- `Payment` → `Order` (`Cascade`) og `Participant` (`Restrict`)
- `Message` → `Order` (`Cascade`) og `Participant` (`Restrict`)

---

## Projekt: Frontend.PayBySharePay

**Ansvar:** Angular UI, routing, HTTP-kald via services, responsive layouts.

- Mobil-først, tilpasset iPhone 14
- Brug af Angular services til HTTP-kommunikation
- Typede TypeScript interfaces/modeller
- Bundnavigation, søgefelt, statuschips

---

## Designprincipper

- Interfaces i service- og repository-lag
- `async`/`await` på alle I/O-operationer
- DTO'er adskiller lag
- Ingen forretningslogik i controllers
- Ingen direkte databaseadgang i service-laget
- DI registreres via extensions-metoder (`AddDataStorage`, `AddServiceLayer`)
