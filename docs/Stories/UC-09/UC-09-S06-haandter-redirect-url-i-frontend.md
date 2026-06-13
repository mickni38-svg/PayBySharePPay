# UC-09-S06 — Håndter RedirectUrl til MobilePay i frontend

**Use Case:** [UC-09 — Reserver Betaling](../../usecases/UC-09-reserver-betaling.md)  
**Type:** Gap-story (G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som en deltager der betaler via MobilePay/Vipps  
Vil jeg automatisk blive sendt videre til Vipps-appen efter reservation  
Så jeg kan godkende betalingen uden manuel navigation.

---

## Baggrund

`POST /api/orders/{id}/reserve` returnerer en `RedirectUrl` til Vipps-appen i `ReserveParticipantPaymentResult`. Angular-frontend bruger denne URL ikke — betalingsstatus vises kun i ordreoverblikket. Brugeren sendes ikke automatisk videre til MobilePay-appen (G2 i UC-09).

---

## Acceptkriterier

- [ ] Når `RedirectUrl` er til stede i reserve-response, navigeres brugeren automatisk til URL'en.
- [ ] Hvis `RedirectUrl` er null (Fake provider), vises en bekræftelsesbesked i stedet.
- [ ] Efter retur fra Vipps-appen vises opdateret betalingsstatus i ordreoverblikket.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| Angular ordre-komponent | Tjek `redirectUrl` i reserve-response og naviger via `window.location.href` |

---

## Relaterede stories

- [UC-09-S02 — Reserver betaling via MobilePay Sandbox](UC-09-S02-reserver-betaling-via-mobilepay.md)
