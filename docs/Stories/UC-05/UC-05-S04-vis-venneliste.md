# UC-05-S04 — Vis venneliste

**Use Case:** [UC-05 — Find Deltagere og Tilføj Ven](../../usecases/UC-05-find-deltagere-tilfoj-ven.md)  
**Type:** Alternativt forløb (A1)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind bruger  
Vil jeg se mine eksisterende venner når jeg åbner find-deltagere-siden  
Så jeg hurtigt kan se hvem jeg allerede er forbundet med.

---

## Acceptkriterier

- [ ] Tab **Venner** vises som default når siden åbnes.
- [ ] Vennelisten hentes ved sideload via `GET /api/friends/{id}`.
- [ ] Venner vises opdelt i personer og spisestedet under Venner-tabben.
- [ ] Der er ingen søgefunktion på vennelisten — den er fast.

---

## Tekniske detaljer

- **API:** `GET /api/friends/{participantId}` → returnerer `ParticipantDto[]`
- **Frontend:** `FriendService.getFriends(currentUserId)` kaldet i `loadFriends()`
- **Alternativt:** `GET /api/directory/{id}/friends` bruges af `DirectoryService.GetFriendsAsync()` — to overlappende endpoints

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | `GET /api/friends/{id}` har ingen `[Authorize]` — enhver kan hente enhver brugers venneliste | 🔴 Høj |

---

## Relaterede stories

- [UC-05-S01 — Søg efter deltagere i kataloget](UC-05-S01-soeg-efter-deltagere-i-kataloget.md)
- [UC-05-S03 — Markér og tilføj venner](UC-05-S03-marker-og-tilfoj-venner.md)
