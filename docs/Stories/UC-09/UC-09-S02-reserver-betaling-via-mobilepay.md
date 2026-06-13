# UC-09-S02 — Reserver betaling via MobilePay Sandbox

**Use Case:** [UC-09 — Reserver Betaling](../../usecases/UC-09-reserver-betaling.md)  
**Type:** Alternativt forløb (A1)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som deltager der betaler via MobilePay/Vipps  
Vil jeg blive sendt videre til Vipps-appen for at godkende betalingen  
Så min reservation bekræftes via den rigtige payment provider.

---

## Acceptkriterier

- [ ] `MobilePaySandboxPaymentProvider.ReserveAsync()` sender `POST /epayment/v1/payments` til Vipps Sandbox API.
- [ ] Provider returnerer en `RedirectUrl` til Vipps-appen.
- [ ] `ParticipantPayment.Status` forbliver `ReservationStarted` indtil webhook bekræfter.
- [ ] `RedirectUrl` returneres i API-response.
- [ ] Bruger kan redirectes til Vipps-appen via `RedirectUrl`.

---

## Tekniske detaljer

- **Provider:** `MobilePaySandboxPaymentProvider` — asynkron, kræver webhook-bekræftelse
- **Status-flow:** `Created → ReservationStarted` (forbliver til webhook `→ Reserved`)
- **Idempotency-key:** `"reserve-{paymentId}-{orderId}-{participantId}"`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G2 | `RedirectUrl` returneres i response men frontend-integration er ikke fuldt implementeret | 🟡 Medium |
| G3 | Fake provider sætter `Reserved` synkront — Vipps kræver webhook. Adfærd er inkonsistent mellem providers | 🟢 Lav |

---

## Relaterede stories

- [UC-09-S01 — Reserver betaling via Fake provider](UC-09-S01-reserver-betaling-via-fake-provider.md)
- [UC-09-S04 — Håndter mislykket betalingsreservation](UC-09-S04-haandter-mislykket-reservation.md)
- [UC-09-S06 — Håndter RedirectUrl til MobilePay i frontend (gap G2)](UC-09-S06-haandter-redirect-url-i-frontend.md)
