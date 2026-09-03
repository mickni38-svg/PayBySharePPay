# Implementation plan — UC-17

## Klassifikation og approval

- Opgavetype: `NEW_USE_CASE`
- Use case: `docs/usecases/UC-17-offentlig-forside-for-ikke-indlogget-bruger.md`
- Approval: Product Owner godkendte eksplicit beskyttelse af relevante Friends/Messages/Participants/Directory API-endpoints med JWT.
- Database/migration: ingen
- Dependencies: ingen nye
- Deployment: ingen direkte ændring

## Frontend

1. Genbrug `AuthService.isLoggedIn()` som eneste auth-state på forsiden.
2. Vis offentlig velkomstforside i `HomeComponent` når brugeren ikke er logget ind; behold eksisterende dashboard uændret for loggede brugere.
3. Offentlig forside genbruger PayNSync-branding og eksisterende `/login`/`/register` redirects til profil/kontocenter.
4. Undgå private data-kald i `HomeComponent` når session mangler.
5. Tilføj en funktionel auth guard til private routes og behold `/home` samt `/profile` offentlige.
6. Bevar bottom navigation; private links håndteres af guard, som sender udloggede brugere til login-mode i profil.
7. Login/logout skal skifte korrekt UI via eksisterende signals uden reload.

## Backend

1. Tilføj `[Authorize]` til `FriendsController`, `MessagesController` og `DirectoryController`.
2. Tilføj `[Authorize]` til `ParticipantsController`, men bevar merchant-logo GET anonymt med `[AllowAnonymous]`, da logoet er et offentligt asset.
3. Genbrug eksisterende JWT middleware; ingen ny auth-mekanisme.
4. Ingen ændring af betalingsendpoints, database eller DTO-kontrakter ud over at anonyme private kald nu returnerer 401.

## Tests og verification

1. Frontend: dæk offentlig vs. personlig home og guard-adfærd med mocked auth.
2. Backend: eksisterende integration/auth-verifikation skal bekræfte 401 på private controllers.
3. Kør Angular test/build og .NET build/test via GitHub Actions efter ændringen.
4. Review scope, API-autorisation og ingen regression i login/profile.

## Forventede filer

- `src/Frontend.PayBySharePay/src/app/features/home/home.component.html`
- `src/Frontend.PayBySharePay/src/app/app.routes.ts`
- `src/Frontend.PayBySharePay/src/app/core/guards/auth.guard.ts`
- relevante frontend specs
- `FriendsController`, `MessagesController`, `DirectoryController`, `ParticipantsController`
- `docs/current-state.md`, `docs/architecture.md`, UC-17 status efter verification

## Risici

- Legacy kode, der kalder Friends/Messages/Directory/Participants uden JWT, vil nu få 401 og skal bruge det eksisterende interceptor-token.
- Merchant-logo GET bevares anonymt for ikke at bryde offentlig asset-visning.
- Login/profile forbliver offentlige; alle brugerdata-routes beskyttes client-side og server-side.
