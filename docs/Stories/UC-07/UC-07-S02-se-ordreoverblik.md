# UC-07-S02 — Se ordreoverblik for én ordre

**Use Case:** [UC-07 — Se Ordrer og Ordreoverblik](../../usecases/UC-07-se-ordrer-og-overblik.md)  
**Type:** Normalforløb — Ordreoverblik  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind bruger  
Vil jeg se det fulde overblik for en specifik ordre  
Så jeg kan se deltagere, ordrelinjer, betalingsstatus og beskeder.

---

## Acceptkriterier

- [ ] Brugeren kan klikke på en ordre og navigere til `/orders/{id}`.
- [ ] `GET /api/orders/{id}/overview` henter det fulde overblik.
- [ ] Siden viser: titel, status, merchant, deltagerliste med betalingsstatus, ordrelinjer pr. deltager og host-handlingsknapper.
- [ ] Siden returnerer `HTTP 404` hvis ordre-ID ikke eksisterer.

---

## Tekniske detaljer

- **API:** `GET /api/orders/{id}/overview` — JWT-beskyttet
- **Service:** `OrderService.GetOrderOverviewAsync(id)`
- **Response:** `OrderOverviewDto` med deltagere, betalinger, beskeder, ordrelinjer og `ParticipantPaymentSummaryDto`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | `GetOrderOverviewAsync` synkroniserer `OrderParticipant.Status` og kalder `SaveChangesAsync` — write-operation i GET | 🟡 Medium |
| G2 | `totalAmount` beregnes kun fra første draft — kan være forkert ved flere drafts | 🟡 Medium |

---

## Relaterede stories

- [UC-07-S01 — Se liste over egne ordrer](UC-07-S01-se-liste-over-egne-ordrer.md)
- [UC-07-S03 — Se capture-status for ordre](UC-07-S03-se-capture-status.md)
- [UC-07-S04 — Flyt status-synkronisering ud af GET-endpoint (gap G1)](UC-07-S04-flyt-status-synkronisering-ud-af-get.md)
