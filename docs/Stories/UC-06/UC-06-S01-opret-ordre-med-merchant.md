# UC-06-S01 — Opret gruppeordre med merchant

**Use Case:** [UC-06 — Opret Ordre](../../usecases/UC-06-opret-ordre.md)  
**Type:** Normalforløb — Med merchant  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind host  
Vil jeg kunne oprette en gruppeordre og tilknytte et spisested  
Så mine deltagere automatisk modtager et personligt bestillingslink til spisestedet.

---

## Acceptkriterier

- [ ] Host kan navigere til `/orders/create` og se en 4-trins wizard: **Titel → Merchant → Deltagere → Opret**.
- [ ] Host kan vælge en merchant fra kataloget i wizard-trin 2.
- [ ] `POST /api/orders` kaldes med `{ createdByParticipantId, title, category?, message?, merchantParticipantId, participantIds[] }`.
- [ ] `Order` oprettes med status `Collecting` og et unikt `JoinToken`.
- [ ] Host tilføjes som `OrderParticipant` med status `Accepted` og unikt `ParticipantToken`.
- [ ] Alle inviterede deltagere tilføjes med status `Invited` og unikt `ParticipantToken`.
- [ ] Hvert bestillingslink konstrueres som `{merchant.GroupOrderUrl}?orderId={id}&merchantId={merchantId}&participantToken={token}`.
- [ ] En `Message`-record med bestillingslinket sendes til **hver** deltager inkl. host.
- [ ] API returnerer `HTTP 201` og host navigeres til `/orders/{id}`.

---

## Tekniske detaljer

- **API:** `POST /api/orders` (`OrdersController.CreateOrder()`) — JWT-beskyttet
- **Service:** `OrderService.CreateOrderAsync()`
- **Bestillingslink:** konstrueres server-side, gemmes som `Message`-record
- **Token:** `Guid.NewGuid().ToString("N")`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G3 | `createdByParticipantId` sendes i body — ikke udledt fra JWT `sub` | 🔴 Høj |
| G4 | `GroupOrderUrl` kan være null — fallback til localhost demo-URL | 🟡 Medium |

---

## Relaterede stories

- [UC-06-S02 — Opret gruppeordre uden merchant](UC-06-S02-opret-ordre-uden-merchant.md)
- [UC-06-S03 — Tilføj deltagere til ordre ved oprettelse](UC-06-S03-tilfoj-deltagere-ved-oprettelse.md)
- [UC-06-S06 — Udled host-ID fra JWT i stedet for request-body (gap G3)](UC-06-S06-udled-host-id-fra-jwt.md)
