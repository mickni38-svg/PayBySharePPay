# UC-07-S06 — Begræns GET /api/orders til egne ordrer

**Use Case:** [UC-07 — Se Ordrer og Ordreoverblik](../../usecases/UC-07-se-ordrer-og-overblik.md)  
**Type:** Gap-story (G4)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟢 Lav  

---

## Beskrivelse

Som systemets sikkerhedsansvarlige  
Vil jeg at `GET /api/orders` uden filter kun returnerer ordrer tilhørende den autentificerede bruger  
Så brugere ikke kan se andres ordrer.

---

## Baggrund

`GET /api/orders` uden `participantId`-query parameter kalder `GetAll()` der returnerer samtlige ordrer i databasen. Selvom Angular-frontend altid sender `participantId`, kan enhver autentificeret klient kalde endpoint'et uden filter og få alle ordrer (G4 i UC-07).

---

## Acceptkriterier

- [ ] `GET /api/orders` uden filter returnerer kun ordrer hvor den autentificerede bruger er deltager.
- [ ] Brugerens ID udledes fra JWT `sub`-claim — ikke fra query parameter.
- [ ] En separat admin-endpoint (hvis nødvendigt) kan returnere alle ordrer med passende rollebaseret adgangskontrol.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `OrdersController` | Udled `participantId` fra JWT `sub` hvis ingen query param er angivet |
| `OrderService.GetOrdersByParticipantAsync()` | Bruges som default i stedet for `GetAll()` |

---

## Relaterede stories

- [UC-07-S01 — Se liste over egne ordrer](UC-07-S01-se-liste-over-egne-ordrer.md)
