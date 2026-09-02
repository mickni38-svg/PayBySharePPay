# UC-15: Profil- og kontocenter med rollebaserede faner

## Status

✅ Implementeret på `main` den 2. september 2026. Verificeret med backend-build/tests, 42 Angular-tests og Simply-produktionsbuild i GitHub Actions Build & Test #205.

## Implementeringsprofil

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- **Opgavetype:** `NEW_USE_CASE`
- **Størrelse:** Mellem frontend/backend vertical slice uden forventet databaseændring
- **Approval gate:** Ændrer authentication, merchant-login og public API-kontrakt

## Mål

PayNSync skal have ét samlet, overskueligt profil- og kontocenter, hvor en bruger kan redigere sin profil, logge ind, oprette en personkonto eller merchantkonto, administrere Vipps-testmapping og — kun i Development — bruge udviklerværktøjer.

Løsningen skal erstatte den nuværende lange profilside med en tydelig faneopdeling, genbruge eksisterende funktionalitet og gøre merchant-registrering og efterfølgende merchant-login reelt anvendeligt.

## Nuværende situation og dokumenteret gap

Den nuværende profilside blander profilfelter, notifikationer, Vipps-testmapping, tema, udviklerlogin, destruktiv reset og logout i én lang formular.

Kodegennemgangen viser desuden:

- `/login`, `/register` og `/profile` er tre separate lazy-loaded routes med forskellig visuel identitet.
- Profilformularen har én generel **Gem ændringer**-knap, mens Vipps-mapping har sin egen gem-knap; det gør ejerskab af ændringer uklart.
- Directory- og Vipps-data hentes straks ved sideindlæsning, også når brugeren ikke anvender funktionerne.
- Udviklerpanelet ligger nederst på siden og kan ende tæt på eller bag den faste bottom navigation.
- Login- og registreringssiderne bruger teksten `PayBySharePay` og et emoji-logo, mens resten af den offentlige UI bruger PayNSync-brandet.
- Personregistrering sender alle krævede backendfelter og logger brugeren ind efter oprettelse.
- Merchant-registrering i frontend sender ikke det backend-påkrævede `VippsMerchantSerialNumber`; flowet kan derfor ikke gennemføres som vist.
- Merchant-requesten har ikke konto-email eller password, og merchant-data gemmes uden `Participant.Email` og `PasswordHash`.
- Det fælles login søger eksplicit kun efter `ParticipantType.Person`; en merchant kan derfor ikke logge ind igen efter logout.
- Efter UC-09 findes `/api/dev/*` kun i ASP.NET Core `Development`, men det nuværende frontendpanel er ikke environment-skjult.

## UX/UI-analyse

### Problemer i det nuværende layout

1. **For mange forskellige opgaver i samme scroll-flow.** Profilredigering, sandbox-opsætning og destruktive udviklerhandlinger har forskellig risiko og bruges med forskellig frekvens.
2. **Svag informationshierarki.** Siden mangler en tydelig titel, identitetsopsummering og sektionsnavigation. Mange ens separatorer gør det svært at se, hvad der hører sammen.
3. **Utydelige gemmegrænser.** Brugeren kan ikke umiddelbart se, om tema, notifikationer og mapping gemmes automatisk eller af den nederste knap.
4. **Meget lang mobilside.** De sjældent anvendte test- og udviklerfunktioner skubber primære profilhandlinger langt ned.
5. **Rolleforvirring.** Person og merchant er samme domænetype (`Participant`) men har forskellige data og formål; formularerne forklarer ikke forskellen.
6. **Miljøforvirring.** Vipps sandbox og udviklerværktøjer ligner almindelige produktionsfunktioner.
7. **Inkonsekvent branding.** PayNSync-logo, navn, spacing og kortdesign er ikke ens på profil, login og registrering.

### Valgt informationsarkitektur

Brug tre hovedfaner, så mobilnavigationen ikke overfyldes:

| Hovedfane | Indhold | Synlighed |
|---|---|---|
| **Konto** | Min profil, login og opret konto | Altid; indhold afhænger af session |
| **Vipps-test** | Sandbox-testperson og mapping-status | Kun for autentificeret Person, når sandbox/testfunktion er aktiveret |
| **Udvikler** | Testbruger-login og destruktiv reset | Kun når frontend kører i Development |

Inde i **Konto** bruges et sekundært segment:

- **Min profil** — vises kun ved aktiv session.
- **Log ind** — fælles login for Person og Merchant.
- **Opret konto** — har et sekundært valg mellem **Bruger** og **Merchant**.

