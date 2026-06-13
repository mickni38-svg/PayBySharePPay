# UC-07-S01 — Se liste over egne ordrer

**Use Case:** [UC-07 — Se Ordrer og Ordreoverblik](../../usecases/UC-07-se-ordrer-og-overblik.md)  
**Type:** Normalforløb — Ordreliste  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind bruger  
Vil jeg se en liste over alle mine gruppeordrer  
Så jeg hurtigt kan få overblik over aktive og afsluttede ordrer.

---

## Acceptkriterier

- [ ] Brugeren kan navigere til `/orders` og se en liste over sine ordrer.
- [ ] `GET /api/orders?participantId={currentUserId}` henter ordrer hvor brugeren er deltager.
- [ ] Listen vises opdelt i to tabs: **Aktive** og **Afsluttede**.
- [ ] **Aktive** inkluderer: `Collecting`, `ReadyToPay`, `HostApproved`, `Capturing`, `PartiallyFailed`.
- [ ] **Afsluttede** inkluderer: `Paid`, `Completed`, `Cancelled`.
- [ ] Hvert listeelement viser: titel, status, merchant-navn og totalbeløb.

---

## Tekniske detaljer

- **API:** `GET /api/orders?participantId={id}` — JWT-beskyttet
- **Service:** `OrderService.GetOrdersByParticipantAsync(participantId)`
- **Response:** `IEnumerable<OrderSummaryDto>`
- **Filtrering:** Sker client-side på statusværdi

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G4 | `GET /api/orders` uden filter returnerer **alle** ordrer i databasen | 🟢 Lav |

---

## Relaterede stories

- [UC-07-S02 — Se ordreoverblik for én ordre](UC-07-S02-se-ordreoverblik.md)
- [UC-07-S06 — Begræns GET /api/orders til egne ordrer (gap G4)](UC-07-S06-begraens-ordreliste-til-egne-ordrer.md)
