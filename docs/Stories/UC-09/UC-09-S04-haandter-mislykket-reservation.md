# UC-09-S04 — Håndter mislykket betalingsreservation

**Use Case:** [UC-09 — Reserver Betaling](../../usecases/UC-09-reserver-betaling.md)  
**Type:** Undtagelsesforløb (E1 + E2)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som system der forsøger at reservere en betaling  
Vil jeg at fejl fra payment provider håndteres korrekt og logges  
Så ordreflowet ikke hænger i en uafklaret tilstand.

---

## Acceptkriterier

- [ ] Hvis provider returnerer `Success = false`, sættes `ParticipantPayment.Status` til `ReservationFailed`.
- [ ] En `PaymentEventLog`-record skrives med `ErrorCode` og `ErrorMessage`.
- [ ] API returnerer `HTTP 400` med `{ ErrorCode, ErrorMessage }`.
- [ ] Hvis provider kaster en uventet exception, håndteres den og status sættes til `ReservationFailed`.

---

## Tekniske detaljer

- **State-overgang:** `ReservationStarted → ReservationFailed`
- **Service:** `ParticipantPaymentStateService.SetReservationFailedAsync()`
- **EventLog:** Immutable audit trail med fejlkode

---

## Relaterede stories

- [UC-09-S01 — Reserver betaling via Fake provider](UC-09-S01-reserver-betaling-via-fake-provider.md)
- [UC-09-S02 — Reserver betaling via MobilePay Sandbox](UC-09-S02-reserver-betaling-via-mobilepay.md)
