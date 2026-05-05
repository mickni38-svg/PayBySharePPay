# Arkitektur og lag – PayBySharePay

## Lagdeling

### Frontend.PayBySharePay
Ansvar:
- Angular UI
- routing
- HTTP-kald til backend
- visning af data
- responsive layouts

Må ikke:
- indeholde domænelogik fra backend
- tale direkte med databasen

### Api.PayBySharePay
Ansvar:
- HTTP endpoints
- request/response DTO'er
- inputvalidering
- dependency injection og app setup

Må ikke:
- indeholde tung forretningslogik

### Service.PayBySharePay
Ansvar:
- forretningsregler
- use cases
- koordinering mellem repositories
- mapping mellem entiteter og DTO'er hvis relevant

### DataStorage.PayBySharePay
Ansvar:
- EF Core entiteter
- DbContext
- repositories
- queries
- persistence

## Anbefalet afhængighedsretning

- Frontend -> Api via HTTP
- Api -> Service
- Service -> DataStorage

Frontend må ikke referere direkte til backend-projekter.

## Domænemodel

Brug en fælles deltager-model som understøtter både person og merchant.
Anbefalet første version:

- `Participant`
- `ParticipantType` enum (`Person`, `Merchant`)

`OrderParticipant` skal pege på `Participant`.

## Designprincip

Start simpelt og robust.
Copilot skal ikke introducere unødvendig kompleks inheritance i første version, hvis en fladere model er mere stabil.

## Kvalitetskrav

- interfaces i service- og repository-lag
- async/await på I/O
- tydelig navngivning
- DTO'er mellem API og service hvor det giver mening
- separate mapper per ansvar
