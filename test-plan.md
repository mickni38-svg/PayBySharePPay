# Test plan — UC-15

## Backendtests

Brug eksisterende xUnit, FluentAssertions og Moq. Ingen database, EF InMemory eller live eksterne kald.

- Person med korrekt password kan logge ind og får type Person.
- Merchant med korrekt password kan logge ind og får type Merchant.
- Forkert password og manglende password giver 401.
- Manglende password kan ikke omgå et eksisterende hash.
- Passwordløs Person uden hash kan kun logge ind i Development.
- Passwordløs login afvises i Simply/Production og for Merchant.
- Merchantregistrering kræver email, password og MSN.
- Merchantregistrering sender password til hashinglaget og returnerer Merchant-type.
- Email-unikhed kontrolleres på tværs af Person og Merchant.
- Person-/Google-svar indeholder korrekt participant-type.
- PasswordHash returneres aldrig i auth response.

## Frontendtests

Mock alle services og HTTP-grænser.

- Query mode vælger login/register.
- Konto-fanen viser Min profil kun ved session.
- Opret skifter mellem Bruger og Merchant.
- Personvalidering og payload.
- Merchantvalidering for email, passwordbekræftelse og MSN samt korrekt payload.
- Login navigerer Person til home og Merchant til profile.
- Session gemmer participant-type under eksisterende key.
- Vipps-fanen er kun tilgængelig for autentificeret Person og loader data lazy.
- Udvikler-fanen findes kun ved `environment.production === false` og loader directory lazy.
- Production sender ingen dev-kald.
- Profil, mapping og developer feedback er adskilt.
- Fanerne har centrale ARIA-attributter og keyboard navigation.
- Login/register routes lander på korrekt profile mode.

## Verification

- `dotnet build PayBySharePay.sln --configuration Release`
- `dotnet test PayBySharePay.sln --configuration Release --no-build --verbosity normal`
- Angular tests
- Angular Simply-build
- review af auth, public kontrakt, UC-09-adskillelse, scope og dokumentation

## Exit

- Alle nye og eksisterende tests grønne.
- Ingen migration eller dependency.
- Ingen live Google/Vipps/MobilePay-kald.
- UC-15 markeres først implementeret efter grøn GitHub Actions.
