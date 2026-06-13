# UC-10-S02 — Retry capture efter delvis fejl

**Use Case:** [UC-10 — Host Godkend og Capture](../../usecases/UC-10-godkend-og-capture.md)  
**Type:** Alternativt forløb (A1)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som host hvis capture delvist fejlede  
Vil jeg kunne forsøge igen ved at trykke "Godkend og betal" på ny  
Så de resterende betalinger også gennemføres.

---

## Acceptkriterier

- [ ] Host kan kalde `POST /api/orders/{id}/approve` igen når ordren er i status `PartiallyFailed`.
- [ ] Allerede captured betalinger springes over (idempotent — kun `Status = Reserved` behandles).
- [ ] Flowet fortsætter for de resterende reserverede betalinger.
- [ ] Når alle er captured: `Order.Status = "Paid"`.

---

## Tekniske detaljer

- **Tilladte statuser for retry:** `ReadyToPay`, `HostApproved`, `Capturing`, `PartiallyFailed`
- **Idempotens:** Kun `ParticipantPayment` med `Status = Reserved` behandles — `Captured` springes over

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G3 | Capture-loop stopper ved første fejl — resterende deltagere forsøges ikke i samme kald | 🟡 Medium |

---

## Relaterede stories

- [UC-10-S01 — Host godkender og gennemfører betaling for alle deltagere](UC-10-S01-host-godkender-og-gennemfoerer-betaling.md)
- [UC-10-S03 — Håndter capture-fejl fra payment provider](UC-10-S03-haandter-capture-fejl.md)
- [UC-10-S04 — Fortsaet capture for alle deltagere ved fejl (gap G3)](UC-10-S04-fortsaet-capture-ved-fejl.md)
