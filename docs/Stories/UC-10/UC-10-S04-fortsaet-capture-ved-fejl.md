# UC-10-S04 — Fortsæt capture for alle deltagere selvom én fejler

**Use Case:** [UC-10 — Host Godkend og Capture](../../usecases/UC-10-godkend-og-capture.md)  
**Type:** Gap-story (G3)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som host der kører capture  
Vil jeg at systemet forsøger at capture alle deltageres betalinger selv om én fejler  
Så så mange betalinger som muligt gennemføres i ét kald.

---

## Baggrund

Capture-loopét i `ApproveAndCaptureAllAsync` afbrydes ved første `CaptureFailed`. De resterende deltagers reserverede betalinger forsøges ikke i det kald. Host skal kalde `/approve` igen for at forsøge de resterende (G3 i UC-10).

---

## Acceptkriterier

- [ ] Capture-loopét fortsætter til alle `Reserved`-betalinger er forsøgt, selv om en eller flere fejler.
- [ ] Hvert fejlresultat logges i `PaymentEventLog`.
- [ ] `ApproveAndCaptureResult` indeholder status pr. deltager.
- [ ] `Order.Status` sættes til `PartiallyFailed` hvis mindst én fejlede, `Paid` hvis alle lykkedes.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `GroupPaymentOrchestrationService.ApproveAndCaptureAllAsync()` | Fjern `break` ved `CaptureFailed` — brug `continue` eller saml fejl til sidst |

---

## Relaterede stories

- [UC-10-S02 — Retry capture efter delvis fejl](UC-10-S02-retry-capture-efter-delvis-fejl.md)
- [UC-10-S03 — Håndter capture-fejl fra payment provider](UC-10-S03-haandter-capture-fejl.md)
