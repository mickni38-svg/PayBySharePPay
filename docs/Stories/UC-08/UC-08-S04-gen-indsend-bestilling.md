# UC-08-S04 — Gen-indsend bestilling

**Use Case:** [UC-08 — Bestil via Merchant-link](../../usecases/UC-08-bestil-via-merchant-link.md)  
**Type:** Alternativt forløb (A2)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en deltager der ønsker at rette sin bestilling  
Vil jeg kunne indsende en ny bestilling der erstatter min tidligere  
Så mine varer opdateres uden at hele ordren skal genskabes.

---

## Acceptkriterier

- [ ] Hvis deltager indsender en ny bestilling via bestillingslinket, slettes den eksisterende draft for samme deltager.
- [ ] En ny `MerchantOrderDraft` oprettes med de opdaterede ordrelinjer.
- [ ] `OrderParticipant.Status` forbliver `"OrderSubmitted"`.
- [ ] Ny betalingsreservation startes (eksisterende returneres idempotent hvis ikke fejlet).

---

## Tekniske detaljer

- **Service:** `MerchantOrderService.InitOrderAsync()` — sletter eksisterende draft før oprettelse af ny
- Re-submit sker via samme `POST /api/merchant-orders`-endpoint

---

## Relaterede stories

- [UC-08-S02 — Indsend bestilling fra merchant-siden](UC-08-S02-indsend-bestilling-fra-merchant-siden.md)
