# UC-24: Driftssikker merchant-ordrelevering

## Status

Planlagt.

## Formål

Sikre at en betalt ordre ikke går tabt, hvis merchantens modtagersystem midlertidigt er utilgængeligt.

## Brugerhistorie

Som merchant vil jeg være sikker på, at alle betalte ordrer bliver leveret præcis én gang i min ordrekø.

## Funktionelt scope

- Permanent registrering af hvert leveringsforsøg.
- Automatisk retry ved midlertidige fejl.
- Idempotency key på ordrelevering.
- Status for afventer, leveret og fejlet.
- Mulighed for manuel genudsendelse.

## Acceptkriterier

- En timeout eller serverfejl sletter ikke ordren.
- Retry opretter ikke en dublet hos merchant.
- Leveringsstatus kan ses og fejlsøges.
- Betalingsstatus rulles ikke tilbage ved leveringsfejl.

## Ikke i scope

- Refundering som automatisk følge af leveringsfejl.
- Døgnbemandet driftsorganisation eller SLA.

