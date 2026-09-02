# FEATURE 04: Wizard trin 2 – titel og besked

> **Status:** ✅ Implementeret og automatisk verificeret i PR #12.

## Implementeringsnote

Trin 2 bruger den eksisterende wizard-state fra UC-03, validerer og trimmer titel (maks. 80 tegn), bevarer valgfri besked præcist (maks. 500 tegn) og viser dynamisk merchantlogo, merchantnavn og deltagerantal. Emoji er ikke længere et obligatorisk felt. API, database og betalingsflow er uændret.

## Mål

Værten indtaster gruppebetalingens titel og en valgfri besked. Data gemmes i den eksisterende wizard-state og bevares ved navigation.

## Instruks til Copilot Claude

Analysér først det eksisterende wizard-flow, formularmønstre, validering og state. Implementér kun denne feature uden at oprette en parallel formular- eller state-løsning. Bevar arkitektur og kodestil, kør relevante tests, og afslut med ændrede filer samt testresultat.

## Forudsætning

`03-FEATURE-wizard-step-1-participants.md` er implementeret og leverer merchant samt mindst én deltager i wizard-state.

## Designreference

Følg mockupfilen `../images/wizard2.jpeg`.

Teksten **Pizzaaften**, beskeden, merchantnavnet og deltagerantallet i mockuppen er kun eksempler og må ikke hardcodes.

## Adgang

- Trin 2 må kun vises med en gyldig merchant og mindst én gyldig deltager i wizard-state.
- Vis sideoverskriften **Detaljer**, hjælpeteksten og trinindikatoren 2 af 3 som i mockuppen.
- Ved manglende eller ugyldig state navigeres brugeren tilbage til det relevante tidligere trin eller forsiden.
- Genbrug den state og routing, der er etableret i de tidligere features.

## Titel

- Label: **Titel**.
- Enkeltlinjet tekstfelt.
- Placeholder: **Fx Pizzaaften**.
- Påkrævet.
- Trim indledende og afsluttende mellemrum før validering og lagring.
- Maksimalt 80 tegn.
- Vis tegnantal som i mockuppen.
- **Næste** er kun aktiv, når titlen er gyldig.

## Besked

- Label: **Besked**.
- Multilinje-felt (`textarea`).
- Placeholder: **Skriv en besked til deltagerne...**.
- Valgfrit.
- Maksimalt 500 tegn.
- Vis tegnantal som i mockuppen.
- Linjeskift, danske tegn og emoji skal bevares ved state, lagring og senere visning.

## Opsummeringskort

- Vis et kompakt kort med den valgte merchants rigtige logo og navn samt aktuelt deltagerantal.
- Data skal komme fra wizard-state, ikke fra hardcodede værdier.
- Merchantlogoets proportioner skal følge mockuppen.

## Navigation og state

- Tilbage-knappen åbner trin 1 med tidligere valgte deltagere bevaret.
- **Næste** gemmer titel og besked i den eksisterende wizard-state og åbner trin 3.
- Genindlæsning eller den eksisterende state-mekanisme må ikke introducere en ny parallel state-løsning.
- Brug samme sorte theme, smalle indholdsbredde og blå accent som forsiden og trin 1.

## Acceptkriterier

### AC1 – Titelvalidering

**Givet** trin 2  
**Når** titlen er tom eller kun mellemrum  
**Så** kan brugeren ikke fortsætte  
**Når** en titel på højst 80 tegn indtastes  
**Så** kan brugeren fortsætte.

### AC2 – Besked

**Givet** en besked på højst 500 tegn med flere linjer  
**Når** brugeren fortsætter  
**Så** bevares teksten og linjeskiftene korrekt i wizard-state.

### AC3 – Dynamisk opsummering

**Givet** merchant og deltagere fra trin 1  
**Når** trin 2 vises  
**Så** viser kortet den rigtige merchant og det rigtige deltagerantal uden hardcodede data.

### AC4 – Tilbage-navigation

**Givet** udfyldt titel og besked  
**Når** brugeren går tilbage til trin 1 og frem igen  
**Så** bevares titel, besked og deltagervalg.

## Test

- Tom titel, mellemrum, 80 og 81 tegn.
- Tom besked, 500 og 501 tegn.
- Flere linjer, danske tegn og emoji.
- Dynamisk merchant og deltagerantal.
- Frem/tilbage-navigation uden datatab.
- Ugyldig eller manglende wizard-state.

## Ikke en del af denne feature

- Ændring af deltagere ud over navigation tilbage til trin 1.
- Endelig oprettelse, invitationer, reservation eller capture.
