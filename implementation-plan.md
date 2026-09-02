# Implementation plan — UC-09

## Klassifikation

- Opgavetype: `SECURITY_FIX`
- Use case: `docs/usecases/UC-09-beskyt-dev-endpoints.md`
- Database/migration: ingen
- Dependencies: ingen nye
- Frontend: ingen ændring
- Deployment/secrets: ingen ændring
- Godkendelse: brugerens direkte implementeringsordre dækker det afgrænsede UC-09-scope

## Forståelse

UC-09 skal gøre hele `DevController` utilgængelig i Simply, Production, Local og alle andre miljøer end ASP.NET Core `Development`. I Development skal de eksisterende udviklerruter fungere uændret.

Controlleren indeholder fire actions:

- `DELETE /api/dev/reset`
- `POST /api/dev/seed-merchant-urls`
- `POST /api/dev/simulate-authorized`
- `GET /api/dev/merchant-callbacks/latest`

## Trusselsanalyse

| Punkt | Vurdering |
|---|---|
| Trussel | En ekstern bruger kalder test-/udviklerruter i et deployet miljø |
| Aktiv | Ordrer, betalinger, beskeder, merchant-konfiguration og test-callbackdata |
| Angriberens mulighed | Ruterne er offentligt registreret uden authentication |
| Svaghed | `DevController` opdages og mappes i alle environments |
| Konsekvens | Destruktiv datasletning, test-stateændringer og informationslæk |
| Mitigation | Fjern hele `DevController` fra MVC controller discovery uden for `Development` |
| Residual risiko | Andre anonyme endpoints og generel adminfunktionalitet er uden for UC-09 |

## Design

1. Tilføj en lille `IApplicationFeatureProvider<ControllerFeature>`, der fjerner `DevController` fra controller feature-listen, medmindre environment-navnet er `Development`.
2. Registrér provideren ved `AddControllers()` i `Program.cs`.
3. Bevar `DevController` og alle fire actions uændret.
4. Fordi controlleren ikke opdages i ikke-Development:
   - registreres ingen `/api/dev/*` routes;
   - kald returnerer 404;
   - controllerens database-/serviceafhængigheder kan ikke aktiveres gennem HTTP;
   - Swagger/OpenAPI får ingen dev-actions at beskrive.
5. Ingen JWT-rolle eller skjult admin-password tilføjes.

## Forventede filer

- `src/Api.PayBySharePay/Program.cs`
- ny feature-provider under `src/Api.PayBySharePay/Controllers`
- ny fokuseret testfil under `src/Tests.PayBySharePay`
- `implementation-plan.md`
- `test-plan.md`
- `docs/usecases/UC-09-beskyt-dev-endpoints.md`
- `docs/current-state.md`
- relevante sikkerheds-/arkitekturdokumenter

## API- og kompatibilitetspåvirkning

- Development: ingen ændring.
- Simply/Production/Local/andre miljøer: alle `/api/dev/*` routes fjernes og giver 404.
- Ingen almindelige API-ruter ændres.
- Ingen database-, provider-, frontend- eller kontraktændring.

## Risici og afværgning

- En action kan overses ved individuel route-filtrering; derfor fjernes hele controlleren.
- Et filter kunne stadig annoncere routes i Swagger; controller discovery anvendes, så både routing og OpenAPI udelades.
- Environment-navne kan variere; kun frameworkets præcise `Development`-navn tillades, alt andet er deny-by-default.
