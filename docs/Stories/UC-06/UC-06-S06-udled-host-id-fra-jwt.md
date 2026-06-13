# UC-06-S06 — Udled host-ID fra JWT i stedet for request-body

**Use Case:** [UC-06 — Opret Ordre](../../usecases/UC-06-opret-ordre.md)  
**Type:** Gap-story (G3)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som systemets sikkerhedsansvarlige  
Vil jeg at host-ID udledes fra JWT-tokenet og ikke fra request-body  
Så ingen bruger kan oprette en ordre på vegne af en anden.

---

## Baggrund

`POST /api/orders` modtager `createdByParticipantId` i request-body. Der valideres ikke at dette ID matcher JWT'ens `sub`-claim. En autentificeret bruger kan sætte et andet deltager-ID og oprette ordrer på andres vegne (G3 i UC-06).

---

## Acceptkriterier

- [ ] `createdByParticipantId` fjernes fra request-body.
- [ ] `OrdersController.CreateOrder()` udleder host-ID fra `User.FindFirst("sub").Value`.
- [ ] Kald med et JWT der ikke matcher et gyldigt deltager-ID returnerer `HTTP 401` eller `HTTP 404`.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `CreateOrderRequest.cs` | Fjern `CreatedByParticipantId`-felt |
| `OrdersController.CreateOrder()` | Udled `createdByParticipantId` fra JWT `sub`-claim |

---

## Relaterede stories

- [UC-06-S01 — Opret gruppeordre med merchant](UC-06-S01-opret-ordre-med-merchant.md)
