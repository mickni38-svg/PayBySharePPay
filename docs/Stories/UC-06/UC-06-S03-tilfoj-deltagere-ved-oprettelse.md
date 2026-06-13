# UC-06-S03 — Tilføj deltagere til ordre ved oprettelse

**Use Case:** [UC-06 — Opret Ordre](../../usecases/UC-06-opret-ordre.md)  
**Type:** Normalforløb (trin 5 + 13–14)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en host der opretter en gruppeordre  
Vil jeg kunne invitere venner og andre deltagere direkte ved oprettelse  
Så de automatisk tilføjes til ordren med en invitation.

---

## Acceptkriterier

- [ ] Host kan vælge deltagere fra venneliste eller katalog i wizard-trin 3.
- [ ] Valgte deltager-IDs sendes som `participantIds[]` i `POST /api/orders`.
- [ ] Hvert deltager-ID valideres — returnerer `HTTP 404` hvis ID ikke eksisterer.
- [ ] Alle inviterede deltagere tilføjes som `OrderParticipant` med status `Invited` og unikt `ParticipantToken`.
- [ ] Host tilføjes altid som `OrderParticipant` med status `Accepted`.

---

## Tekniske detaljer

- **Service:** `OrderService.CreateOrderAsync()` itererer over `participantIds`
- **Token:** `Guid.NewGuid().ToString("N")` pr. deltager

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G2 | Ingen endpoint til at tilføje deltagere **efter** oprettelse | 🟡 Medium |

---

## Relaterede stories

- [UC-06-S01 — Opret gruppeordre med merchant](UC-06-S01-opret-ordre-med-merchant.md)
- [UC-06-S07 — Tilføj deltagere til eksisterende ordre (gap G2)](UC-06-S07-tilfoj-deltagere-til-eksisterende-ordre.md)
