# UC-19: Samlet merchant-ordrekontrakt

## Status

Planlagt.

## Formål

Definere den standardordre, som PayNSync opbygger efter en gennemført gruppebetaling.

## Brugerhistorie

Som merchant vil jeg modtage én samlet og betalt ordre, så køkkenet ikke skal håndtere deltagernes individuelle ordrekladder.

## Funktionelt scope

- Saml ordrelinjer fra alle deltagere.
- Medtag hostens navn, telefonnummer og leveringsadresse som ordre-snapshot.
- Medtag totalbeløb, valuta, betalingsstatus og PayNSync-ordrenummer.
- Udelad deltagernes identitet og betalingsreferencer.
- Bevar separate ordrelinjer, når tilvalg eller bemærkninger er forskellige.
- Gør standardordren anvendelig direkte i PayNSync Order Hub.

## Acceptkriterier

- Kontrakten repræsenterer én færdig merchant-ordre.
- Ordren oprettes først efter succesfuld capture.
- Hostens kontakt- og leveringsoplysninger følger ordren som snapshot.
- Summen af ordrelinjerne stemmer med den gennemførte betaling.
- En gennemført gruppebetaling opretter kun én merchant-ordre.

## Ikke i scope

- Merchant-specifik JSON-mapping.
- Visning af ordren hos merchant.
