# PayNSync – implementeringsrækkefølge

## Formål

Denne mappe opdeler redesign af oprettelse af gruppebetaling i fem afgrænsede features. Giv kun Copilot Claude én featurefil ad gangen og fortsæt først, når den aktuelle feature er implementeret og testet.

## Rækkefølge

1. `01-FEATURE-merchant-logo.md`
2. `02-FEATURE-home-merchant-carousel.md`
3. `03-FEATURE-wizard-step-1-participants.md`
4. `04-FEATURE-wizard-step-2-details.md`
5. `05-FEATURE-wizard-step-3-review-create.md`

## Fælles regler for alle features

- Analysér den eksisterende løsning, før der ændres kode.
- Genbrug eksisterende komponenter, services, API-kald, modeller, validering og oprettelsesflow.
- Merchant- og deltagerdata må aldrig hardcodes eller erstattes med statiske mockupdata.
- Navne, e-mailadresser, merchants og tekster i mockupperne er kun visuelle eksempler.
- Merchant og deltagere bygger på samme underliggende bruger- og vennerelation, men vises i forskellige grupper og bruges i forskellige roller.
- Bevar eksisterende arkitektur, kodestil og projektinstruktioner.
- Implementér kun den featurefil, der er givet. Foretag ikke ændringer fra senere featurefiler på forhånd.
- Tilføj eller opdatér relevante tests, og kør eksisterende tests efter ændringen.
- Afslut med en kort liste over ændrede filer, testresultater og eventuelle åbne punkter.

## Fælles designregler

- De tilhørende mockups er bindende reference for layout, bredde, størrelser, afstande, typografi, kort, felter, knapper og progress-indikator.
- Siderne skal bruge PayNSyncs eksisterende helt sorte theme med de eksisterende blå og orange brandfarver.
- Forsiden og alle tre wizard-sider skal have samme visuelle theme.
- Indholdet skal følge den eksisterende smalle, centrerede mobilbredde under PayNSync-logoet.
- Designet skal være responsivt og må ikke skabe vandret overflow på siden.

## Flow efter alle fem features

1. Brugeren vælger en merchant fra forsiden.
2. Wizard trin 1 åbner med merchanten låst og brugeren vælger deltagere.
3. På trin 2 indtastes titel og valgfri besked.
4. På trin 3 kontrolleres oplysningerne, og den eksisterende oprettelsesfunktion kaldes.

