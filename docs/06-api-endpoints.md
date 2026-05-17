# 06 – API Endpoints

> Swagger/OpenAPI er tilgængeligt lokalt på: `https://localhost:7071/swagger`

Alle endpoints kræver **JWT Bearer authentication** medmindre andet er angivet.

---

## Auth

### POST /api/auth/login
**Formål:** Logger en bruger ind og returnerer JWT-token.  
**Auth:** Ingen  
**Controller:** [`AuthController`](../src/Api.PayBySharePay/Controllers/AuthController.cs)

**Request:**
```json
{ "email": "bruger@example.com", "password": "hemmeligt" }
```
**Response:**
```json
{ "token": "eyJ..." }
```

---

### POST /api/auth/register
**Formål:** Registrerer ny bruger/deltager.  
**Auth:** Ingen

**Request:**
```json
{ "name": "Navn", "email": "email@example.com", "password": "hemmeligt" }
```

---

## Ordrer

### GET /api/orders
**Formål:** Henter alle ordrer (kan filtreres på deltager).  
**Controller:** [`OrdersController`](../src/Api.PayBySharePay/Controllers/OrdersController.cs)

**Query params:** `?participantId=1` (valgfri)

---

### POST /api/orders
**Formål:** Opretter en ny ordre med deltagere.

**Request:**
```json
{
  "createdByParticipantId": 1,
  "title": "Pizza fredag",
  "category": "Mad",
  "message": "Hej – vi spiser pizza!",
  "merchantParticipantId": 2,
  "participantIds": [1, 2, 3]
}
```

**Response:** `201 Created` med `OrderDto`

---

### GET /api/orders/{id}/overview
**Formål:** Henter detaljeret overblik over en ordre inkl. deltagere og betalingsstatus.

**Response:** `OrderOverviewDto`

---

### POST /api/orders/{id}/complete
**Formål:** Markerer en ordre som afsluttet.

---

## Deltagere

### GET /api/participants
**Formål:** Henter alle deltagere.  
**Controller:** [`ParticipantsController`](../src/Api.PayBySharePay/Controllers/ParticipantsController.cs)

---

### POST /api/participants
**Formål:** Opretter en ny deltager.

---

### GET /api/participants/{id}
**Formål:** Henter én deltager.

---

## Betalinger

### POST /api/payments
**Formål:** Registrerer en betaling for en ordredeltager.  
**Controller:** [`PaymentsController`](../src/Api.PayBySharePay/Controllers/PaymentsController.cs)

**Request:**
```json
{
  "orderId": 1,
  "participantId": 2,
  "amount": 150.00
}
```

---

## Beskeder

### GET /api/messages
**Formål:** Henter beskeder for den indloggede bruger.  
**Controller:** [`MessagesController`](../src/Api.PayBySharePay/Controllers/MessagesController.cs)

---

### POST /api/messages
**Formål:** Opretter/sender en besked.

---

### PUT /api/messages/{id}/read
**Formål:** Markerer en besked som læst.

---

## Merchant Orders (ingen auth)

### GET /api/merchant-orders
**Formål:** Henter ordredata til deltager via token. Bruges af MerchantDemo.  
**Auth:** Ingen (offentlig)  
**Controller:** [`MerchantOrdersController`](../src/Api.PayBySharePay/Controllers/MerchantOrdersController.cs)

**Query params:** `?token=<deltager-token>`

---

### POST /api/merchant-orders/init
**Formål:** Initialiserer en merchant-gruppeordre.

---

### POST /api/merchant-orders/confirm
**Formål:** Deltager bekræfter sin betaling via MerchantDemo.

---

## Venner

### GET /api/friends
**Formål:** Henter venneliste for den indloggede bruger.  
**Controller:** [`FriendsController`](../src/Api.PayBySharePay/Controllers/FriendsController.cs)

---

### POST /api/friends
**Formål:** Tilføjer en ven.

---

## Directory

### GET /api/directory
**Formål:** Søger i brugerdatabasen (bruges til at finde deltagere).  
**Controller:** [`DirectoryController`](../src/Api.PayBySharePay/Controllers/DirectoryController.cs)

**Query params:** `?search=navn`

---

## Se også

- [Backend](04-backend.md)
- [Authentication og security](08-authentication-security.md)
