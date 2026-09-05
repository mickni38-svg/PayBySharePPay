# UC-20: Realistisk merchant-takeaway-demo

## Status

Implementeret.

## Formål

Erstatte den enkle Merchant Demo med en mere realistisk takeaway-side til salg, demonstration og end-to-end-test efter den første Order Hub-version.

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
- Funktionalitet, som er nødvendig for at lancere den første Order Hub-version.

## Implementeret løsning

- Pizzeria Roma har et statisk, mobilvenligt katalog med fem kategorier, stabile produkt-id'er og stabile tilvalgs-id'er.
- Deltageren vælger antal og tilvalg pr. produkt og kan ændre eller fjerne separate linjer i kurven.
- Forskellige konfigurationer af samme produkt bevares som separate ordrelinjer.
- Produkt-id sendes som normaliseret `lineId`; produkt- og tilvalgs-id'er gemmes desuden struktureret i `rawMerchantPayloadJson`.
- Checkout sender én draft til `POST /api/merchant-orders` og følger API'ets `paymentRedirectUrl`.
- Demoen kalder ikke Vipps/MobilePay eller merchantens ordrekø direkte.
