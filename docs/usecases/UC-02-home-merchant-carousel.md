# FEATURE 02: Merchant-søgning og carousel på forsiden

**Status:** Implementeret

**Teknisk note:** Senest anvendte merchants gemmes pr. bruger i browserens `localStorage`. Brugere uden historik får alfabetisk sortering.

## Mål

Den gamle knap **Opret ny gruppebetaling** erstattes af et søgefelt og en kompakt carousel med brugerens merchant-venner. Valg af en merchant er den eneste adgang til en ny gruppebetalingswizard.

## Instruks til Copilot Claude

Analysér først den eksisterende forside, venner-/merchant-services, routing, wizard-state, profilside og tests. Implementér derefter kun denne feature ved at genbruge den eksisterende funktionalitet. Bevar arkitektur og kodestil, kør relevante tests, og afslut med ændrede filer samt testresultat.

## Forudsætning

`01-FEATURE-merchant-logo.md` er implementeret.

## Designreference

Følg mockupfilen `../images/overview.jpeg`.

Mockuppens merchants og logoer er kun eksempler og må ikke hardcodes.

## Datakilde

- Brug den allerede implementerede venner-/forbindelsesfunktionalitet.
- Vis kun aktive venner fra merchant-gruppen.
- Merchant og deltagere er samme underliggende type, men denne side viser kun merchant-gruppen.
- Genbrug eksisterende services, modeller og API-kald. Udvid kun kontrakter, hvis logo eller rolledata mangler.

## Layout

- Fjern knappen **Opret ny gruppebetaling**.
- Vis overskriften **Start en gruppebetaling**.
- Vis søgefeltet **Søg spisested...** over carousellen.
- Hvert merchant-kort viser rigtigt logo og navn.
- Brug initialer som fallback, hvis en ældre merchant mangler logo.
- Kort og logoer skal have de kompakte proportioner fra mockuppen.
- Carouselens viewport skal holde sig inden for samme smalle, centrerede indholdsbredde som PayNSync-logoet og de øvrige forsideelementer.
- Ingen kort eller dele af carousellen må tegnes ud over højre eller venstre indholdskant.
- Flere merchants nås med swipe, vandret scrolling, mus eller tastatur inde i carousel-området.
- Der vises højst otte merchants i standardvisningen og højst otte søgeresultater.
- Der må ikke vises et kort eller en knap med teksten **Vælg fra liste**.
- **Overblik**, **Beskeder** og bundmenuen bevares.
- Rækken med brugernavn og **Log ud** fjernes fra forsiden.
- **Log ud** skal være tilgængelig på profilsiden gennem den eksisterende logout-funktion.

## Sortering og søgning

- Uden søgetekst vises op til otte merchants sorteret efter senest anvendt og derefter alfabetisk.
- Hvis brugeren har færre end otte, vises alle.
- Søgningen skal kunne søge blandt alle brugerens merchant-venner, men vise højst otte resultater.
- Match merchantnavn uden forskel på store og små bogstaver.
- Ved nul resultater vises **Ingen spisesteder fundet**.

## Valg og navigation

Når brugeren trykker på et merchant-kort:

1. Gem merchantens rigtige ID samt nødvendige visningsdata i wizardens midlertidige state.
2. Åbn wizard trin 1.
3. Merchant må ikke kunne ændres inde i wizarden.

Hvis wizardens URL åbnes uden gyldigt `merchantId` eller state, skal brugeren sendes tilbage til forsiden.

## Tom tilstand

Hvis brugeren ikke har merchant-venner:

- vis **Du har endnu ingen spisesteder på din venneliste**;
- vis **Find spisested**, som åbner det eksisterende flow til at finde eller tilføje en merchant;
- vis ikke en tom carousel.

## Acceptkriterier

### AC1 – Dynamisk carousel

**Givet** aktive venner i merchant-gruppen  
**Når** forsiden åbnes  
**Så** vises op til otte merchants fra den eksisterende datakilde med logo og navn  
**Og** ingen merchantdata er hardcodet.

### AC2 – Begrænset bredde

**Givet** flere merchants end der er plads til  
**Når** carousellen vises på en lille mobilskærm  
**Så** forbliver viewport og kort inden for den centrerede indholdsbredde  
**Og** resten nås ved swipe inde i carousel-området.

### AC3 – Søgning

**Givet** flere merchant-venner  
**Når** brugeren indtaster hele eller dele af et navn  
**Så** vises højst otte matchende merchants uafhængigt af store og små bogstaver.

### AC4 – Start wizard

**Givet** en merchant i carousellen  
**Når** brugeren vælger kortet  
**Så** åbnes trin 1 med den valgte merchants rigtige ID i wizard-state.

### AC5 – Logout

**Givet** forsiden  
**Så** vises **Log ud** ikke på siden  
**Og** den eksisterende logout-handling kan bruges fra profilsiden.

## Test

- 0, 1, 8 og flere end 8 merchant-venner.
- Søgning med fuldt/delvist navn, store/små bogstaver og nul resultater.
- Swipe, mus, tastatur og smalle mobilbredder uden side-overflow.
- Merchant uden logo.
- Valg overfører korrekt merchant-ID.
- Direkte adgang til wizard uden gyldig state sender tilbage til forsiden.

## Ikke en del af denne feature

- Valg af deltagere.
- Titel og besked.
- Oprettelse af gruppebetaling.
- Redesign af **Overblik**, **Beskeder** eller bundmenuen.
