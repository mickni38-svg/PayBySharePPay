# UC-20: Realistisk merchant-takeaway-demo

## Status

Planlagt.

## Formål

Erstatte den enkle Merchant Demo med en mere realistisk takeaway-side til end-to-end-test.

## Brugerhistorie

Som deltager vil jeg kunne vælge mad på en realistisk takeaway-side og bruge PayNSync som gruppebetalingsmulighed.

## Funktionelt scope

- Menu med kategorier, produkter, antal og tilvalg.
- Kurv med priser og samlet beløb.
- En tydelig PayNSync-knap i checkout.
- Indsendelse af deltagerens ordrekladde til PayNSync.
- Ingen frigivelse af kladden til merchantens ordrekø.

## Acceptkriterier

- Deltageren kan gennemføre det eksisterende reservationsflow fra merchant-siden.
- Produkter og tilvalg har stabile ID'er, som senere kan mappes.
- Merchantens ordrekø forbliver tom, indtil gruppebetalingen er gennemført.

## Ikke i scope

- Komplet CMS eller produktionsklart menusystem.
- Almindelig kortbetaling uden PayNSync.

