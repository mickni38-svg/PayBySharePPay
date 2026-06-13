# UC-06-S02 — Opret gruppeordre uden merchant

**Use Case:** [UC-06 — Opret Ordre](../../usecases/UC-06-opret-ordre.md)  
**Type:** Normalforløb — Uden merchant  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind host  
Vil jeg kunne oprette en gruppeordre uden at tilknytte et spisested  
Så jeg kan samle en gruppe om en betaling uden en fast merchant.

---

## Acceptkriterier

- [ ] Host kan oprette en ordre uden at vælge merchant i wizard-trin 2.
- [ ] `POST /api/orders` kaldes med `merchantParticipantId = null`.
- [ ] Inviterede deltagere (ikke host) modtager en generel invitation: *"[Creator] har inviteret dig til gruppebetaling: '[Titel]'. Åbn appen for at se detaljer."*
- [ ] Host modtager ingen invitationsbesked.
- [ ] API returnerer `HTTP 201` og host navigeres til `/orders/{id}`.

---

## Tekniske detaljer

- **Service:** `MerchantParticipantId` er null → generel invitationsbesked til inviterede
- Bestillingslink genereres **ikke** — kun tekstbesked

---

## Relaterede stories

- [UC-06-S01 — Opret gruppeordre med merchant](UC-06-S01-opret-ordre-med-merchant.md)
- [UC-06-S03 — Tilføj deltagere til ordre ved oprettelse](UC-06-S03-tilfoj-deltagere-ved-oprettelse.md)
