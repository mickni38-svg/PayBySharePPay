# UC-13: Deltag i gruppeordre via JoinToken

## Implementeringsprofil

- **Anbefalet model:** GPT-5.6 Sol
- **Reasoning:** Medium
- **Opgavetype:** NEW_USE_CASE
- **Størrelse:** Afgrænset join-flow; genbrug eksisterende Order/OrderParticipant

## Mål

Et eksisterende `Order.JoinToken` kan bruges i et invitationslink, så en logget ind person kan tilslutte sig en aktiv gruppeordre uden at host manuelt vælger personen på forhånd.

## Beslutningsgate

Før implementering skal Product Owner godkende:

1. om kun eksisterende PayNSync-brugere må joine (anbefalet i v1);
2. om join kræver host-godkendelse eller giver status `Accepted` direkte;
3. om linket udløber eller deaktiveres, når ordren forlader `Collecting`.

Modellen må ikke vælge disse forretningsregler selv.

## Scope efter beslutning

- Offentligt link må vise minimal, ikke-følsom information; selve join-handlingen kræver JWT.
- Endpoint slår ordren op på et uforudsigeligt JoinToken og validerer aktiv status.
- Genbrug eksisterende `OrderParticipant` og generér unikt `ParticipantToken`.
- Host, merchant og eksisterende deltager må ikke oprettes som dublet.
- Join skal være atomisk og idempotent for samme bruger/token.
- Angular-route viser gyldig, ugyldig, udløbet og allerede-tilmeldt tilstand.
- Der må ikke vises andre deltageres persondata før autoriseret adgang.

## Acceptkriterier

### AC1 – Join

En godkendt bruger med gyldigt token tilføjes efter den valgte statusregel og kan se ordren efter eksisterende autorisationsregler.

### AC2 – Idempotens

Gentaget join fra samme bruger returnerer eksisterende medlemskab uden dublet.

### AC3 – Ugyldigt link

Ukendt, deaktiveret eller ikke-længere-gyldigt token giver 404/410 uden at afsløre ordredata.

### AC4 – Rollebeskyttelse

Host og merchant kan ikke tilføjes som almindelig deltager via linket.

### AC5 – Samtidighed

To samtidige join-requests for samme bruger skaber højst én OrderParticipant. Hvis der kræves nyt unikt database-constraint, skal migrationen godkendes særskilt.

## Test

- Gyldig join, gentagelse, ugyldigt token, forkert ordrestatus, host/merchant og samtidighed.
- Frontend HTTP mocks og route-tilstande.
- Ingen betalingsreservation startes ved join.

## Ikke en del af use casen

- Anonym gæstebruger.
- Deling via SMS/e-mail.
- Tilføjelse efter betalingsflowet er startet.
- Ændring af reserve/capture.
