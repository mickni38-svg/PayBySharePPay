# UC-10-S01 — Host godkender og gennemfører betaling for alle deltagere

**Use Case:** [UC-10 — Host Godkend og Capture](../../usecases/UC-10-godkend-og-capture.md)  
**Type:** Normalforløb  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som host for en gruppeordre  
Vil jeg kunne godkende at alle reserverede betalinger gennemføres på én gang  
Så alle deltageres betalinger captures og ordren afsluttes.

---

## Acceptkriterier

- [ ] Host kan trykke "Godkend og betal" på `/orders/{id}` — knappen vises kun for host.
- [ ] `POST /api/orders/{id}/approve` sendes med `{ requestingParticipantId }`.
- [ ] Systemet validerer at `requestingParticipantId == order.CreatedByParticipantId`.
- [ ] `Order.Status` sættes til `HostApproved`, derefter `Capturing`.
- [ ] Alle `ParticipantPayment`-records med `Status = Reserved` captures sekventielt.
- [ ] State-flow pr. betaling: `Reserved → CapturePending → Captured`.
- [ ] `PaymentEventLog`-record skrives ved hvert state-skift.
- [ ] Når alle er captured: `Order.Status = "Paid"`.
- [ ] `MerchantCallbackService` sender HTTP POST til merchant.
- [ ] API returnerer `HTTP 200` med `ApproveAndCaptureResult { allCaptured: true, orderStatus: "Paid" }`.

---

## Tekniske detaljer

- **API:** `POST /api/orders/{id}/approve` (`OrdersController.ApproveOrder()`) — JWT-beskyttet
- **Orchestration:** `GroupPaymentOrchestrationService.ApproveAndCaptureAllAsync()`
- **Capture:** Sekventiel (ikke parallel) for at undgå race conditions på `RowVersion`
- **Idempotency-key:** `"capture-{paymentId}-{orderId}"`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | `requestingParticipantId` sendes i body — valideres ikke mod JWT `sub` | 🔴 Høj |
| G2 | `UnauthorizedAccessException` mappes ikke til `HTTP 403` — returnerer `HTTP 500` | 🔴 Høj |

---

## Relaterede stories

- [UC-10-S02 — Retry capture efter delvis fejl](UC-10-S02-retry-capture-efter-delvis-fejl.md)
- [UC-10-S03 — Håndter capture-fejl fra payment provider](UC-10-S03-haandter-capture-fejl.md)
- [UC-10-S05 — Udled host-ID fra JWT ved godkendelse (gap G1+G2)](UC-10-S05-udled-host-id-fra-jwt.md)
