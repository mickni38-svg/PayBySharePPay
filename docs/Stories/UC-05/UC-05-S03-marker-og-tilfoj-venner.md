# UC-05-S03 — Markér og tilføj venner

**Use Case:** [UC-05 — Find Deltagere og Tilføj Ven](../../usecases/UC-05-find-deltagere-tilfoj-ven.md)  
**Type:** Normalforløb (trin 9–19)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der har fundet én eller flere deltagere  
Vil jeg kunne markere dem og tilføje dem alle som venner på én gang  
Så jeg hurtigt kan opbygge min venneliste.

---

## Acceptkriterier

- [ ] Brugeren kan klikke på en eller flere deltagere for at markere dem (`toggleSelect()`).
- [ ] Valgte deltagere fremhæves visuelt i listen.
- [ ] Knappen "Tilføj" sender `POST /api/friends` for hver valgt deltager.
- [ ] API returnerer `HTTP 204 No Content` ved succes.
- [ ] Tilføjede brugere fjernes automatisk fra søgeresultatlisten efter tilføjelse.

---

## Tekniske detaljer

- **API:** `POST /api/friends` med `{ initiatorId, receiverId }` (`FriendsController.AddFriend()`)
- **Service:** `ParticipantService.AddFriendAsync()`
- **Frontend:** `addSelected()` sender parallelle POST-kald for hvert valgt element

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | `POST /api/friends` har ingen `[Authorize]` | 🔴 Høj |
| G2 | `initiatorId` valideres ikke mod JWT `sub`-claim | 🔴 Høj |
| G6 | Ingen batch-endpoint — ét HTTP-kald per valgt bruger | 🟢 Lav |

---

## Relaterede stories

- [UC-05-S01 — Søg efter deltagere i kataloget](UC-05-S01-soeg-efter-deltagere-i-kataloget.md)
- [UC-05-S04 — Vis venneliste](UC-05-S04-vis-venneliste.md)
- [UC-05-S05 — Håndter duplikat venskab](UC-05-S05-haandter-duplikat-venskab.md)
- [UC-05-S07 — Beskyt venne- og katalog-endpoints med autorisation](UC-05-S07-beskyt-endpoints-autorisation.md)
