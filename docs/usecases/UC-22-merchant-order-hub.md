# UC-22: Merchant Order Hub til iPad

## Status

Planlagt.

## Formål

Give merchants uden eget ordresystem en enkel webapp til at modtage PayNSync-ordrer.

## Brugerhistorie

Som merchant vil jeg se og behandle nye betalte ordrer på en iPad, så jeg kan bruge PayNSync uden et eksternt POS-system.

## Funktionelt scope

- Merchant-login og adgang til egne ordrer.
- Manuel aktivering af Order Hub-adgang på merchantkontoen.
- Permanent oprettelse af merchant-ordren umiddelbart efter succesfuld capture.
- Tabletvenlig ordrekø med lyd ved nye ordrer.
- Visning af ordrelinjer, host, adresse, bemærkning og betaling.
- Statusflow fra ny ordre til afsluttet ordre.
- Enkel ordrehistorik.
- Mulighed for at installere webappen som PWA på en iPad.

## Acceptkriterier

- En færdig PayNSync-ordre vises kun for den relevante merchant.
- Order Hub modtager ordren internt og afhænger ikke af et eksternt merchant-callback.
- Merchant kan acceptere ordren og ændre dens status.
- Genåbning efter forbindelsestab viser alle ikke-afsluttede ordrer.
- Alarmlyd kan aktiveres af merchant fra brugergrænsefladen.

## Ikke i scope

- Komplet POS-, lager- eller regnskabssystem.
- Native iPad-app.
- Automatisk abonnement, fakturering eller betalingsopkrævning.
