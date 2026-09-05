# Test Plan — UC-21 Merchant-adapter og ordre-API

## Automated
- Adapter mapper reference, produkter, priser, modifiers, host og levering korrekt.
- Adapter bruger minor units og samme currency.
- Merchant finalization bevarer strukturerede modifiers fra merchant draft.
- Eksternt ordrenummer og merchant response gemmes.
- Et allerede gemt eksternt ordrenummer må ikke overskrives af en ny levering.
- Simuleret merchant API returnerer samme eksterne ordre for samme idempotency key.
- Eksisterende payment/capture tests skal fortsat bestå.

## Build
- `dotnet build PayBySharePay.sln`
- `dotnet test src/Tests.PayBySharePay/Tests.PayBySharePay.csproj`

## Regression boundaries
- Ingen ændring i Vipps reserve/capture-semantik.
- Ingen participant identity eller provider payment references i merchant payload.
- `GroupOrderUrl` bruges fortsat til deltagerens menu-link.
- Final merchant delivery sker først efter full capture.
