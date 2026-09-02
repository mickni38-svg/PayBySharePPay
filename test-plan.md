# Test plan — UC-09

## Teststrategi

Brug eksisterende xUnit og FluentAssertions. Ingen ny testpakke, EF InMemory, database eller live ekstern integration.

## Controller discovery-tests

Byg en `ApplicationPartManager` med API-assemblyens controllers og den nye environment-provider.

- `Development`: `DevController` findes fortsat i `ControllerFeature`.
- `Simply`: `DevController` findes ikke.
- `Production`: `DevController` findes ikke.
- `Local`: `DevController` findes ikke.
- Ukendt/tomt environment: `DevController` findes ikke.
- En almindelig controller forbliver registreret i alle miljøer, så provideren ikke rammer bredere end UC-09.

## Side-effect og Swagger-verifikation

Når `DevController` ikke findes i MVC controller discovery:

- registreres ingen af controllerens fire attribut-routes;
- controlleren kan ikke instantieres eller kalde database/services via HTTP;
- ApiExplorer/Swagger modtager ingen dev-actions.

Dette verificeres ved controller feature-listen, som både endpoint-routing og ApiExplorer bygger deres controller action discovery på.

## Regression

Kør:

- `dotnet build PayBySharePay.sln --configuration Release`
- `dotnet test PayBySharePay.sln --configuration Release --no-build --verbosity normal`
- GitHub Actions' Angular test/build som samlet regressionskontrol efter push til `main`.

## Exit-kriterier

- DevController findes kun i Development discovery.
- Almindelige controllers forbliver registreret.
- Alle eksisterende og nye tests består.
- Ingen database-, dependency-, frontend- eller betalingsændring er introduceret.
- Dokumentation opdateres først efter grøn verifikation.
