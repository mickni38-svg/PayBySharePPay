# UC-17: Leveringsadresse og separat Indstillinger-fane

## Status

✅ Implementeret på `main`.

## Formål

Give personbrugere en gemt standard-leveringsadresse, som senere kan bruges som udgangspunkt, når en gruppeordre skal leveres, og samtidig gøre **Profil og konto** mere overskuelig på mobil ved at flytte app-indstillinger til deres egen fane.

## Brugerhistorie

Som bruger vil jeg kunne gemme min leveringsadresse på min profil, så en fremtidig ordre kan afleveres det rigtige sted, uden at jeg skal indtaste adressen igen hver gang.

Som bruger vil jeg have konto-oplysninger og app-indstillinger adskilt, så profilsiden ikke kræver unødvendig scrolling.

## Funktionelt scope

### Konto-fanen

- Profil-accordion bevares.
- Personlige oplysninger indeholder **Navn**, **Email** og **Telefon**.
- Personkonti får sektionen **Leveringsadresse** med:
  - Adresse
  - Postnr.
  - By
  - Land
- Land starter som **Danmark**, hvis brugeren ikke tidligere har gemt et land.
- Leveringsadressen gemmes sammen med profilen via den eksisterende **Gem profil**-handling.
- Merchantkonti viser ikke personens leveringsadressefelter; deres eksisterende virksomhedsadresse-flow ændres ikke.

### Indstillinger-fanen

- **Indstillinger** flyttes ud af Konto-fanen og bliver en selvstændig topfane.
- Fanen indeholder eksisterende **Notifikationer** og **Tema**.
- Indstillinger gemmes fortsat på samme måde som før; ingen ændring af storage-semantik.

### Faner

For en autentificeret person vises:

`Konto | Indstillinger | Vipps-test`

I Development kan den eksisterende udviklerfane desuden være synlig.

## Data

Følgende nullable felter tilføjes til `Participant`:

- `Address`
- `PostalCode`
- `City`
- `Country`

Profil-API'et læser og skriver felterne.

Leveringsadressen er en **standardadresse på profilen**. Når selve ordreleveringen implementeres, skal adressen kopieres til ordren som et snapshot, så en senere profilændring ikke ændrer leveringsadressen på en allerede oprettet ordre.

## Ikke i scope

- Ingen kopiering af adressen til `GroupOrder` endnu.
- Ingen integration til bud-/leveringstjenester.
- Ingen adresseopslag eller validering mod ekstern adresse-service.
- Ingen GPS/geokodning.

## Acceptkriterier

### AC1 — Leveringsadresse kan indtastes

**Givet** en personbruger er logget ind  
**Når** Konto → Profil åbnes  
**Så** kan brugeren indtaste Adresse, Postnr., By og Land.

### AC2 — Leveringsadresse gemmes

**Givet** brugeren har udfyldt leveringsadressen  
**Når** brugeren vælger **Gem profil**  
**Så** gemmes leveringsadressen på brugerens Participant-record via profil-API'et.

### AC3 — Gemte værdier genindlæses

**Givet** brugeren tidligere har gemt sin leveringsadresse  
**Når** profilsiden åbnes igen  
**Så** vises de gemte værdier i felterne.

### AC4 — Indstillinger har egen fane

**Givet** en autentificeret bruger åbner Profil og konto  
**Så** vises **Indstillinger** som selvstændig topfane og ikke som accordion under Konto.

### AC5 — Eksisterende indstillinger bevares

**Når** brugeren ændrer notifikationer eller tema under Indstillinger  
**Så** fungerer lagringen efter samme regler som før UC-17.

### AC6 — Merchant påvirkes ikke af leveringsfelterne

**Givet** den autentificerede konto er en merchant  
**Så** vises personens leveringsadressefelter ikke, og eksisterende merchantdata fortsætter uændret.

## Testkrav

Frontendtests skal verificere:

- Indstillinger kan åbnes som separat fane for autentificerede brugere.
- Anonyme brugere kan ikke åbne Indstillinger-fanen.
- Gemte adressefelter indlæses i profil-state.
- Adressefelterne sendes med ved `saveProfile()`.
- Eksisterende Vipps- og developer-lazy-loading bevares.

## Definition of Done

- Frontend, API, service- og datalag understøtter de fire adressefelter.
- EF-migration tilføjer felterne til `Participants`.
- Konto- og Indstillinger-layout er adskilt på mobil.
- Eksisterende profilgemning og tema/notifikationsfunktionalitet er bevaret.