Dette er bevidst valgt frem for fem lige store hovedfaner. Det holder navigationen stabil på små skærme og samler beslægtede kontohandlinger.

## Brugerhistorier

### Person

Som person vil jeg kunne logge ind, oprette en konto og redigere mine grundoplysninger fra samme kontocenter, så jeg ikke skal forstå flere separate sider.

### Merchant

Som merchant vil jeg kunne oprette en konto med de nødvendige virksomheds- og Vipps-oplysninger og senere logge ind igen med email og password.

### Tester

Som sandbox-tester vil jeg kunne se og ændre min Vipps-testperson i en separat fane, så testopsætning ikke blandes med min almindelige profil.

### Udvikler

Som lokal udvikler vil jeg kunne skifte til en seedet testbruger og nulstille testdata fra en tydeligt markeret udviklerfane, som ikke findes i deployede miljøer.

## Funktionelt scope

### 1. Konto — Min profil

- Vis et kompakt identitetskort med navn, kontotype og email.
- Person kan redigere navn, email og telefon via den eksisterende profilservice.
- Notifikationer og tema vises som separate kort med tydelig tekst om, at de gemmes lokalt/øjeblikkeligt.
- Kun profilfelterne styres af knappen **Gem profil**.
- Vis success, fejl og loading tæt på den handling, de tilhører.
- **Log ud** placeres nederst i Konto-fanen som en tydelig sekundær/destruktiv handling.
- Merchant vises med kontotype og firmanavn. Fuld redigering af CVR, credentials, logo og integration er ikke en del af UC-15.

### 2. Konto — Log ind

- Email og password bruges til både Person og Merchant.
- Google-login mærkes tydeligt som oprettelse/login for en personlig brugerkonto, ikke merchant.
- Ved succes gemmes token, participant-ID, navn og participant-type i eksisterende session storage.
- Person navigeres til `/home`.
- Merchant navigeres tilbage til `/profile` i **Min profil**, da et særskilt merchant-dashboard ikke findes i denne use case.
- Hvis en anden konto allerede er aktiv, erstattes sessionen først efter vellykket login.
- Fejl skelner mellem manglende konto/forkert password og teknisk fejl uden at afsløre, om en vilkårlig email findes.

### 3. Konto — Opret bruger

- Genbrug eksisterende felter: fulde navn, email, valgfri telefon, password og gentag password.
- Password er mindst 6 tegn, og de to passwordfelter skal matche.
- Email trimmes og valideres.
- Ved succes oprettes sessionen automatisk og brugeren navigeres til `/home`.

### 4. Konto — Opret merchant

Følgende felter er påkrævede:

- Spisestedets viste navn
- Firmanavn
- Konto-email
- Password og gentag password
- Vipps Merchant Serial Number (MSN), med kort hjælpetekst om hvor det findes

Følgende felter er valgfrie:

- CVR-nummer
- Adresse
- Kontaktperson
- Kontakt-email
- Kontakttelefon

Backend-kontrakten udvides, så merchant-requesten indeholder konto-email og password. De gemmes i de eksisterende `Participant.Email` og `Participant.PasswordHash`-felter. Der forventes derfor ingen EF migration.

Ved succes logges merchant automatisk ind og vises **Min profil** med en bekræftelse. Merchantens konto-email og kontakt-email er to forskellige felter; kontakt-email må defaultes visuelt til konto-email, men ændres ikke skjult.

### 5. Fælles login for Person og Merchant

- Login-opslaget må ikke filtrere til `ParticipantType.Person`.
- Email skal være unik på tværs af Person og Merchant.
- LoginResponse udvides med `participantType` (`Person` eller `Merchant`).
- Password er påkrævet ved almindeligt login. En request uden password må ikke kunne omgå et eksisterende password-hash.
- Legacy passwordløst testlogin må kun fungere i ASP.NET Core `Development` og kun for eksisterende seed-personer uden password-hash; det må aldrig fungere i Simply/Production eller for merchants.

### 6. Vipps-test-fanen

- Fanen viser nuværende mapping, forklarer at den kun gælder Vipps sandbox og viser om en testperson er optaget.
- Data hentes først, når fanen åbnes.
- Kun autentificerede Person-konti kan gemme mapping.
- Gem-knappen tilhører kun mapping og har egne loading-, success- og fejltilstande.
- Fanen skjules eller deaktiveres med en klar forklaring, hvis sandbox/testfunktionen ikke er aktiv.

