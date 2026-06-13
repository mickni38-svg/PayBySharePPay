# UC-08-S07 — Håndter PaymentRedirectUrl i betaling

**Use Case:** [UC-08 — Bestil via Merchant-link](../../usecases/UC-08-bestil-via-merchant-link.md)  
**Type:** Gap-story (G3)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som en deltager der har indsendt sin bestilling  
Vil jeg automatisk blive sendt videre til betalingssiden (fx MobilePay)  
Så jeg kan gennemføre min betaling med det samme.

---

## Baggrund

`POST /api/merchant-orders` returnerer en `PaymentRedirectUrl` i `MerchantOrderDraftDto`. Denne URL peger på payment provider (fx MobilePay). Merchant Demo viser kun en bekræftelsesbesked — redirect-URL'en bruges ikke til at sende deltager videre til betaling (G3 i UC-08).

---

## Acceptkriterier

- [ ] Når `POST /api/merchant-orders` returnerer en `PaymentRedirectUrl`, videresendes deltager automatisk til denne URL.
- [ ] Hvis `PaymentRedirectUrl` er null eller tom, vises en bekræftelsesbesked i stedet.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| Merchant Demo (HTML/JS) | Tjek `paymentRedirectUrl` i response — redirect hvis til stede |

---

## Relaterede stories

- [UC-08-S02 — Indsend bestilling fra merchant-siden](UC-08-S02-indsend-bestilling-fra-merchant-siden.md)
