# PayBySharePay – Dokumentation

> **Denne side er GitHub Wiki-hjemmesiden for PayBySharePay.**  
> Den er opdelt i en del til ikke-tekniske brugere og en teknisk del med kodeeksempler og links til kildekoden.

---

## Indholdsfortegnelse

| # | Afsnit |
|---|--------|
| 1 | [Hvad er PayBySharePay?](#1-hvad-er-paybysharepay) |
| 2 | [Hvem bruger systemet?](#2-hvem-bruger-systemet) |
| 3 | [Sådan virker det – trin for trin (ikke-teknisk)](#3-sådan-virker-det--trin-for-trin-ikke-teknisk) |
| 4 | [Systemarkitektur](#4-systemarkitektur) |
| 5 | [Klikbart flowdiagram](#5-klikbart-flowdiagram) |
| 6 | [Tekniske flows med kodeeksempler](#6-tekniske-flows-med-kodeeksempler) |
| 6.1 | [Registrering og login](#61-registrering-og-login) |
| 6.2 | [Oprettelse af ordre](#62-oprettelse-af-ordre) |
| 6.3 | [Deltager-flow via MerchantDemo](#63-deltager-flow-via-merchantdemo) |
| 6.4 | [Betalingsflow](#64-betalingsflow) |
| 7 | [API-reference](#7-api-reference) |
| 8 | [Database og datamodel](#8-database-og-datamodel) |
| 9 | [GitHub Actions Workflows og kendte problemer](#9-github-actions-workflows-og-kendte-problemer) |
| 10 | [Konfiguration og miljøer](#10-konfiguration-og-miljøer) |
| 11 | [Lokal udvikling](#11-lokal-udvikling) |

---

## 1. Hvad er PayBySharePay?

PayBySharePay er en webapplikation der gør det nemt at **dele regninger og koordinere gruppebetalinger**. Forestil dig, at du og dine kolleger vil bestille frokost fra det samme sted. Normalt opstår der rod: Hvem bestiller hvad? Hvem betaler? Hvem skylder hvem noget?

Med PayBySharePay løser det sig selv:

1. Én person (kaldet "host" eller "vært") opretter en ordre og inviterer de andre.
2. Alle modtager et link og kan vælge deres del af bestillingen.
3. Systemet holder styr på, hvem der har betalt, og hvem der mangler.
4. Når alle har betalt, markerer værten ordren som afsluttet.

**Ingen regneark. Ingen diskussioner. Ingen glemte betalinger.**

Systemet er primært udviklet til det danske marked og understøtter betalinger i DKK.

---

## 2. Hvem bruger systemet?

Systemet har tre typer brugere:

### 👤 Opretter / Vært (Host)
Den person der starter ordren. Det kan være dig, der ringer efter pizza til hele kontoret. Du logger ind i appen, opretter ordren, vælger spisested og inviterer dine kollegaer.

### 👥 Deltager
Den der modtager en invitation. Du får et link (fx via en besked i appen), klikker på det, vælger din mad og betaler din del. Du behøver ikke at logge ind i PayBySharePay for at bestille.

### 🏪 Merchant (Spisested / Forretning)
En registreret butik eller restaurant. Merchant-siden kan modtage gruppeordrer direkte fra PayBySharePay via en simpel demo-integration. Merchantens side viser bestillingsformularen til deltagerne.

---

## 3. Sådan virker det – trin for trin (ikke-teknisk)

### Flow 1: Vært opretter en frokostordre

```
1. Vært logger ind på PayBySharePay-appen
2. Klikker på "Opret ny ordre"
3. Giver ordren en titel (fx "Fredagspizza") og vælger et spisested
4. Tilføjer kollegaer som deltagere (søg på navn eller e-mail)
5. Klikker "Opret"
```

Når ordren er oprettet, sender systemet automatisk et **personligt link** til alle inviterede deltagere via beskeder i appen. Hvert link er unikt og tilhører én specifik person.

---

### Flow 2: Deltager bestiller via MerchantDemo

```
1. Deltager modtager en besked med et link
2. Klikker på linket → åbner spisestedets bestillingsside
3. Vælger retter fra menuen
4. Klikker "Bekræft bestilling"
5. Systemet registrerer bestillingen og notificerer værten
```

Når **alle** deltagere har bestilt, skifter ordren automatisk til status **"Klar til betaling"** og værten modtager en notifikation.

---

### Flow 3: Vært gennemfører betaling

```
1. Vært ser notifikation: "Alle har bestilt!"
2. Åbner ordren og ser overblik over hvad alle har valgt
3. Klikker "Gennemfør betaling"
4. Systemet sender betalingen til betalingsudbyderen
5. Ordren markeres som "Afsluttet"
```

---

### Ordrestatusser (livscyklus)

En ordre går igennem følgende faser:

```
Collecting  →  ReadyToPay  →  Completed
```

| Status | Hvad det betyder |
|--------|-----------------|
| **Collecting** | Ordren er oprettet og venter på, at deltagere bestiller |
| **ReadyToPay** | Alle deltagere har bestilt – vært kan nu betale |
| **Completed** | Betaling er gennemført – ordren er afsluttet |

---

## 4. Systemarkitektur

PayBySharePay er bygget op af fire uafhængige dele, der arbejder sammen:

| Komponent | Teknologi | Formål |
|-----------|-----------|--------|
| **API** | ASP.NET Core 9 (C#) | Håndterer al forretningslogik og data |
| **Frontend** | Angular (TypeScript) | Brugergrænsefladen til vært og administratorer |
| **MerchantDemo** | Vanilla HTML/CSS/JS | Simpel demo-side for deltagere og spisesteders integration |
| **Database** | SQL Server / Azure SQL | Gemmer alle ordrer, brugere, betalinger og beskeder |

### Deployment-miljøer

| Miljø | API | Frontend | MerchantDemo |
|-------|-----|----------|--------------|
| **TEST** | `paybysharepay-api.azurewebsites.net` | `purple-coast-0d01c1003.7.azurestaticapps.net` | `brave-flower-0026a7503.7.azurestaticapps.net` |
| **PROD** | `paybysharepay-api-win.azurewebsites.net` | `icy-water-0750d2703.7.azurestaticapps.net` | `ashy-bay-0e753db03.7.azurestaticapps.net` |

Alt kører på **Microsoft Azure**. API'et kører som en App Service (Windows), og de to frontends kører som Static Web Apps.

---

## 5. Klikbart flowdiagram

> **Klik på et område i diagrammet for at gå direkte til den relevante kode.**

```mermaid
flowchart TD
    A["👤 Vært logger ind\n/api/auth/login"] --> B["Opretter ordre\n/api/orders POST"]
    B --> C["Inviterer deltagere\nOrderService.CreateOrderAsync"]
    C --> D["Deltagere modtager link\nm. ParticipantToken"]
    D --> E["Deltager åbner MerchantDemo\nmed orderId + token"]
    E --> F["Deltager bestiller\n/api/merchant-orders POST"]
    F --> G{"Alle bestilt?"}
    G -- Nej --> E
    G -- Ja --> H["Status: ReadyToPay\nOrderService.CheckAndSetReadyToPayAsync"]
    H --> I["Vært ser notifikation"]
    I --> J["Vært gennemfører betaling\n/api/orders/:id/pay POST"]
    J --> K["ExternalPaymentService.ChargeAsync"]
    K --> L["Status: Completed"]

    click A "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Api.PayBySharePay/Controllers/AuthController.cs" "Gå til AuthController"
    click B "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Api.PayBySharePay/Controllers/OrdersController.cs" "Gå til OrdersController"
    click C "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Service.PayBySharePay/Services/OrderService.cs" "Gå til OrderService"
    click D "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/DataStorage.PayBySharePay/Entities/OrderParticipant.cs" "Gå til OrderParticipant"
    click F "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Api.PayBySharePay/Controllers/MerchantOrdersController.cs" "Gå til MerchantOrdersController"
    click G "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Service.PayBySharePay/Services/MerchantOrderService.cs" "Gå til MerchantOrderService"
    click H "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Service.PayBySharePay/Services/OrderService.cs#L317" "Gå til CheckAndSetReadyToPayAsync"
    click J "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Api.PayBySharePay/Controllers/OrdersController.cs#L81" "Gå til PayOrder endpoint"
    click K "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Service.PayBySharePay/Services/ExternalPaymentService.cs" "Gå til ExternalPaymentService"
```

---

### Betalingsflow i detaljer (klikbart)

```mermaid
flowchart LR
    P1["POST /api/payments\nRegisterPayment"] --> P2["PaymentService\n.RegisterPaymentAsync"]
    P2 --> P3["Opdater\nOrderParticipant.Status = Paid"]
    P3 --> P4["Send notifikation\ntil host via Messages"]

    O1["POST /api/orders/:id/pay\nPayOrder"] --> O2["ExternalPaymentService\n.ChargeAsync"]
    O2 --> O3["OrderService\n.CompleteOrderAsync"]
    O3 --> O4["Order.Status = Completed"]

    click P1 "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Api.PayBySharePay/Controllers/PaymentsController.cs" "Gå til PaymentsController"
    click P2 "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Service.PayBySharePay/Services/PaymentService.cs" "Gå til PaymentService"
    click O1 "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Api.PayBySharePay/Controllers/OrdersController.cs" "Gå til OrdersController"
    click O2 "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Service.PayBySharePay/Services/ExternalPaymentService.cs" "Gå til ExternalPaymentService"
    click O3 "https://github.com/mickni38-svg/PayBySharePPay/blob/main/src/Service.PayBySharePay/Services/OrderService.cs" "Gå til OrderService"
```

---

## 6. Tekniske flows med kodeeksempler

### 6.1 Registrering og login

**Filer:**
- Controller: [`AuthController.cs`](../src/Api.PayBySharePay/Controllers/AuthController.cs)
- Token-service: [`JwtTokenService.cs`](../src/Api.PayBySharePay/Auth/JwtTokenService.cs)

#### Login-flow

Login sker ved at sende en e-mail til `/api/auth/login`. Der er **ingen adgangskode** i MVP-versionen — systemet finder blot brugeren på e-mail og returnerer et JWT-token.

**HTTP Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "mads@example.dk"
}
```

**HTTP Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "participantId": 42,
  "name": "Mads Hansen",
  "expiresAt": "2025-05-24T13:00:00Z"
}
```

**Kode (AuthController.cs):**
```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var participants = await _participantService.SearchParticipantsAsync(request.Email);
    var person = participants.FirstOrDefault(p =>
        p.Type == "Person" &&
        string.Equals(p.Email, request.Email, StringComparison.OrdinalIgnoreCase));

    if (person is null)
        return Unauthorized(new { error = "Ingen bruger fundet med denne email." });

    var token = _tokenService.GenerateToken(person.Id, person.Name);
    return Ok(new LoginResponse { Token = token, ParticipantId = person.Id, Name = person.Name });
}
```

JWT-tokenet genereres i [`JwtTokenService.GenerateToken()`](../src/Api.PayBySharePay/Auth/JwtTokenService.cs) og indeholder brugerens `participantId` og navn som claims. Tokenet udløber efter **480 minutter (8 timer)**.

#### Registrering af ny bruger

```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "Mads Hansen",
  "email": "mads@example.dk",
  "phone": "+4512345678"
}
```

Hvis e-mailen allerede eksisterer, returneres `409 Conflict`. Ellers oprettes brugeren og et JWT returneres direkte (brugeren er automatisk logget ind).

#### Registrering af merchant (spisested)

```http
POST /api/auth/register-merchant
Content-Type: application/json

{
  "name": "Pizza Palace",
  "companyName": "Pizza Palace ApS",
  "cvrNumber": "12345678",
  "contactPerson": "Lars Jensen",
  "contactEmail": "lars@pizzapalace.dk",
  "contactPhone": "+4587654321",
  "companyAddress": "Hovedgaden 1, 8000 Aarhus"
}
```

---

### 6.2 Oprettelse af ordre

**Filer:**
- Controller: [`OrdersController.cs`](../src/Api.PayBySharePay/Controllers/OrdersController.cs)
- Service: [`OrderService.cs`](../src/Service.PayBySharePay/Services/OrderService.cs)
- Entity: [`Order.cs`](../src/DataStorage.PayBySharePay/Entities/Order.cs), [`OrderParticipant.cs`](../src/DataStorage.PayBySharePay/Entities/OrderParticipant.cs)

Alle ordre-endpoints kræver et gyldigt **JWT Bearer token** i headeren:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Opret ordre

```http
POST /api/orders
Authorization: Bearer <token>
Content-Type: application/json

{
  "createdByParticipantId": 1,
  "title": "Fredagspizza",
  "category": "Mad",
  "message": "Vi bestiller pizza kl. 12!",
  "merchantParticipantId": 5,
  "participantIds": [1, 2, 3, 4]
}
```

**Hvad sker der bag kulisserne?**

1. [`OrderService.CreateOrderAsync()`](../src/Service.PayBySharePay/Services/OrderService.cs) validerer at opretteren og alle deltagere eksisterer
2. En unik `JoinToken` (GUID) genereres til ordren
3. Hvert `OrderParticipant`-objekt får sit eget unikke `ParticipantToken` (bruges til MerchantDemo-linket)
4. Opretteren tilføjes automatisk med status `Accepted`; inviterede tilføjes med status `Invited`
5. Der sendes en besked til hvert `OrderParticipant` med et personligt bestillingslink

**Genereret bestillingslink til deltager:**
```
https://merchant-demo.azurestaticapps.net?orderId=42&merchantId=5&participantToken=abc123def456
```

**Kodeeksempel – generering af deltagerlink:**
```csharp
var participantLink = $"{baseUrl}?orderId={order.Id}&merchantId={merchant.Id}&participantToken={op.ParticipantToken}";
var msgText = isHost
    ? $"🍽️ Du har oprettet '{ordreNavn}' hos {merchant.CompanyName}. Bestil din mad her: {participantLink}"
    : $"🍽️ {creator.Name} har inviteret dig til '{ordreNavn}' hos {merchant.CompanyName}. Bestil din mad her: {participantLink}";
```

#### Hent ordredetaljer

```http
GET /api/orders/42/overview
Authorization: Bearer <token>
```

**Response:**
```json
{
  "orderId": 42,
  "title": "Fredagspizza",
  "status": "Collecting",
  "totalAmount": 387.50,
  "merchantName": "Pizza Palace ApS",
  "participants": [
    { "participantId": 1, "name": "Mads Hansen", "status": "Accepted" },
    { "participantId": 2, "name": "Sara Nielsen", "status": "Invited" },
    { "participantId": 3, "name": "Jonas Berg", "status": "OrderSubmitted" }
  ],
  "participantOrderLines": [
    {
      "participantId": 3,
      "participantName": "Jonas Berg",
      "hasPaid": false,
      "lines": [
        { "name": "Margherita", "quantity": 1, "unitPrice": 89.00, "lineTotal": 89.00 }
      ]
    }
  ]
}
```

---

### 6.3 Deltager-flow via MerchantDemo

**Filer:**
- Controller: [`MerchantOrdersController.cs`](../src/Api.PayBySharePay/Controllers/MerchantOrdersController.cs)
- Service: [`MerchantOrderService.cs`](../src/Service.PayBySharePay/Services/MerchantOrderService.cs)
- Entity: [`MerchantOrderDraft.cs`](../src/DataStorage.PayBySharePay/Entities/MerchantOrderDraft.cs), [`MerchantOrderLine.cs`](../src/DataStorage.PayBySharePay/Entities/MerchantOrderLine.cs)

MerchantDemo er en simpel HTML-side (vanilla JS), der simulerer et spisestedets bestillingsside. Den er hostet som en Azure Static Web App og kræver **ingen login**.

#### Deltager indsender bestilling

```http
POST /api/merchant-orders
Content-Type: application/json

{
  "orderId": 42,
  "merchantParticipantId": 5,
  "participantToken": "abc123def456",
  "merchantDraftReference": "REF-001",
  "subtotalAmount": 89.00,
  "totalAmount": 89.00,
  "currency": "DKK",
  "paymentMode": "AuthorizeThenCapture",
  "lines": [
    {
      "lineId": "line-001",
      "name": "Margherita",
      "quantity": 1,
      "unitPrice": 89.00,
      "lineTotal": 89.00
    }
  ]
}
```

> ⚠️ **Bemærk:** Dette endpoint er `[AllowAnonymous]` — deltagere behøver ikke logge ind. I stedet valideres de via `participantToken`.

**Hvad sker der bag kulisserne?**

1. [`MerchantOrderService.InitOrderAsync()`](../src/Service.PayBySharePay/Services/MerchantOrderService.cs) validerer `participantToken` mod `OrderParticipants`-tabellen
2. En `MerchantOrderDraft` oprettes med deltagerens bestillingslinjer
3. `OrderParticipant.Status` sættes til `OrderSubmitted`
4. [`OrderService.CheckAndSetReadyToPayAsync()`](../src/Service.PayBySharePay/Services/OrderService.cs#L317) tjekker om **alle** deltagere har submitted
5. Hvis alle har submitted, sættes `Order.Status = "ReadyToPay"` og en notifikation sendes til værten

**Kodeeksempel – automatisk ReadyToPay-tjek:**
```csharp
public async Task CheckAndSetReadyToPayAsync(int orderId)
{
    var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
    var nonMerchantParticipants = order.OrderParticipants
        .Where(op => op.Participant.Type != ParticipantType.Merchant)
        .ToList();

    var allSubmitted = nonMerchantParticipants.All(op => op.Status == "OrderSubmitted");
    if (allSubmitted)
    {
        order.Status = "ReadyToPay";
        order.Messages.Add(new Message
        {
            Content = $"✅ Alle deltagere har bestilt til '{order.Title}'. Du kan nu gennemføre betalingen."
        });
        await _orderRepository.SaveChangesAsync();
    }
}
```

---

### 6.4 Betalingsflow

Der er to måder at registrere en betaling på:

#### Metode A: Individuel betaling (deltager betaler sin del)

**Filer:**
- Controller: [`PaymentsController.cs`](../src/Api.PayBySharePay/Controllers/PaymentsController.cs)
- Service: [`PaymentService.cs`](../src/Service.PayBySharePay/Services/PaymentService.cs)

```http
POST /api/payments
Content-Type: application/json

{
  "orderId": 42,
  "participantId": 2,
  "amount": 127.50
}
```

Systemet registrerer betalingen, sætter `OrderParticipant.Status = "Paid"` og sender en notifikation til værten: `"✅ Sara Nielsen har betalt 127,50 kr."`

#### Metode B: Vært gennemfører samlet betaling via eksternt API

**Filer:**
- Controller: [`OrdersController.cs`](../src/Api.PayBySharePay/Controllers/OrdersController.cs#L81)
- Service: [`ExternalPaymentService.cs`](../src/Service.PayBySharePay/Services/ExternalPaymentService.cs)

```http
POST /api/orders/42/pay
Authorization: Bearer <token>
Content-Type: application/json

{
  "requestingParticipantId": 1,
  "amount": 0,
  "currency": "DKK"
}
```

> Hvis `amount = 0` bruges det samlede beløb fra ordren automatisk.

**Kodeeksempel – PayOrder endpoint:**
```csharp
[HttpPost("{id}/pay")]
public async Task<IActionResult> PayOrder(int id, [FromBody] PayOrderRequest request)
{
    var overview = await _orderService.GetOrderOverviewAsync(id);
    var amount = request.Amount > 0 ? request.Amount : overview.TotalAmount;

    // Kald eksternt betalings-API (pt. dummy — altid success)
    var paymentResult = await _externalPaymentService.ChargeAsync(new(
        OrderId: id,
        Amount: amount,
        Currency: request.Currency,
        Description: $"Gruppebetaling #{id}: {overview.Title}"
    ));

    if (!paymentResult.Success)
        return StatusCode(402, new { error = paymentResult.ErrorMessage });

    // Sæt ordre til Completed
    var order = await _orderService.CompleteOrderAsync(id, request.RequestingParticipantId);

    return Ok(new PayOrderResponse
    {
        OrderId = id,
        Status = order.Status,
        PaymentReference = paymentResult.PaymentReference
    });
}
```

> ⚠️ **TODO:** `ExternalPaymentService` er pt. en dummy-implementering der altid returnerer success med en tilfældig reference (`DUMMY-{orderId}-{guid}`). Den skal erstattes med en rigtig integration (Nets Easy, Stripe eller MobilePay) inden produktion.

---

## 7. API-reference

Alle endpoints undtagen `/api/auth/*` og `POST /api/merchant-orders` kræver:
```http
Authorization: Bearer <JWT-token>
```

### Auth

| Metode | URL | Beskrivelse | Auth |
|--------|-----|-------------|------|
| `POST` | `/api/auth/login` | Login med e-mail, returnerer JWT | Ingen |
| `POST` | `/api/auth/register` | Opret ny bruger | Ingen |
| `POST` | `/api/auth/register-merchant` | Opret ny merchant | Ingen |

**Controller:** [`AuthController.cs`](../src/Api.PayBySharePay/Controllers/AuthController.cs)

---

### Ordrer

| Metode | URL | Beskrivelse |
|--------|-----|-------------|
| `GET` | `/api/orders` | Hent alle ordrer (filter: `?participantId=1`) |
| `POST` | `/api/orders` | Opret ny ordre |
| `GET` | `/api/orders/{id}/overview` | Detaljeret overblik over én ordre |
| `POST` | `/api/orders/{id}/complete` | Sæt ordre til Completed (host only) |
| `POST` | `/api/orders/{id}/pay` | Gennemfør betaling via eksternt API |

**Controller:** [`OrdersController.cs`](../src/Api.PayBySharePay/Controllers/OrdersController.cs)

---

### Deltagere

| Metode | URL | Beskrivelse |
|--------|-----|-------------|
| `GET` | `/api/participants/search` | Søg deltagere (`?query=navn&initiatorId=1`) |
| `POST` | `/api/participants/person` | Opret Person-deltager |
| `POST` | `/api/participants/merchant` | Opret Merchant-deltager |

**Controller:** [`ParticipantsController.cs`](../src/Api.PayBySharePay/Controllers/ParticipantsController.cs)

---

### Betalinger

| Metode | URL | Beskrivelse |
|--------|-----|-------------|
| `POST` | `/api/payments` | Registrer en betaling |

**Controller:** [`PaymentsController.cs`](../src/Api.PayBySharePay/Controllers/PaymentsController.cs)

---

### Merchant-ordrer

| Metode | URL | Beskrivelse | Auth |
|--------|-----|-------------|------|
| `POST` | `/api/merchant-orders` | Deltager indsender bestilling via token | Ingen |
| `GET` | `/api/merchant-orders/by-order/{orderId}` | Hent merchant order draft | JWT |

**Controller:** [`MerchantOrdersController.cs`](../src/Api.PayBySharePay/Controllers/MerchantOrdersController.cs)

---

### Venner

| Metode | URL | Beskrivelse |
|--------|-----|-------------|
| `GET` | `/api/friends/{participantId}` | Hent venneliste |
| `POST` | `/api/friends` | Tilføj ven |

**Controller:** [`FriendsController.cs`](../src/Api.PayBySharePay/Controllers/FriendsController.cs)

---

### Beskeder

| Metode | URL | Beskrivelse |
|--------|-----|-------------|
| `GET` | `/api/messages` | Hent beskeder for indlogget bruger |
| `POST` | `/api/messages` | Opret besked |
| `PUT` | `/api/messages/{id}/read` | Markér besked som læst |

**Controller:** [`MessagesController.cs`](../src/Api.PayBySharePay/Controllers/MessagesController.cs)

---

## 8. Database og datamodel

**Filer:**
- DbContext: [`PayBySharePayDbContext.cs`](../src/DataStorage.PayBySharePay/Context/PayBySharePayDbContext.cs)
- Entities: [`src/DataStorage.PayBySharePay/Entities/`](../src/DataStorage.PayBySharePay/Entities/)

Databasen er en **SQL Server** database (Azure SQL i produktion) styret med **Entity Framework Core Code First**. Det betyder, at databasestrukturen er defineret i C#-kode og migreres automatisk.

### ER-diagram (Entity Relationship)

```mermaid
erDiagram
    Participant {
        int Id PK
        string Type
        string Name
        string Email
        string Phone
        string CompanyName
        string CvrNumber
        string GroupOrderUrl
    }
    Order {
        int Id PK
        int CreatedByParticipantId FK
        int MerchantParticipantId FK
        string Title
        string Status
        string JoinToken
        datetime CreatedAt
    }
    OrderParticipant {
        int Id PK
        int OrderId FK
        int ParticipantId FK
        string Status
        string ParticipantToken
    }
    Payment {
        int Id PK
        int OrderId FK
        int ParticipantId FK
        decimal Amount
        string Status
        datetime CreatedAt
    }
    Message {
        int Id PK
        int OrderId FK
        int ParticipantId FK
        string Content
        datetime CreatedAt
    }
    MerchantOrderDraft {
        int Id PK
        int OrderId FK
        int MerchantParticipantId FK
        int ParticipantId FK
        decimal TotalAmount
        string Status
        string Currency
    }
    MerchantOrderLine {
        int Id PK
        int MerchantOrderDraftId FK
        int ParticipantId FK
        string Name
        int Quantity
        decimal UnitPrice
        decimal LineTotal
    }

    Participant ||--o{ OrderParticipant : "deltager i"
    Order ||--o{ OrderParticipant : "har"
    Order ||--o{ Payment : "har"
    Order ||--o{ Message : "har"
    Order ||--o{ MerchantOrderDraft : "har"
    MerchantOrderDraft ||--o{ MerchantOrderLine : "indeholder"
    Participant ||--o{ FriendRelation : "initierer"
```

### Ordrestatusser

| `Order.Status` | Forklaring |
|----------------|------------|
| `Collecting` | Standardstatus – venter på bestillinger |
| `ReadyToPay` | Alle deltagere har indsendt – klar til betaling |
| `Completed` | Betaling gennemført |

### Deltagerstatusser i ordre (`OrderParticipant.Status`)

| Status | Forklaring |
|--------|------------|
| `Invited` | Inviteret, har ikke taget aktion |
| `Accepted` | Har accepteret invitationen (host sættes automatisk) |
| `OrderSubmitted` | Har indsendt bestilling via MerchantDemo |
| `Paid` | Har registreret betaling |

---

## 9. GitHub Actions Workflows og kendte problemer

**Workflow-filer:** [`.github/workflows/`](../.github/workflows/)

Vi bruger GitHub Actions til at **bygge og deploye** systemet automatisk. Her er en oversigt over de eksisterende workflows:

### Oversigt over workflows

| Workflow-fil | Trigger | Formål |
|---|---|---|
| [`build.yml`](../.github/workflows/build.yml) | Push/PR til `main` | Bygger og tester API + Frontend |
| [`deploy-api.yml`](../.github/workflows/deploy-api.yml) | Manuel (`workflow_dispatch`) | Deployer kun API til TEST |
| [`deploy-frontend.yml`](../.github/workflows/deploy-frontend.yml) | Manuel | Deployer kun Angular Frontend til TEST |
| [`deploy-merchantdemo.yml`](../.github/workflows/deploy-merchantdemo.yml) | Manuel | Deployer kun MerchantDemo til TEST |
| [`deploy-test.yml`](../.github/workflows/deploy-test.yml) | Manuel | Deployer **alle tre** komponenter til TEST i ét trin |

### Build-workflow i detaljer

`build.yml` kører ved hvert push til `main` og ved alle pull requests. Den har to parallelle jobs:

```yaml
# Job 1: .NET API
- dotnet restore
- dotnet build --configuration Release
- dotnet test

# Job 2: Angular Frontend  
- npm ci
- npx ng build --configuration production
```

### Deploy-workflow: TEST (alle komponenter)

`deploy-test.yml` deployer alt på én gang og er typisk det workflow vi bruger under aktiv udvikling:

1. Angular bygges med `--configuration test` (bruger `environment.test.ts`)
2. Angular-dist deployes til Azure Static Web App via `@azure/static-web-apps-cli`
3. MerchantDemo-mappen deployes direkte (ingen build-step)
4. .NET API publishes og pakkes som ZIP
5. ZIP deployes til Azure App Service via `az webapp deploy`

---

### ⚠️ Kendte problemer og erfaringer med workflows

#### Problem 1: `dist`-mappenavn efter Angular-build

**Problem:** Efter `ng build` lander output i `./dist/frontend.paybysharepay/browser/` i nyere Angular-versioner, men deploy-scriptet pegede på `./dist/frontend.paybysharepay/`. Dette medførte, at deploymentet kørte uden fejl, men den uploadede mappe var tom eller forkert.

**Løsning:** Deploy-kommandoen er opdateret til at pege på den korrekte sti, og vi tilføjede et `ls -l ./dist`-step i `deploy-test.yml` for at verificere indholdet under CI-kørslen.

```yaml
- name: List dist folder contents
  working-directory: src/Frontend.PayBySharePay
  run: ls -l ./dist
```

---

#### Problem 2: ZIP-kommando i deploy-test.yml

**Problem:** I `deploy-test.yml` bruges kommandoen:
```yaml
run: zip -r publish-output.zip ./*
working-directory: ./publish-output
```
Her zippes indholdet af mappen, men der opstod fejl fordi `working-directory` sættes *efter* at `run`-stien er evalueret, hvilket på visse GitHub runner-versioner giver en relativ stikonflikt. Den korrekte ZIP-sti var `./publish-output/publish-output.zip`.

**Løsning:** Brugen af `az webapp deploy --src-path ./publish-output/publish-output.zip` løste problemet ved eksplicit at angive den absolutte sti fra repo-roden.

---

#### Problem 3: HTTPS-redirect bryder MerchantDemo lokalt

**Problem:** Under lokal udvikling kører MerchantDemo på `http://localhost:8081` (plain HTTP). Hvis `UseHttpsRedirection()` er aktiv i API'et, vil browseren følge redirect til `https://localhost:7071`, men den self-signede HTTPS-certifikat accepteres ikke af vanilla JS `fetch`-kald, og CORS-headerne matcher ikke.

**Løsning:** `UseHttpsRedirection` er konditionelt deaktiveret i development:
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
```
Og MerchantDemo's `http://localhost:8081` er whitelistet i CORS-konfigurationen.

---

#### Problem 4: Azure login credentials i secrets

**Problem:** Workflowet bruger `${{ secrets.AZURE_CREDENTIALS }}` til at logge ind på Azure CLI. Det kræver at en service principal er oprettet med `Contributor`-rettigheder på resource group `paybysharepay-rg`, og JSON-credentials er gemt som et repository secret. Mangler dette secret vil deploy-jobbet fejle med en uklar "Login failed"-besked.

**Løsning:** Secrets skal oprettes i GitHub under Settings → Secrets and variables → Actions:

| Secret-navn | Beskrivelse |
|---|---|
| `AZURE_CREDENTIALS` | Service principal JSON fra `az ad sp create-for-rbac` |
| `SWA_TOKEN_FRONTEND_TEST` | Deployment token fra Azure Static Web App (Frontend TEST) |
| `SWA_TOKEN_MERCHANT_TEST` | Deployment token fra Azure Static Web App (MerchantDemo TEST) |

---

#### Problem 5: MerchantOrderService – re-submit håndtering

**Problem:** Hvis en deltager forsøgte at genbestille (klikke "Bestil" igen på MerchantDemo) efter at have indsendt, opstod en database-fejl fordi en unik constraint på `(OrderId, ParticipantId)` i drafts-tabellen blev overtrådt.

**Løsning:** [`MerchantOrderService.InitOrderAsync()`](../src/Service.PayBySharePay/Services/MerchantOrderService.cs) sletter nu eksisterende draft for samme deltager inden den nye gemmes:
```csharp
// Slet eventuel eksisterende draft for samme deltager (re-submit)
var existing = await _db.MerchantOrderDrafts
    .Where(d => d.OrderId == dto.OrderId && d.ParticipantId == orderParticipant.ParticipantId)
    .FirstOrDefaultAsync();
if (existing != null)
{
    _db.MerchantOrderDrafts.Remove(existing);
    await _db.SaveChangesAsync();
}
```

---

## 10. Konfiguration og miljøer

**Filer:** [`appsettings.json`](../src/Api.PayBySharePay/appsettings.json), [`appsettings.Test.json`](../src/Api.PayBySharePay/appsettings.Test.json)

### API-konfiguration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "PayBySharePayDb": "Server=...;Database=PayBySharePayDb;..."
  },
  "Jwt": {
    "Key": "<hemmelig-nøgle-min-32-tegn>",
    "Issuer": "PayBySharePay",
    "Audience": "PayBySharePayUsers",
    "ExpiresInMinutes": "480"
  },
  "AppSettings": {
    "MerchantDemoUrl": "http://localhost:8081",
    "FrontendUrl": "http://localhost:4200"
  }
}
```

### Angular-miljøer

Angular-projektet har to build-konfigurationer:

| Konfiguration | API URL | Bruges til |
|---|---|---|
| `production` | `https://paybysharepay-api-win.azurewebsites.net` | PROD |
| `test` | `https://paybysharepay-api.azurewebsites.net` | TEST |

---

## 11. Lokal udvikling

### Krav

- .NET 9 SDK
- Node.js 20+ og npm
- Angular CLI (`npm install -g @angular/cli`)
- SQL Server (lokal) eller adgang til Azure SQL

### Start API lokalt

```bash
# Fra repo-roden
cd src/Api.PayBySharePay
dotnet run
# API kører på https://localhost:7071
# Swagger UI: https://localhost:7071/swagger
```

### Start Angular Frontend lokalt

```bash
cd src/Frontend.PayBySharePay
npm install
npx ng serve
# Frontend kører på http://localhost:4200
```

### Start MerchantDemo lokalt

MerchantDemo er statisk HTML/JS og kan åbnes direkte i browseren, eller serves med en simpel HTTP-server:

```bash
# Kræver Node.js http-server eller lignende
npx serve src/Frontend.MerchantDemo -p 8081
# MerchantDemo kører på http://localhost:8081
```

### Database-migrering

```bash
cd src/Api.PayBySharePay
dotnet ef database update
```

---

## Se også

- [Eksisterende teknisk dokumentation (`docs/`)](../docs/)
- [Swagger UI (TEST)](https://paybysharepay-api.azurewebsites.net/swagger)
- [Swagger UI (PROD)](https://paybysharepay-api-win.azurewebsites.net/swagger)
