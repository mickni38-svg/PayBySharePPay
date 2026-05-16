# API og integrationer

## Base URL

| Miljø | URL |
|-------|-----|
| Lokalt | `http://localhost:5265` (se `launchSettings.json`) |
| Produktion | Azure App Service (URL konfigureres i `environment.prod.ts`) |

## Authentication

Alle endpoints undtagen `/api/auth/login` og `/api/auth/register-*` kræver:
```
Authorization: Bearer <JWT-token>
```

JWT udstedes af `JwtTokenService`. Token-levetid: 480 minutter (8 timer) for dev-login, 43200 minutter konfigureret i `appsettings.json`.

---

## Endpoints

### Auth (`/api/auth`)

| Method | Path | Beskrivelse | Auth krævet |
|--------|------|-------------|-------------|
| POST | `/api/auth/login` | Log ind med email, få JWT | Nej |
| POST | `/api/auth/register-person` | Opret ny person | Nej |
| POST | `/api/auth/register-merchant` | Opret ny merchant | Nej |

**Login request:**
```json
{ "email": "test1@dev.dk" }
```
**Login response:**
```json
{ "token": "...", "participantId": 1, "name": "Anders Nielsen", "expiresAt": "..." }
```

---

### Ordrer (`/api/orders`)

| Method | Path | Beskrivelse |
|--------|------|-------------|
| GET | `/api/orders` | Hent alle ordrer |
| GET | `/api/orders?participantId=X` | Hent ordrer for deltager |
| POST | `/api/orders` | Opret ny ordre |
| GET | `/api/orders/{id}/overview` | Fuld ordredetalje |

---

### Betalinger (`/api/payments`)

| Method | Path | Beskrivelse |
|--------|------|-------------|
| POST | `/api/payments` | Registrér betaling |

**Request:**
```json
{ "orderId": 1, "participantId": 2, "amount": 150.00 }
```

---

### Merchant-ordrer (`/api/merchant-orders`)

| Method | Path | Beskrivelse |
|--------|------|-------------|
| POST | `/api/merchant-orders/init` | Opret merchant-draft |
| GET | `/api/merchant-orders/{orderId}` | Hent draft for ordre |
| PUT | `/api/merchant-orders/{orderId}/complete` | Marker draft som klar |

---

### Deltagere (`/api/participants`)

| Method | Path | Beskrivelse |
|--------|------|-------------|
| GET | `/api/participants/{id}` | Hent deltager |
| GET | `/api/participants/search?q=X` | Søg deltagere |

---

### Telefonbog (`/api/directory`)

| Method | Path | Beskrivelse |
|--------|------|-------------|
| GET | `/api/directory/search?q=X` | Søg i telefonbog |

---

### Venner (`/api/friends`)

| Method | Path | Beskrivelse |
|--------|------|-------------|
| GET | `/api/friends/{participantId}` | Hent venneliste |
| POST | `/api/friends` | Tilføj ven |

---

### Beskeder (`/api/messages`)

| Method | Path | Beskrivelse |
|--------|------|-------------|
| GET | `/api/messages?orderId=X` | Hent beskeder for ordre |
| POST | `/api/messages` | Send besked |

---

## Frontend API-kald

HTTP-interceptoren (`api.interceptor.ts`) tilføjer automatisk `Authorization: Bearer <token>` på alle requests og sætter base URL fra `environment.apiUrl`.

| Angular Service | Kalder |
|-----------------|--------|
| `AuthService` | `/api/auth/*` |
| `OrderService` | `/api/orders/*` |
| `PaymentService` | `/api/payments` |
| `ParticipantService` | `/api/participants/*` |
| `DirectoryService` | `/api/directory/*` |
| `MessageService` | `/api/messages/*` |
| `FriendService` | `/api/friends/*` |
| `ActivityService` | Kalder **ikke** API direkte – bygger feed fra `OrderService` |

---

## Fejlhåndtering (API)

`ExceptionHandlingMiddleware` i `Api/Middleware/` fanger alle uhåndterede exceptions og returnerer:
```json
{ "error": "Fejlbesked" }
```
med passende HTTP-statuskode.

---

## Konfiguration

### `appsettings.json` (lokal dev)
- Connection string: lokal SQL Server Express
- JWT-nøgle: placeholder `SBYS-DEV-SECRET-REPLACE-IN-PRODUCTION-MIN-32-CHARS`

### `appsettings.Production.json` (prod)
- Connection string: Azure SQL med `Active Directory Default` auth
- JWT-nøgle og Issuer/Audience: **skal konfigureres som miljøvariable eller i Azure App Configuration** – de er ikke i filen

### Swagger
Swagger UI er tilgængelig på `/swagger` i development-miljø.

---

## Integrationer der mangler

- Ingen email-integration (påmindelser sendes ikke som emails)
- Ingen push-notifikationer
- Ingen betalingsgateway (fx MobilePay, Stripe)
- `JoinToken` på ordrer er genereret men der er intet API-endpoint til at acceptere invitationer via token
