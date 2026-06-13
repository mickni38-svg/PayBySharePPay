# UC-08-S01 — Åbn bestillingslink og se merchant-menu

**Use Case:** [UC-08 — Bestil via Merchant-link](../../usecases/UC-08-bestil-via-merchant-link.md)  
**Type:** Normalforløb (trin 1–3)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en deltager der har modtaget et bestillingslink  
Vil jeg kunne åbne linket og se merchant's menu  
Så jeg kan vælge mine varer og afgive bestilling.

---

## Acceptkriterier

- [ ] Deltager kan åbne bestillingslinket `{merchant.GroupOrderUrl}?orderId=X&merchantId=Y&participantToken=Z`.
- [ ] Merchant Demo (Pizzeria Roma) viser en menu med varer og priser.
- [ ] Siden læser `orderId`, `merchantId` og `participantToken` fra URL-parametrene.
- [ ] Siden vises uden krav om login i PayNSync.

---

## Tekniske detaljer

- **Merchant Demo:** Statisk HTML-side (Pizzeria Roma) — hardcodet menu
- **Auth:** Ingen — linket er token-baseret

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G2 | Menuen er hardcodet — ingen dynamisk menu pr. merchant | 🟡 Medium |

---

## Relaterede stories

- [UC-08-S02 — Indsend bestilling fra merchant-siden](UC-08-S02-indsend-bestilling-fra-merchant-siden.md)
- [UC-08-S06 — Understøt dynamisk menu pr. merchant (gap G2)](UC-08-S06-dynamisk-menu-pr-merchant.md)
