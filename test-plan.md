# Test plan — UC-04

## Automatiske komponenttests

Udvid `create-order.component.spec.ts` med:

- trin 2 viser overskrift, hjælpetekst og trinindikator 2 af 3;
- tom titel og titel med kun mellemrum holder **Næste** deaktiveret;
- titel på 80 tegn accepteres, og titel på 81 tegn afvises defensivt;
- titel trimmes før lagring i wizard-state;
- tom besked accepteres;
- besked på 500 tegn accepteres, og besked på 501 tegn afvises defensivt;
- titel- og beskedtællere følger de aktuelle værdier;
- flere linjer, danske tegn og emoji bevares uændret i beskeden;
- emoji er ikke længere et obligatorisk felt eller valideringskrav;
- opsummeringskortet viser dynamisk merchantnavn, rigtigt logo og korrekt deltagerantal;
- tilbage til trin 1 og frem igen bevarer titel, besked og deltagervalg;
- næste gemmer detaljerne i den eksisterende wizard-state og åbner trin 3;
- manglende deltager-state forhindrer trin 2 og sender brugeren tilbage til trin 1;
- ugyldig merchant-state følger den eksisterende navigation tilbage til forsiden;
- den eksisterende create-request kan fortsat oprettes med `category` udeladt.

## Build og test

- Kør `npx ng test --watch=false --browsers=ChromeHeadless`.
- Kør `npx ng build --configuration simply`.
- Lad GitHub Actions køre .NET-testjobbet og Angular-test/build-jobbet.
- Gennemgå PR-diffen for ændringer uden for UC-04.

## Manuel kontrol

- Sammenlign trin 2 med `docs/images/wizard2.jpeg`.
- Kontrollér sort tema, smal mobilbredde og blå accent.
- Kontrollér placering, størrelse og læsbarhed af begge tegntællere.
- Kontrollér at merchantlogoet bevarer proportionerne og ikke beskæres uhensigtsmæssigt.
- Kontrollér at langt indhold og 500 tegn ikke bryder mobil-layoutet.
- Kontrollér frem/tilbage-navigation med touch uden tab af state.