### 7. Udvikler-fanen

- Fanen renderes kun når `environment.production === false` og backend forventes at køre som `Development`.
- Directory-data hentes først, når fanen åbnes.
- Testbruger-login kan skifte fra den aktuelle session til en seedet Person.
- Reset kræver en eksplicit bekræftelse, viser præcist hvilke data der slettes og har egen fejl/successtatus.
- Ingen hemmeligt adminpassword, rolle eller produktionsgenvej tilføjes.
- Ved 404 fra dev-API vises en miljøforklaring; der forsøges ikke fallback til andre endpoints.

## Navigation og kompatibilitet

- `/profile` bliver det kanoniske kontocenter og kan åbnes uden aktiv session.
- `/login` og `/register` bevares som kompatible indgange og viderestiller til henholdsvis `/profile?mode=login` og `/profile?mode=register`.
- Query-parametret må deep-linke til en Konto-mode, men må ikke aktivere Vipps-test eller Udvikler, hvis adgangskravet ikke er opfyldt.
- Bottom navigation og PayNSync-header må ikke dække faner, formularhandlinger eller feedback.
- Formularstate bevares ved skift mellem Kontoens login/opret-segmenter, indtil handlingen lykkes eller brugeren forlader siden.

## UI-krav

- Genbrug PayNSync-logoet fra app-shell; ingen ekstra emoji-logo eller `PayBySharePay`-tekst.
- Tilføj sidetitel **Profil og konto** samt kort forklaring under titlen.
- Hver hovedfane består af tydelige cards med egen overskrift, beskrivelse, felter og handling.
- Primær handling må kun være grøn, når den er gyldig og aktiv.
- Destruktive handlinger bruger danger-styling og må ikke stå ved siden af almindelig gem.
- Fanebjælken er sticky under headeren, har minimum 44×44 px touch targets og viser aktiv fane med både farve og form/indikator.
- På mobil bruges én kolonne uden horisontal scrolling.
- På tablet må formularen fortsat være centreret, men indhold grupperes i kort, så den nuværende lange ubrudte kolonne undgås.
- Der skal være tilstrækkelig kontrast i alle fire eksisterende temaer.

## Tilgængelighed

- Brug semantisk `tablist`, `tab`, `tabpanel`, `aria-selected` og korrekt tastaturnavigation.
- Fokus flyttes til paneloverskriften ved programmatisk faneskift.
- Fejl kobles til relevante felter med `aria-describedby` og annonceres i live region.
- Password-visning har tydelig label og bevarer fokus.
- Loading må ikke kun kommunikeres med farve.
- Validering vises både ved feltet og i en kort opsummering ved submit.

## Acceptkriterier

### AC1 — Overskueligt kontocenter

**Givet** at en bruger åbner `/profile`  
**Når** siden vises  
**Så** ses højst tre adgangsberettigede hovedfaner, og kun den aktive fanes indhold er i dokumentflowet.

### AC2 — Personregistrering

**Givet** gyldige personoplysninger og matchende password  
**Når** Brugerkonto oprettes  
**Så** oprettes og gemmes sessionen, og brugeren navigeres til `/home`.

### AC3 — Merchantregistrering

**Givet** gyldigt firmanavn, vist navn, unik konto-email, password og Vipps MSN  
**Når** Merchantkonto oprettes  
**Så** gemmes email og hashed password på merchant-participanten, sessionen oprettes, og merchantens profil vises.

### AC4 — Manglende merchantdata

**Givet** at konto-email, password eller Vipps MSN mangler  
**Når** formularen valideres  
**Så** sendes ingen request, og feltet får en konkret fejltekst.

### AC5 — Fælles login

**Givet** en eksisterende Person eller Merchant med password  
**Når** korrekt email og password indsendes  
**Så** returnerer API’et JWT og participant-type, og UI navigerer efter kontotypen.

### AC6 — Password kan ikke omgås

**Givet** en konto med password eller et kald i Simply/Production  
**Når** login-requesten mangler password  
**Så** returneres 401 uden session eller JWT. Kun en passwordløs seed-person i Development kan bruge det eksplicitte udviklerlogin.

### AC7 — Profilgemning

**Givet** en autentificeret bruger med ændrede profilfelter  
**Når** **Gem profil** vælges  
**Så** opdateres kun profilfelterne, mens tema og Vipps-mapping ikke gensendes.

### AC8 — Vipps-testmapping

