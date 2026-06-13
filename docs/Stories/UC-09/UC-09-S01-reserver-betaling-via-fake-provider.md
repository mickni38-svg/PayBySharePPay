# UC-09-S01 — Reserver betaling via Fake provider

**Use Case:** [UC-09 — Reserver Betaling](../../usecases/UC-09-reserver-betaling.md)  
**Type:** Normalforløb  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som system (kaldt fra UC-08 ved merchant-bestilling)  
Vil jeg kunne reservere en deltagers betaling via Fake payment provider  
Så betalingen registreres og ordreflowet kan fortsætte.

---

## Acceptkriterier

- [ ] `POST /api/orders/{id}/reserve` modtager `{ participantId, amountMinorUnits, currency, returnUrl, callbackUrl }`.
- [ ] Idempotens-tjek forhindrer dobbelt-reservation: eksisterende ikke-cancelled betaling returneres.
- [ ] `ParticipantPayment` oprettes med status `Created`.
- [ ] State-flow gennemføres: `Created → ReservationStarted → Reserved`.
- [ ] `PaymentEventLog`-record skrives ved hvert state-skift.
- [ ] `FakePaymentProvider` returnerer synkront `Success = true` og `ProviderPaymentId = "FAKE-..."`.
- [ ] API returnerer `HTTP 200` med `ReserveParticipantPaymentResult`.

---

## Tekniske detaljer

- **API:** `POST /api/orders/{id}/reserve` (`OrdersController.ReservePayment()`) — JWT-beskyttet
- **Orchestration:** `GroupPaymentOrchestrationService.ReserveParticipantPaymentAsync()`
- **State service:** `ParticipantPaymentStateService`
- **Provider:** `FakePaymentProvider` — synkron success
- **Concurrency:** `RowVersion` på `ParticipantPayment` (optimistisk concurrency)

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | `participantId` sendes i request-body — valideres ikke mod JWT `sub`-claim | 🔴 Høj |

---

## Relaterede stories

- [UC-09-S02 — Reserver betaling via MobilePay Sandbox](UC-09-S02-reserver-betaling-via-mobilepay.md)
- [UC-09-S03 — Returner eksisterende reservation idempotent](UC-09-S03-idempotent-reservation.md)
- [UC-09-S05 — Valider participantId mod JWT ved reservation (gap G1)](UC-09-S05-valider-participantid-mod-jwt.md)
