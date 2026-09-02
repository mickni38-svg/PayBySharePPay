# Test plan — UC-03

## Automatiske tests

Udvid `create-order.component.spec.ts` med:

- direkte adgang uden merchant-state sender brugeren til forsiden;
- ukendt eller ikke-ven merchant-ID sender brugeren til forsiden;
- merchantkortet bruger valideret navn og logo fra den eksisterende service;
- kun `Person`-venner vises som deltagere;
- værten filtreres fra;
- den valgte merchant filtreres fra;
- dublerede personer deduplikeres efter ID;
- søgning matcher navn og sekundær tekst uden forskel på store/små bogstaver;
- valg og fravalg ændrer kun den valgte persons state;
- samme deltager kan ikke forekomme eller vælges to gange;
- næste-knappen er deaktiveret uden deltagere;
- mindst én deltager gør trin 1 gyldigt og åbner trin 2;
- merchant, HostUserId og valgte deltager-ID'er er bevaret i wizard-state;
- frem/tilbage-navigation bevarer deltagervalg;
- fejlet venneindlæsning giver en stabil fejltilstand.

## Build og test

- Kør `npx ng test --watch=false --browsers=ChromeHeadless`.
- Kør `npx ng build --configuration simply`.
- Lad GitHub Actions køre både .NET-testjobbet og Angular-test/build-jobbet.
- Gennemgå PR-diffen for ændringer uden for UC-03.

## Manuel kontrol

- Sammenlign trin 1 med `docs/images/wizard1.jpeg`.
- Kontrollér den smalle mobilbredde og sort/blå styling.
- Kontrollér kompakt merchant-logo i samme visuelle størrelse som på forsiden.
- Kontrollér fravær af "Valgt", flueben, Skift og merchant-søgning.
- Kontrollér søgning, valg/fravalg og valgt antal med touch.
- Kontrollér at tilbage fra trin 1 går til forsiden uden at oprette data.
