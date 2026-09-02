# Test plan — UC-08

## Teststrategi

Brug eksisterende xUnit, FluentAssertions og Moq. Ingen ny testpakke, database, EF InMemory eller live Vipps/MobilePay-kald.

## Controller-tests

Tilføj direkte tests af `OrdersController` med en `DefaultHttpContext` og claims-principal.

### Claim-identitet

- `ApproveOrder` sender JWT participant-ID til orchestration, selv når body indeholder et andet ID.
- `CancelOrder` sender JWT participant-ID til orchestration.
- `CompleteOrder` sender JWT participant-ID til order service.
- `PayOrder` sender JWT participant-ID til complete-service og ignorerer body-ID.
- Dæk både `ClaimTypes.NameIdentifier` og fallback til `sub`.

### Manglende/ugyldig identitet

- Manglende claim giver 401 og ingen service/provider-kald.
- Ikke-numerisk, nul eller negativ claim giver 401 og ingen service/provider-kald.
- Reflektions-/metadata-test bekræfter, at `OrdersController` fortsat har `[Authorize]`.

### Manipulation og payment side effect

- Ikke-host JWT kombineret med host-ID i body kan ikke kalde approve/cancel/complete med host-ID.
- På legacy `/pay` afvises ikke-host før `IExternalPaymentService.ChargeAsync`.
- Gyldig host kan fortsat gennemføre det eksisterende flow.

## Middleware-tests

- `UnauthorizedAccessException` returnerer 403.
- Response indeholder en generisk fejl og ikke exceptionens interne besked.
- Nærliggende mappings for 400, 404 og 409 forbliver uændrede eller dækkes af eksisterende tests.
- Uventet exception forbliver 500 med den eksisterende generiske fejl; UC-08 ændrer ikke øvrig exception policy.

## Eksisterende tests

Kør mindst:

- `dotnet build PayBySharePay.sln --configuration Release`
- `dotnet test PayBySharePay.sln --configuration Release --no-build --verbosity normal`
- relevante eksisterende `GroupPaymentOrchestrationServiceTests` for host, capture, cancel og idempotens

Frontendkode ændres ikke. GitHub Actions kører fortsat Angular test/build som samlet regressionskontrol efter push.

## Manuel/API-kontrol

Med testtokens for host A og bruger B:

1. A + vilkårligt body-ID på A's ordre lykkes.
2. B + A's ID i body på A's ordre giver 403.
3. Intet token giver 401.
4. Fejlen indeholder ikke stack trace, exception-type eller intern exceptiontekst.
5. Ikke-host `/pay` udløser intet betalingskald.

## Exit-kriterier

- Alle nye og eksisterende .NET-tests består.
- Ingen provider-kald sker ved 401/403.
- Ingen database-, payment state- eller frontendændring er introduceret.
- Diffen indeholder kun UC-08-relaterede filer og nødvendige dokumentationsrettelser.
