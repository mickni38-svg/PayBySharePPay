# Implementation plan — UC-08

## Klassifikation

- Opgavetype: `SECURITY_FIX`
- Use case: `docs/usecases/UC-08-jwt-identitet-og-host-autorisation.md`
- Database/migration: ingen
- Dependencies: ingen nye
- Deployment/secrets: ingen ændring
- Approval gate: authentication, authorization og public API-adfærd

## Forståelse

UC-08 skal lukke muligheden for at opnå host-rettigheder ved at sende en anden brugers `requestingParticipantId` i request-body. Den validerede JWT-identitet skal være eneste autoritative brugeridentitet for eksisterende host-handlinger.

## Trusselsanalyse

| Punkt | Vurdering |
|---|---|
| Trussel | En autentificeret bruger sender hostens participant-ID i body |
| Aktiv | Gruppeordrer, reservationer, capture/cancel og legacy betaling |
| Angriberens mulighed | Kan ændre JSON-body og kalde beskyttede endpoints med eget gyldigt JWT |
| Svaghed | Controller sender klientens ID til service-lagets ellers korrekte host-tjek |
| Konsekvens | Uautoriseret capture, cancel eller complete; legacy `/pay` kan kalde betalingsservice før host-tjek |
| Mitigation | Udled participant-ID fra JWT og brug det i alle eksisterende host-only actions |
| Residual risiko | `CreateOrder.CreatedByParticipantId`, reserve-participant-ID og read-endpoints er separate identitets-/adgangsproblemer og er uden for UC-08 |

## Kortlagte host-endpoints

- `POST /api/orders/{id}/approve`
- `POST /api/orders/{id}/cancel`
- `POST /api/orders/{id}/complete`
- `POST /api/orders/{id}/pay` (legacy)

`/reserve` er en deltagerhandling og ændres ikke. Oprettelse og read-endpoints ændres heller ikke i UC-08.

## Valgt kompatibilitetsstrategi

Anbefaling: Bevar de nuværende request-DTO'er og body-format i UC-08, men ignorér `RequestingParticipantId` fuldstændigt ved autorisation. Det lukker sårbarheden uden samtidig at bryde den eksisterende Angular-klient eller andre klienter.

Felterne dokumenteres som deprecated/sikkerhedsmæssigt irrelevante. Fysisk fjernelse af felterne og tomme request-bodies kan ske som en separat kontraktoprydning.

Dette valg kræver Product Owner-godkendelse før kodeændring.

## Implementering

1. Tilføj en lille privat helper i `OrdersController`, der:
   - kræver authenticated principal;
   - læser `ClaimTypes.NameIdentifier` og har fallback til JWT `sub`;
   - parser et positivt integer participant-ID;
   - giver 401 uden service/provider-kald ved manglende eller ugyldig claim.
2. Brug claim-ID'et i `ApproveOrder`, `CancelOrder`, `CompleteOrder` og `PayOrder`; body-ID ignoreres.
3. Bevar service-metodernes `requestingParticipantId` parameter. Den bliver nu trusted server input fra controlleren, så eksisterende domænevalidering og tests genbruges.
4. På legacy `PayOrder` sammenlignes claim-ID med `overview.CreatedByParticipantId` før `IExternalPaymentService.ChargeAsync`, så ikke-host aldrig kan udløse provider-kald. Service-lagets efterfølgende host-tjek bevares som defense in depth.
5. Tilføj en specifik `UnauthorizedAccessException`-mapping i `ExceptionHandlingMiddleware` før den generiske handler:
   - HTTP 403;
   - generisk, ikke-intern fejltekst;
   - warning-log uden JWT eller følsomme data.
6. Bevar `[Authorize]` på `OrdersController`; manglende/ugyldigt token håndteres fortsat af JwtBearer som 401.
7. Opdatér XML-kommentarer/DTO-kommentarer, så body-ID ikke beskrives som autoritativ identitet.
8. Tilføj API-projektreference til det eksisterende testprojekt (ingen NuGet-pakke) for direkte controller- og middlewaretests.
9. Efter bestået build/tests opdateres UC-08, `docs/current-state.md`, `docs/architecture.md`, `docs/business-rules.md`, `docs/flows.md` og `docs/glossary.md` kun dér, hvor de beskriver body-ID/500-adfærden.

## Forventede filer

- `src/Api.PayBySharePay/Controllers/OrdersController.cs`
- `src/Api.PayBySharePay/Middleware/ExceptionHandlingMiddleware.cs`
- `src/Api.PayBySharePay/DTOs/ApproveOrderRequest.cs`
- `src/Api.PayBySharePay/DTOs/CancelOrderRequest.cs`
- `src/Api.PayBySharePay/DTOs/CompleteOrderRequest.cs`
- `src/Api.PayBySharePay/DTOs/PayOrderRequest.cs`
- `src/Tests.PayBySharePay/Tests.PayBySharePay.csproj`
- ny fokuseret controller-/middleware-testfil under `src/Tests.PayBySharePay`
- `docs/usecases/UC-08-jwt-identitet-og-host-autorisation.md`
- berørte status-/arkitekturdokumenter
- `implementation-plan.md`
- `test-plan.md`

## API- og kompatibilitetspåvirkning

- Endpoint-URL'er og response-success-kontrakter ændres ikke.
- Body-feltet accepteres fortsat, men bestemmer ikke længere identiteten.
- Manipulerede body-ID'er, som tidligere kunne give adgang, resulterer nu i 403 ud fra JWT-ejerskab.
- `UnauthorizedAccessException` ændres fra 500 til 403.
- Ingen frontendændring er nødvendig for sikkerhedsrettelsen.

## Risici

- Claim mapping kan variere mellem `sub` og `ClaimTypes.NameIdentifier`; helperen understøtter begge og testes.
- Legacy `/pay` har et eksternt kald før det nuværende service-host-tjek; derfor kræves eksplicit pre-check.
- Middlewareændringen påvirker alle `UnauthorizedAccessException` globalt, men retter den dokumenterede 500-fejl til den semantisk korrekte 403.
