# UC-10-S05 — Udled host-ID fra JWT ved godkendelse

**Use Case:** [UC-10 — Host Godkend og Capture](../../usecases/UC-10-godkend-og-capture.md)  
**Type:** Gap-story (G1 + G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som systemets sikkerhedsansvarlige  
Vil jeg at host-tjekket baseres på JWT'ens `sub`-claim og at uautoriserede kald returnerer `HTTP 403`  
Så ingen kan godkende en andens ordre og fejlbeskeder er korrekte.

---

## Baggrund

`POST /api/orders/{id}/approve` modtager `requestingParticipantId` i request-body. Host-validering sker ved at sammenligne dette ID med `order.CreatedByParticipantId` — JWT `sub`-claim bruges ikke. Derudover kastes `UnauthorizedAccessException` men `ExceptionHandlingMiddleware` mapper den ikke, så klienten modtager `HTTP 500` i stedet for `HTTP 403` (G1 + G2 i UC-10).

---

## Acceptkriterier

- [ ] `requestingParticipantId` fjernes fra request-body.
- [ ] `OrdersController.ApproveOrder()` udleder host-ID fra `User.FindFirst("sub").Value`.
- [ ] Hvis JWT-ID ikke matcher `order.CreatedByParticipantId`, returneres `HTTP 403 Forbidden`.
- [ ] `ExceptionHandlingMiddleware` mapper `UnauthorizedAccessException` til `HTTP 403`.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `ApproveOrderRequest.cs` | Fjern `RequestingParticipantId`-felt |
| `OrdersController.ApproveOrder()` | Udled `requestingParticipantId` fra JWT `sub`-claim |
| `ExceptionHandlingMiddleware.cs` | Tilføj mapping: `UnauthorizedAccessException` → `HTTP 403` |

---

## Relaterede stories

- [UC-10-S01 — Host godkender og gennemfører betaling for alle deltagere](UC-10-S01-host-godkender-og-gennemfoerer-betaling.md)
- [UC-08-S05 — Håndter ugyldigt participant-token](../UC-08/UC-08-S05-haandter-ugyldigt-participant-token.md)