**Givet** en autentificeret Person i et miljø med sandboxfunktion  
**Når** en ledig testperson vælges og gemmes  
**Så** opdateres mappingen med selvstændig feedback uden at gemme profilformularen.

### AC9 — Udviklerværktøjer

**Givet** en Simply/Production-build  
**Når** profilen vises  
**Så** findes Udvikler-fanen ikke i DOM’en, og frontend kalder ingen `/api/dev/*`-rute.

### AC10 — Development

**Givet** lokal frontend og backend i Development  
**Når** Udvikler-fanen åbnes  
**Så** kan en seedet testperson vælges, og reset kan udføres efter eksplicit bekræftelse som før.

### AC11 — Navigation

**Givet** et gammelt link til `/login` eller `/register`  
**Når** linket åbnes  
**Så** lander brugeren i korrekt mode i det nye kontocenter uden 404 eller tab af redirect-intention.

### AC12 — Responsivitet og accessibility

**Givet** en viewport fra 320 px mobilbredde til tablet  
**Når** faner og formularer bruges med touch eller tastatur  
**Så** er alle handlinger synlige, bottom-nav dækker ikke indhold, og tabs kan betjenes med korrekt fokus/ARIA.

## Testkrav

### Frontend

- Tab- og mode-skift, inklusive query-parametre.
- Synlighed for Min profil, Vipps-test og Udvikler efter session, participant-type og environment.
- Person- og merchantvalidering, herunder MSN, email, password og passwordbekræftelse.
- Korrekte AuthService payloads for begge kontotyper.
- Merchant- og personlogin giver korrekt navigation.
- Lazy data-load: mapping/directory kaldes først ved åbning af deres faner.
- Dev-fanen og alle dev-API-kald er fraværende i production build-state.
- Profil-, mapping- og reset-feedback påvirker ikke hinanden.
- Accessibility-attributter og tastaturbaseret faneskift.

### Backend

- Merchantregistrering kræver email, password og MSN.
- Merchant password hashes og returneres aldrig.
- Email-unikhed gælder på tværs af Person og Merchant.
- Login virker for både Person og Merchant og afviser forkert password.
- Login uden password afvises i Simply/Production, også når en gammel participant mangler password-hash.
- Login uden password kan kun bruges i Development af eksisterende seed-personer uden password-hash.
- En tom password-request kan ikke omgå password-verifikation for en konto med password-hash.
- LoginResponse indeholder korrekt participant-type.

Alle HTTP-, Google- og eksterne integrationer mockes i frontendtests. Ingen live Google-, Vipps- eller MobilePay-kald.

## Forventet teknisk påvirkning ved implementering

- Angular profile/account component, template, styles og tests.
- Login/register routes eller kompatible redirects.
- `AuthService` sessionmodel og requests.
- `AuthController`, register/login DTO’er og participant-service/repository lookup.
- Relevante backend unit/controller-tests.
- Dokumentation for auth, profil og current state.

## Database, sikkerhed og deployment

- **Database:** Ingen migration forventes; `Participant.Email` og `Participant.PasswordHash` findes allerede.
- **Sikkerhed:** Public auth-kontrakt og merchant-login ændres; separat implementeringsplan og Product Owner-godkendelse er obligatorisk.
- **Secrets:** Ingen nye secrets.
- **Dependencies:** Ingen nye npm- eller NuGet-pakker.
- **Deployment:** Ingen automatisk deploy. Simply-konfiguration må fortsat skjule Udvikler-fanen og vil efter UC-09 returnere 404 for dev-ruter.

## Ikke en del af use casen

- Merchant-dashboard, ordrebehandling eller POS-integration.
- Redigering af merchant-logo, Vipps client secret eller subscription key.
- Password reset, email-verifikation eller MFA.
- Nye OAuth-providers eller Google-login for merchants.
- Nye dev-endpoints eller adgang til dev-endpoints i Simply/Production.
- Ændring af betalings-, reservation- eller capture-flow.
- Generelt redesign af øvrige PayNSync-sider.

## Definition of Done

- Alle acceptance criteria er implementeret.
- Backend- og frontendtests er grønne.
- Production viser ingen udviklerfunktioner og sender ingen dev-kald.
- Person- og merchantkonto kan oprettes og logge ind igen.
- UI bruger PayNSync-branding, eksisterende temaer og mobile-first mønstre.
- Berørte dokumenter opdateres efter verificeret implementering.
- Ingen databaseændring, ny dependency eller skjult scopeudvidelse er introduceret.
