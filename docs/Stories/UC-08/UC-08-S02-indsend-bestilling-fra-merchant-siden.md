# UC-08-S02 — Indsend bestilling fra merchant-siden

**Use Case:** [UC-08 — Bestil via Merchant-link](../../usecases/UC-08-bestil-via-merchant-link.md)  
**Type:** Normalforløb (trin 3–17)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en deltager der har valgt sine varer på merchant-siden  
Vil jeg kunne indsende min bestilling  
Så mine varer registreres på gruppeordren og betalingsreservationen startes.

---

## Acceptkriterier

- [ ] Deltager vælger varer og trykker "Betal" på Merchant Demo-siden.
- [ ] `POST /api/merchant-orders` sendes anonymt med `{ orderId, merchantParticipantId, participantToken, lines[], subtotalAmount, totalAmount, currency }`.
- [ ] `participantToken` valideres mod `OrderParticipant` i databasen.
- [ ] En ny `MerchantOrderDraft` med `Status = "Submitted"` oprettes.
- [ ] Alle `MerchantOrderLine`-records tildeles deltagerens `ParticipantId`.
- [ ] `OrderParticipant.Status` sættes til `"OrderSubmitted"`.
- [ ] Betalingsreservation startes via `ReserveParticipantPaymentAsync`.
- [ ] API returnerer `HTTP 201` med `MerchantOrderDraftDto` inkl. `PaymentRedirectUrl`.
- [ ] Merchant Demo viser en bekræftelsesbesked.

---

## Tekniske detaljer

- **API:** `POST /api/merchant-orders` (`MerchantOrdersController.InitOrder()`) — `[AllowAnonymous]`
- **Service:** `MerchantOrderService.InitOrderAsync()`
- **Token-validering:** EF Core opslag på `orderId` + `participantToken`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G3 | `PaymentRedirectUrl` returneres i response men håndteres ikke af frontend/demo | 🟡 Medium |
| G4 | `participantToken` valideres ikke som GUID-format inden DB-opslag | 🟢 Lav |

---

## Relaterede stories

- [UC-08-S01 — Åbn bestillingslink og se merchant-menu](UC-08-S01-aabn-bestillingslink-og-se-menu.md)
- [UC-08-S03 — Automær ordre som klar til betaling når alle har bestilt](UC-08-S03-automaer-ordre-klar-til-betaling.md)
- [UC-08-S04 — Gen-indsend bestilling](UC-08-S04-gen-indsend-bestilling.md)
