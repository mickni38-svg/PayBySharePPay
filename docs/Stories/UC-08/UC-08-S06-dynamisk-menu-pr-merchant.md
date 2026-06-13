# UC-08-S06 — Understøt dynamisk menu pr. merchant

**Use Case:** [UC-08 — Bestil via Merchant-link](../../usecases/UC-08-bestil-via-merchant-link.md)  
**Type:** Gap-story (G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som et merchant der er tilmeldt systemet  
Vil jeg kunne konfigurere min egen menu  
Så deltagere ser mine aktuelle varer og priser — ikke en hardcodet demo-menu.

---

## Baggrund

Merchant Demo viser altid Pizzeria Roma-menuen uanset hvilken merchant der er tilknyttet ordren. Menuen er hardcodet i en statisk HTML-fil. Der er intet API til at hente eller administrere en dynamisk menu pr. merchant (G2 i UC-08).

---

## Acceptkriterier

- [ ] `GET /api/merchants/{id}/menu` returnerer merchants aktuelle menupunkter.
- [ ] Merchant Demo henter menuen dynamisk fra API ved sidelads.
- [ ] Merchants kan administrere deres menu via en dedikeret side i appen.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `MerchantsController` (nyt) | Tilføj `GET /api/merchants/{id}/menu` endpoint |
| Database | Tilføj `MenuItem`-entitet med `MerchantId`, `Name`, `Price`, `Description` |
| Merchant Demo (HTML) | Hent menu dynamisk fra API |

---

## Relaterede stories

- [UC-08-S01 — Åbn bestillingslink og se merchant-menu](UC-08-S01-aabn-bestillingslink-og-se-menu.md)
