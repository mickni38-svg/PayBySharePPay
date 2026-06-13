# UC-07-S04 — Flyt status-synkronisering ud af GET-endpoint

**Use Case:** [UC-07 — Se Ordrer og Ordreoverblik](../../usecases/UC-07-se-ordrer-og-overblik.md)  
**Type:** Gap-story (G1)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som udvikler  
Vil jeg at GET-endpoints ikke foretager skriveoperationer til databasen  
Så endpoints opfører sig forudsigeligt og sideeffektfrit.

---

## Baggrund

`GetOrderOverviewAsync` synkroniserer `OrderParticipant.Status` til `Paid` og kalder `SaveChangesAsync` som del af et GET-kald. Dette bryder HTTP-semantikken for GET (som bør være idempotent og sideeffektfri) og kan skabe uventede bieffekter ved gentagne kald (G1 i UC-07).

---

## Acceptkriterier

- [ ] `GetOrderOverviewAsync` udfører ingen skriveoperationer til databasen.
- [ ] Status-synkronisering flyttes til en separat service-metode der kaldes eksplicit (fx ved betaling eller via en dedikeret `PATCH`-operation).
- [ ] `GET /api/orders/{id}/overview` returnerer samme data som før men uden at mutere databasen.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `OrderService.GetOrderOverviewAsync()` | Fjern `SaveChangesAsync()`-kald og status-mutation |
| Nyt kald-sted | Tilføj eksplicit synkronisering ved betalingsregistrering |

---

## Relaterede stories

- [UC-07-S02 — Se ordreoverblik for én ordre](UC-07-S02-se-ordreoverblik.md)
