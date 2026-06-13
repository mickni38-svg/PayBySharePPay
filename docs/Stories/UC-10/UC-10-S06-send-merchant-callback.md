# UC-10-S06 — Send merchant callback når ordre er betalt

**Use Case:** [UC-10 — Host Godkend og Capture](../../usecases/UC-10-godkend-og-capture.md)  
**Type:** Normalforløb (trin 12) / Gap-story (G4)  
**Status:** ✅ Implementeret (med kendt gap)  

---

## Beskrivelse

Som merchant der modtager gruppeordrer  
Vil jeg modtage en notifikation når alle betalinger er gennemført  
Så jeg kan behandle ordren i mit eget system.

---

## Acceptkriterier

- [ ] Når `Order.Status` sættes til `Paid`, sender `MerchantCallbackService` HTTP POST til merchant's callback-URL.
- [ ] Request-body indeholder ordredetaljer.
- [ ] Callback sendes asynkront efter at ordren er markeret som `Paid`.

---

## Tekniske detaljer

- **Service:** `MerchantCallbackService.SendCallbackAsync(order)`
- Kaldes som del af `ApproveAndCaptureAllAsync` efter alle captures

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G4 | Fejl i `SendCallbackAsync` ignoreres — ordren forbliver `Paid` selv om merchant ikke modtog callback | 🟢 Lav |

---

## Relaterede stories

- [UC-10-S01 — Host godkender og gennemfører betaling for alle deltagere](UC-10-S01-host-godkender-og-gennemfoerer-betaling.md)
