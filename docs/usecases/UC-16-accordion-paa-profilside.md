# UC-16: Accordion på profilsidens Konto-sektion

## Status

📝 Klar til review – ikke implementeret.

## Formål

Gøre **Profil og konto** mere kompakt og overskuelig på mobil ved at gøre de to store bokse i **Konto**-fanen til accordions.

Use casen ændrer kun præsentation og interaktion på profilsiden. Eksisterende profilfelter, indstillinger, gemmeadfærd og backend-kontrakter skal genbruges uændret.

## Nuværende situation

På **Konto**-fanen vises i dag to store cards efter hinanden:

1. Profilkortet med brugerens identitet, navn, email, telefon og knappen **Gem profil**.
2. **Indstillinger** med bl.a. notifikationer og øvrige lokale indstillinger.

På mobil giver det en lang side med meget scrolling. Brugeren har typisk kun behov for at arbejde med én af de to sektioner ad gangen.

## Brugerhistorie

Som bruger vil jeg kunne folde **Profil** og **Indstillinger** sammen og ud, så profilsiden er kortere og jeg hurtigt kan fokusere på den sektion, jeg vil ændre.

## Funktionelt scope

### 1. Profil-accordion

- Det eksisterende profilkort bliver en accordion med overskriften **Profil**.
- Accordion-headeren viser fortsat den kompakte identitet med avatar/initial, navn og kontotype, så brugeren kan genkendeke sin konto uden at åbne sektionen.
- Når sektionen er åben, vises de eksisterende felter **Navn**, **Email** og **Telefon (valgfri)** samt **Gem profil**.
- Eksisterende validering, loading, success- og fejlfeedback ændres ikke funktionelt.
- Profilsektionen er **åben som standard**, når Konto-fanen åbnes.

### 2. Indstillinger-accordion

- Det eksisterende **Indstillinger**-card bliver en accordion med overskriften **Indstillinger**.
- Den eksisterende korte beskrivelse kan vises i det udfoldede indhold.
- Når sektionen er åben, vises de eksisterende indstillinger, herunder **Notifikationer**, med samme funktionalitet som i dag.
- Indstillinger er **lukket som standard**.

### 3. Accordion-adfærd

- Kun én af de to sektioner er åben ad gangen.
- Når brugeren åbner **Indstillinger**, lukkes **Profil** automatisk.
- Når brugeren åbner **Profil**, lukkes **Indstillinger** automatisk.
- Den åbne sektion kan lukkes, så begge sektioner kan være lukkede.
- Hele accordion-headeren er klikbar/tapbar.
- Headeren viser en tydelig chevron/pil, som visuelt angiver åben/lukket tilstand.
- Åbning og lukning må gerne animeres diskret, men animationen må ikke forsinke interaktionen mærkbart.

### 4. State og data

- Accordion-skift må ikke nulstille eller genindlæse formularfelter.
- Ugemte ændringer i profilformularen bevares, hvis Profil lukkes og åbnes igen.
- Accordion-skift må ikke sende API-kald.
- Eksisterende gemmegrænser fra UC-15 bevares: **Gem profil** gemmer kun profilfelter, mens lokale indstillinger fortsat håndteres som hidtil.

## Ikke i scope

- Ingen ændring af **Konto / Vipps-test**-fanerne.
- Ingen ændring af login, registrering eller merchant-flow.
- Ingen backendændringer, databaseændringer eller migrations.
- Ingen ændring af bottom navigation.
- Ingen ny accordion på Vipps-test eller eventuelle udviklerfunktioner.

## UI-krav

- Accordions skal visuelt passe til det eksisterende mørke PayNSync-carddesign.
- Når en sektion er lukket, skal den fylde væsentligt mindre end det nuværende udfoldede card.
- Accordion-headeren skal have minimum 44×44 px touch target.
- Chevron/pil placeres tydeligt i højre side af headeren.
- Fokus-, hover- og aktiv tilstand skal være tydelig og fungere i eksisterende temaer.
- Indholdet må ikke blive skjult bag den faste bottom navigation, når en accordion åbnes.

## Tilgængelighed

- Accordion-headeren implementeres som en rigtig `button` eller tilsvarende semantisk kontrol.
- Brug `aria-expanded` til at angive åben/lukket tilstand.
- Brug `aria-controls` til at forbinde header og panel.
- Accordion skal kunne betjenes med tastatur.
- Chevronen må ikke være den eneste indikator for tilstanden; den semantiske tilstand skal være tilgængelig for skærmlæsere.
- Ved `prefers-reduced-motion` skal åbne/lukke-animation reduceres eller fjernes.

## Acceptkriterier

### AC1 — Profil er åben som standard

**Givet** en autentificeret bruger åbner **Profil og konto → Konto**  
**Når** siden vises  
**Så** er **Profil** udfoldet, og **Indstillinger** er sammenfoldet.

### AC2 — Åbn Indstillinger

**Givet** at **Profil** er åben  
**Når** brugeren trykker på **Indstillinger**  
**Så** åbnes Indstillinger, og Profil lukkes automatisk.

### AC3 — Åbn Profil

**Givet** at **Indstillinger** er åben  
**Når** brugeren trykker på **Profil**  
**Så** åbnes Profil, og Indstillinger lukkes automatisk.

### AC4 — Begge kan være lukkede

**Givet** at en accordion er åben  
**Når** brugeren trykker på dens header igen  
**Så** lukkes den, uden at den anden accordion åbnes automatisk.

### AC5 — Formularstate bevares

**Givet** at brugeren har ændret et profilfelt uden at gemme  
**Når** Profil lukkes og senere åbnes igen  
**Så** er den ugemte værdi stadig i feltet.

### AC6 — Ingen ekstra API-kald

**Givet** at profilsiden allerede er indlæst  
**Når** brugeren åbner eller lukker en accordion  
**Så** foretages ingen API-kald alene på grund af accordion-skiftet.

### AC7 — Eksisterende funktionalitet bevares

**Givet** at Profil eller Indstillinger er udfoldet  
**Når** brugeren anvender felter, Gem profil eller indstillingskontroller  
**Så** fungerer de efter samme regler som før UC-16.

### AC8 — Tilgængelig betjening

**Givet** at brugeren anvender tastatur eller skærmlæser  
**Når** accordion-headeren fokuseres og aktiveres  
**Så** kan sektionen åbnes/lukkes, og dens aktuelle tilstand kommunikeres via `aria-expanded`.

## Testkrav

Frontendtests skal som minimum verificere:

- Profil åben og Indstillinger lukket ved initial visning.
- Skift mellem de to accordions.
- Muligheden for at lukke den aktive accordion, så begge er lukkede.
- Kun én accordion kan være åben ad gangen.
- Ugemte profilværdier bevares gennem luk/åbn.
- Eksisterende **Gem profil**-adfærd fungerer efter accordion-ændringen.
- `aria-expanded` ændres korrekt.
- Accordion-headerne kan aktiveres med tastatur.

Tests må ikke kræve live eksterne services.

## Afhængigheder

- Bygger videre på **UC-15: Profil- og kontocenter med rollebaserede faner**.
- Skal genbruge den eksisterende profilside og dens state frem for at oprette nye formularer eller services.

## Definition of Done

- Alle acceptkriterier er implementeret og dækket af relevante frontendtests.
- Eksisterende tests er fortsat grønne.
- Angular production build er grøn.
- Ingen backend- eller databaseændringer er nødvendige.
- Mobilvisningen er manuelt verificeret, så de sammenfoldede cards giver en tydeligt kortere Konto-side.
