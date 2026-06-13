# UC-10-S03 — Håndter capture-fejl fra payment provider

**Use Case:** [UC-10 — Host Godkend og Capture](../../usecases/UC-10-godkend-og-capture.md)  
**Type:** Undtagelsesforløb (E4)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som system der forsøger at capture betalinger  
Vil jeg at fejl fra payment provider håndteres korrekt  
Så ordren ikke hænger i en uafklaret tilstand og host kan retry.

---

## Acceptkriterier

- [ ] Hvis provider returnerer fejl ved capture, sættes `ParticipantPayment.Status` til `CaptureFailed`.
- [ ] `PaymentEventLog`-record skrives med fejlkode.
- [ ] `Order.Status` sættes til `PartiallyFailed`.
- [ ] API returnerer `ApproveAndCaptureResult { allCaptured: false, orderStatus: "PartiallyFailed" }`.
- [ ] Hvis provider kaster en uventet exception, håndteres den og status sættes til `CaptureFailed`.

---

## Tekniske detaljer

- **State-overgang:** `CapturePending → CaptureFailed`
- **Service:** `ParticipantPaymentStateService.SetCaptureFailedAsync()`
- **Loop-adfærd:** Capture-loop afbrydes ved første fejl

---

## Relaterede stories

- [UC-10-S01 — Host godkender og gennemfører betaling for alle deltagere](UC-10-S01-host-godkender-og-gennemfoerer-betaling.md)
- [UC-10-S02 — Retry capture efter delvis fejl](UC-10-S02-retry-capture-efter-delvis-fejl.md)
