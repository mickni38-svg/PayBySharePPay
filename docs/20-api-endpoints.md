# API endpoints – PayBySharePay

Base URL: `https://localhost:{port}`

Swagger UI er tilgængeligt på rod-URL (`/`) når backend kører i Development-mode.

---

## Deltagere

### `GET /api/participants/search`

Søger efter deltagere (personer og merchants) på navn.

**Query parameter:** `query` (string, påkrævet)

**Respons 200 OK:**
```json
[
  {
    "id": 1,
    "name": "Anders Andersen",
    "type": "Person",
    "email": "anders@example.com",
    "phone": "12345678",
    "companyName": null
  },
  {
    "id": 2,
    "name": "Acme ApS",
    "type": "Merchant",
    "email": null,
    "phone": null,
    "companyName": "Acme ApS"
  }
]
```

---

### `POST /api/participants/person`

Opretter en ny person-deltager.

**Request body:**
```json
{
  "name": "Anders Andersen",
  "email": "anders@example.com",
  "phone": "12345678"
}
```

**Valideringsregler:**
- `name` er påkrævet

**Respons 201 Created:** `ParticipantDto`

---

### `POST /api/participants/merchant`

Opretter en ny merchant-deltager.

**Request body:**
```json
{
  "name": "Acme ApS",
  "companyName": "Acme ApS",
  "cvrNumber": "12345678",
  "vatNumber": "DK12345678",
  "contactPerson": "Bente Hansen",
  "contactEmail": "bente@acme.dk",
  "contactPhone": "87654321",
  "companyAddress": "Hovedgaden 1, 1000 København",
  "paymentReference": "REF-001",
  "payoutAccountInfo": "1234-56789012",
  "paymentProvider": "MobilePay"
}
```

**Valideringsregler:**
- `name` er påkrævet
- `companyName` er påkrævet

**Respons 201 Created:** `ParticipantDto`

---

## Venner

### `POST /api/friends`

Opretter en venrelation mellem to deltagere.

**Request body:**
```json
{
  "initiatorId": 1,
  "receiverId": 2
}
```

**Valideringsregler:**
- En deltager kan ikke tilføje sig selv som ven

**Respons 204 No Content**

---

## Ordrer

### `POST /api/orders`

Opretter en ny ordre og tilknytter deltagere.

**Request body:**
```json
{
  "title": "Fælles middag",
  "category": "Restaurantbesøg",
  "message": "Vi spiser kl. 19",
  "participantIds": [1, 2, 3]
}
```

**Valideringsregler:**
- `title` er påkrævet

**Respons 201 Created:** `OrderDto`

---

### `GET /api/orders/{id}/overview`

Henter overblik over en ordre inkl. deltagere, status og betalinger.

**Respons 200 OK:** `OrderOverviewDto`
```json
{
  "id": 1,
  "title": "Fælles middag",
  "category": "Restaurantbesøg",
  "message": "Vi spiser kl. 19",
  "status": "Open",
  "createdAt": "2026-05-05T15:38:49Z",
  "participants": [
    {
      "participantId": 1,
      "name": "Anders Andersen",
      "type": "Person",
      "status": "Pending"
    }
  ],
  "payments": [
    {
      "participantId": 2,
      "amount": 150.00,
      "paidAt": "2026-05-05T16:00:00Z"
    }
  ]
}
```

**Respons 404 Not Found** hvis ordren ikke eksisterer.

---

## Betalinger

### `POST /api/payments`

Registrerer en betaling fra en deltager på en ordre.

**Request body:**
```json
{
  "orderId": 1,
  "participantId": 2,
  "amount": 150.00
}
```

**Valideringsregler:**
- `amount` skal være større end 0
- Ordren og deltageren skal eksistere

**Respons 201 Created:** `PaymentDto`

---

## Beskeder

### `GET /api/messages/order/{orderId}`

Henter alle beskeder knyttet til en ordre.

**Respons 200 OK:**
```json
[
  {
    "id": 1,
    "orderId": 1,
    "participantId": 1,
    "participantName": "Anders Andersen",
    "content": "Jeg har betalt min del",
    "createdAt": "2026-05-05T16:10:00Z"
  }
]
```

---

### `POST /api/messages`

Opretter en ny besked på en ordre.

**Request body:**
```json
{
  "orderId": 1,
  "participantId": 1,
  "content": "Jeg har betalt min del"
}
```

**Valideringsregler:**
- `content` må ikke være tom

**Respons 201 Created:** `MessageDto`

---

## Fejlsvar

Alle fejl returneres som JSON:

```json
{
  "error": "Beskrivelse af fejlen"
}
```

| Statuskode | Årsag |
|-----------|-------|
| 400 | Valideringsfejl eller ugyldigt input |
| 404 | Ressource ikke fundet |
| 409 | Konflikt (f.eks. venrelation eksisterer allerede) |
| 500 | Uventet serverfejl |
