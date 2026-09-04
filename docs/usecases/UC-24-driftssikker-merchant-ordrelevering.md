# UC-24: Driftssikker merchant-ordrelevering

## Status

Planlagt.

## Formål

Sikre først, at en betalt ordre oprettes permanent og idempotent i PayNSync Order Hub. Senere kan samme princip anvendes ved levering til eksterne merchant-systemer.

## Brugerhistorie

Som merchant vil jeg være sikker på, at alle betalte ordrer bliver leveret præcis én gang i min ordrekø.

## Funktionelt scope

- Permanent registrering af hvert leveringsforsøg.
- Permanent registrering mellem succesfuld capture og Order Hub-ordrekøen.
- Automatisk retry ved midlertidige fejl.
- Idempotency key på ordrelevering.
- Status for afventer, leveret og fejlet.
- Mulighed for manuel genudsendelse.

## Acceptkriterier

- En timeout eller serverfejl sletter ikke ordren.
- Retry opretter ikke en dublet hos merchant.
- Leveringsstatus kan ses og fejlsøges.
- Betalingsstatus rulles ikke tilbage ved leveringsfejl.
- En betalt ordre kan genskabes i Order Hub efter en afbrudt proces.

## Ikke i scope

- Refundering som automatisk følge af leveringsfejl.
- Døgnbemandet driftsorganisation eller SLA.
- Fuld retry-integration til eksterne POS-systemer i første Order Hub-version.
