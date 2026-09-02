# Implementation plan — UC-15

## Klassifikation og approval

- Opgavetype: `NEW_USE_CASE`
- Use case: `docs/usecases/UC-15-profil-og-kontocenter.md`
- Modelprofil: GPT-5.6 Sol, Medium
- Approval: Product Owner godkendte implementering direkte på `main`
- Database/migration: ingen forventet
- Dependencies: ingen nye
- Deployment: ingen

## Nuværende gaps

- Profil, tema, mapping og development-værktøjer ligger i én lang formular.
- Merchant-frontend sender ikke det påkrævede MSN.
- Merchantregistrering gemmer ikke konto-email eller password-hash.
- Login finder kun Person.
- En tom password-request omgår den nuværende password-verifikation.
- Development-panelet vises i production-frontend, selv om UC-09 har fjernet backend-ruterne.

## Backend

1. Udvid `RegisterMerchantRequest` og `CreateMerchantDto` med required konto-email og password.
2. Gem merchant-email i `Participant.Email` og hash password med samme BCrypt-mønster som Person.
3. Gør repository email-lookup typeuafhængigt.
4. Gør login fælles for Person og Merchant.
5. Kræv password ved normalt login.
6. Tillad kun passwordløst login for passwordløse Person-seedkonti i ASP.NET Core Development.
7. Udvid `LoginResponse` med `ParticipantType` og returnér typen fra login, registrering og Google-login.
8. Bevar generiske 401-fejl og eksponér aldrig password-hash.
9. Ingen entity- eller migrationsændring.

## Frontend

1. Udvid AuthService-sessionen med participant-type under de eksisterende localStorage-nøgler.
2. Udvid merchant-requesten med email, password og Vipps MSN.
3. Gør `/profile` til samlet kontocenter med hovedfanerne Konto, Vipps-test og Development-only Udvikler.
4. Konto indeholder modes Min profil, Log ind og Opret konto; oprettelse skifter mellem Bruger og Merchant.
5. Genbrug eksisterende profil-, theme-, auth-, directory-, Vipps- og dev-services.
6. Hent directory/Vipps-data først, når den relevante fane åbnes.
7. Skjul Vipps-test for ikke-Person/ikke-logget ind og Udvikler helt i production.
8. Bevar gamle `/login` og `/register` links som redirects til query-parametre på `/profile`.
9. Bevar mobil-first, temaer, touch targets, bottom-nav-frirum og tilgængelige tab-semantikker.
10. Person går til home efter login/oprettelse; Merchant går til Min profil.

## Forventede filer

- API auth controller og auth DTO'er
- participant DTO/service/repository
- Angular AuthService, routes og profile component/template/styles/tests
- eventuelt login/register komponenter kun hvis kompatibilitetsredirect kræver det
- fokuserede backend auth-tests
- berørte dokumenter efter grøn verification

## Risici

- Eksisterende passwordløse production-data må ikke kunne logge ind uden password.
- Email-lookup på tværs af typer kan afdække historiske dubletter; nye registreringer skal afvise dem deterministisk.
- Gamle localStorage-sessioner mangler participant-type; UI skal kunne falde tilbage til indlæst profiltype.
- UC-09 må ikke svækkes: dev-fanen er frontend-skjult, og backend forbliver route-fraværende uden for Development.
- Merchant-dashboard og merchant-masterdataredigering er uden for scope.
