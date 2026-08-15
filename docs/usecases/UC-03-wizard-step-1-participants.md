# FEATURE 03: Wizard trin 1 – merchant og deltagere

## Mål

Wizard trin 1 viser den merchant, der allerede er valgt på forsiden, og lader værten søge efter samt vælge deltagere gennem den eksisterende vennerfunktionalitet.

## Instruks til Copilot Claude

Analysér først det nuværende wizard-flow, deltagerliste, søgning, state, routing og backendkontrakter. Implementér kun denne feature og genbrug den eksisterende deltagerfunktionalitet. Bevar arkitektur og kodestil, kør relevante tests, og afslut med ændrede filer samt testresultat.

## Forudsætning

`02-FEATURE-home-merchant-carousel.md` er implementeret og leverer et gyldigt `merchantId` til wizard-state.

## Designreference

Følg mockupfilen `../images/wizard1.jpeg`.

Personer, e-mailadresser og merchant i mockuppen er kun eksempler og må ikke hardcodes.

## Adgang og merchant

- Wizarden kan kun startes ved valg af merchant på forsiden.
- Hvis `merchantId` mangler eller er ugyldigt, sendes brugeren tilbage til forsiden.
- Vis kortet **VALGT SPISESTED** med merchantens rigtige logo og navn.
- Logoet skal have samme kompakte størrelse som merchant-logoerne på forsiden.
- Vis ikke teksten **Valgt**, et valgt-flueben, **Skift**, merchant-søgning eller en merchant-liste.
- Merchant kan ikke ændres på trin 1. En anden merchant vælges ved at gå tilbage til forsiden og starte på ny.

## Datamodel og datakilde

- Merchant og deltagere er samme underliggende bruger-/vennetype, men hører til forskellige grupper og roller.
- Deltagerlisten skal komme fra den eksisterende venner-/forbindelsesfunktionalitet og deltagergruppe.
- Genbrug eksisterende søgning, indlæsning, valg og fravalg af deltagere.
- Opret ikke statiske deltagerlister eller demo-personer.
- Den valgte merchant gemmes som `MerchantId`.
- Valgte venner gemmes som deltagere gennem de eksisterende ID'er og modeller.

## Filtrering

- Den indloggede bruger/vært må aldrig vises, fremsøges eller vælges som deltager.
- Værten gemmes som `HostUserId`, ikke som deltager.
- Den valgte merchant må ikke vises, fremsøges eller vælges som deltager.
- Kun aktive venner fra deltagergruppen vises.
- Den samme deltager må kun vælges én gang.

## Brugerflade

- Vis sideoverskriften **Vælg deltagere** og trinindikatoren 1 af 3 som i mockuppen.
- Vis sektionen **Deltagere** under merchant-kortet.
- Vis søgefeltet **Søg venner...**.
- Vis eksisterende deltagere i kompakte rækker med initial/avatar, navn og sekundær tekst.
- Valgte deltagere markeres tydeligt og kan fravælges.
- Vis antal valgte deltagere.
- **Næste** er kun aktiv, når mindst én deltager er valgt.
- Tilbage-knappen går til forsiden uden at skabe en gruppebetaling.
- Brug samme sorte theme, bredde og blå accent som mockuppen og forsiden.

## State

Ved navigation til trin 2 skal wizard-state mindst indeholde:

- `MerchantId` og nødvendige merchant-visningsdata;
- valgte deltager-ID'er og nødvendige visningsdata;
- `HostUserId` fra den aktuelle session, ikke fra brugerinput.

State skal bevares, hvis brugeren senere går tilbage fra trin 2 eller 3.

## Acceptkriterier

### AC1 – Merchant fra forsiden

**Givet** at en merchant er valgt på forsiden  
**Når** trin 1 åbnes  
**Så** vises merchantens rigtige navn og kompakte logo  
**Og** merchanten kan ikke skiftes på siden  
**Og** teksten **Valgt** og et valgt-flueben vises ikke.

### AC2 – Eksisterende deltagerfunktion

**Givet** venner i deltagergruppen  
**Når** trin 1 indlæses  
**Så** kommer listen fra den eksisterende service/API  
**Og** den kan søges, vælges og fravælges uden hardcodede personer.

### AC3 – Værten filtreres

**Givet** den indloggede bruger er vært  
**Når** listen eller søgningen vises  
**Så** forekommer værten ikke og kan ikke vælges.

### AC4 – Merchant filtreres

**Givet** en valgt merchant  
**Når** deltagere vises eller søges  
**Så** forekommer merchanten ikke som mulig deltager.

### AC5 – Næste

**Givet** ingen deltagere er valgt  
**Så** er **Næste** deaktiveret  
**Når** mindst én gyldig deltager vælges  
**Så** aktiveres knappen og korrekt state føres til trin 2.

## Test

- Direkte adgang uden merchant-state.
- Merchantkortets logo, navn og fravær af ekstra valgt-markering.
- Vært og merchant filtreres fra både liste og søgning.
- Valg, fravalg og dubletbeskyttelse.
- Ingen deltagere og mindst én deltager.
- State bevares ved frem/tilbage-navigation.

## Ikke en del af denne feature

- Indtastning af titel og besked.
- Endelig oprettelse af gruppebetaling.
- Ny venner- eller venskabsanmodningsfunktionalitet.
