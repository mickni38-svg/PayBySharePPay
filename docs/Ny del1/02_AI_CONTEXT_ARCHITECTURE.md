# SBYS AI Context — Architecture

## Arkitekturprincip
SBYS skal bygges som en **API-first platform**.

Det betyder:
- Backend/API er source of truth
- Web frontend/mobile view bruger API
- Senere mobilapp kan bruge samme API
- Merchant-integration bruger samme API

## Frontend-strategi
Nuværende videreudvikling skal tage udgangspunkt i:

- web frontend
- mobilvisning/mobile-first
- responsive layout
- simpel MVP UI

Frontend må gerne ligne en mobilapp, men skal kunne køre som web.

## Backend
Backend bør være ASP.NET Core API.

Typiske lag:
- API controllers
- Contracts/DTOs
- Logic/services
- Data/entities
- EF Core DbContext
- Migrations
- Seed data

## Database
Domain models skal mappes til tabeller via EF Core.

Vigtige domæner:
- Users
- Friends/friend requests
- Merchants
- MerchantIntegration
- Directory/connections
- GroupPayments
- GroupPaymentMembers
- Merchant order draft
- Participant order/payment status
- Invitation/join tokens

## Integration
Merchant integration skal være Model A:

> Merchant håndterer betaling. SBYS håndterer orchestration og status.

Merchant sender ordredata og betalingsstatus til SBYS via API/webhooks.

SBYS sender status tilbage til merchant, når ordre kan frigives.
