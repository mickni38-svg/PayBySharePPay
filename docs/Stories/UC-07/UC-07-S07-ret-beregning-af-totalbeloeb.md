# UC-07-S07 — Ret beregning af totalbeløb på ordre

**Use Case:** [UC-07 — Se Ordrer og Ordreoverblik](../../usecases/UC-07-se-ordrer-og-overblik.md)  
**Type:** Gap-story (G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som en bruger der ser et ordreoverblik  
Vil jeg se det korrekte samlede beløb for ordren  
Så jeg har et præcist overblik over hvad ordren koster i alt.

---

## Baggrund

`totalAmount` i `OrderOverviewDto` og `OrderSummaryDto` beregnes som `draft?.TotalAmount ?? 0m` — kun fra det første `MerchantOrderDraft`. Hvis der er drafts fra flere deltagere med individuelle beløb, er totalbeløbet forkert (G2 i UC-07).

---

## Acceptkriterier

- [ ] `totalAmount` summerer beløb fra **alle** `MerchantOrderDraft`-records tilknyttet ordren.
- [ ] Eller: `totalAmount` summeres fra `ParticipantPaymentSummaryDto` pr. deltager.
- [ ] Ændringen påvirker både `OrderSummaryDto` og `OrderOverviewDto`.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `OrderService.GetOrderOverviewAsync()` | Summer `TotalAmount` fra alle drafts i stedet for kun første |
| `OrderService.GetOrdersByParticipantAsync()` | Samme fix i summary-beregning |

---

## Relaterede stories

- [UC-07-S02 — Se ordreoverblik for én ordre](UC-07-S02-se-ordreoverblik.md)
