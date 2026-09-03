# Test plan — UC-17

## Frontendtests

Mock alle services og HTTP-grænser.

- Udlogget `/home` viser PayNSync velkomst, Log ind og Opret bruger.
- Udlogget `/home` viser ikke merchant-carousel, Overblik, Beskeder eller andre private dashboarddata.
- Logget bruger ser eksisterende dashboard uændret.
- `HomeComponent` laver ingen private data-kald uden session.
- Auth guard tillader private routes for logget bruger.
- Auth guard sender udlogget bruger til `/profile?mode=login`.
- `/home` og `/profile` forbliver offentlige.
- Login/logout reagerer på eksisterende `AuthService` signals uden browser reload.

## Backendtests

Brug eksisterende auth-/controller-testmønstre. Ingen database, EF InMemory eller live eksterne kald.

- Friends kræver autentificeret JWT.
- Messages kræver autentificeret JWT.
- Directory kræver autentificeret JWT.
- Private Participants-operationer kræver autentificeret JWT.
- `GET /api/participants/{id}/logo` forbliver anonymt tilgængeligt som public asset.
- Auth/register endpoints påvirkes ikke.

## Verification

- `dotnet build PayBySharePay.sln --configuration Release`
- `dotnet test PayBySharePay.sln --configuration Release --no-build --verbosity normal`
- Angular tests
- Angular Simply-build
- review af routebeskyttelse, login/profile-flow og API-anonymitet

## Exit

- Alle nye og eksisterende relevante tests grønne.
- Ingen migration eller dependency.
- Ingen live Google/Vipps/MobilePay-kald.
- UC-17 markeres først implementeret efter grøn verification.
