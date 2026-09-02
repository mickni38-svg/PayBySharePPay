# UC-12: Send påmindelser til afventende deltagere

## Implementeringsprofil

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- **Opgavetype:** NEW_USE_CASE
- **Størrelse:** Én vertikal slice gennem Angular, API og eksisterende Message-model

## Mål

En host kan sende en rigtig beskedpåmindelse til deltagere, der stadig har status `Invited`. Den nuværende frontend-placeholder og `console.log` erstattes af et backend-kald, som opretter beskeder gennem den eksisterende beskedfunktionalitet.

## Forretningsregler

- Kun ordrenes host må sende påmindelser.
- Kun deltagere med status `Invited` modtager påmindelsen.
- Host og merchant må aldrig modtage den.
- Påmindelsen skal genbruge eksisterende Message-entitet/service og beskedindbakke.
- Gentagne klik må ikke skabe en beskedstorm. Planen skal foreslå en lille, testbar cooldown/idempotensregel og vente på Product Owner-godkendelse af tidsrummet.
- Ingen push-notifikation eller e-mail i denne use case.

## Scope

- Et host-beskyttet endpoint under den eksisterende ordre-API.
- Server-side opslag og validering af aktuelle afventende deltagere.
- Opret én systembesked pr. gyldig modtager atomisk efter eksisterende persistence-mønster.
- Angular kalder endpointet, viser loading, succes og forståelig fejl.
- Knap deaktiveres under request og ved ingen afventende deltagere.
- Fjern falsk success fra placeholderen.

## Acceptkriterier

### AC1 – Gyldig påmindelse

Host sender påmindelse, og hver aktuel `Invited` deltager får præcis én ny besked med link/kontekst til ordren.

### AC2 – Modtagere

Accepted/Declined deltagere, host og merchant får ingen påmindelse.

### AC3 – Autorisation

Ikke-host får 403; manglende token giver 401; ingen beskeder oprettes.

### AC4 – Dubletbeskyttelse

Dobbeltklik eller gentaget request inden for den godkendte regel opretter ikke dubletbeskeder.

### AC5 – UI-fejl

Ved API-fejl vises fejl, og UI må ikke vise “sendt”.

## Test

- Backend: modtagerfilter, 401/403, ingen afventende, idempotens/cooldown og rollback.
- Frontend: HTTP mock, loading/success/error og dobbeltklik.
- Ingen live eksterne tjenester.

## Ikke en del af use casen

- Push, SMS eller e-mail.
- Automatisk tidsplan.
- Nye Message-tabeller.
- Ændring af deltagerstatus.
