# UC-09-S03 — Returner eksisterende reservation idempotent

**Use Case:** [UC-09 — Reserver Betaling](../../usecases/UC-09-reserver-betaling.md)  
**Type:** Alternativt forløb (A2)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som system der kalder reserve-endpoint flere gange for samme deltager  
Vil jeg at der ikke oprettes dobbelte betalinger  
Så systemet er robust over for genforsøg og parallelle kald.

---

## Acceptkriterier

- [ ] Hvis der allerede eksisterer en ikke-cancelled/ikke-fejlet `ParticipantPayment` for deltager + ordre, oprettes ingen ny.
- [ ] Den eksisterende betaling returneres med `Success = true`.
- [ ] Ingen ny `PaymentEventLog` skrives ved idempotent retur.

---

## Tekniske detaljer

- **Orchestration:** Idempotens-tjek i `GroupPaymentOrchestrationService.ReserveParticipantPaymentAsync()` — søger eksisterende `ParticipantPayment` inden oprettelse

---

## Relaterede stories

- [UC-09-S01 — Reserver betaling via Fake provider](UC-09-S01-reserver-betaling-via-fake-provider.md)
