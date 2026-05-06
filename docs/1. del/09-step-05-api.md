# Step 05 – API

## Formål
At eksponere use cases gennem Web API.

## Krav der skal implementeres i dette step
- Opret controllers for:
  - participants
  - friends
  - orders
  - payments
  - messages
- Understøt endpoints til:
  - søg deltagere
  - opret person
  - opret merchant
  - opret venrelation
  - opret ordre
  - hent ordreoverblik
  - registrer betaling
  - hent/opret beskeder
- Opret request/response DTO'er
- Konfigurér dependency injection
- Konfigurér Swagger

## Vigtige regler
- ingen tung forretningslogik i controllers
- services skal bruges fra Service.PayBySharePay
