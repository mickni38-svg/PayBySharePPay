# UC-05-S01 — Søg efter deltagere i kataloget

**Use Case:** [UC-05 — Find Deltagere og Tilføj Ven](../../usecases/UC-05-find-deltagere-tilfoj-ven.md)  
**Type:** Normalforløb (trin 1–8)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind bruger  
Vil jeg kunne søge efter andre brugere og spisestedet ved navn, handle eller e-mail  
Så jeg kan finde de rigtige personer at tilføje som venner.

---

## Acceptkriterier

- [ ] Brugeren kan navigere til `/find-participants` og se et søgefelt.
- [ ] Ved sideload hentes alle deltagere via `GET /api/directory/search?query=&excludeFriendsOf={id}`.
- [ ] Eksisterende venner ekskluderes automatisk fra søgeresultaterne.
- [ ] Brugeren kan skrive i søgefeltet og trykke søg for at hente filtrerede resultater.
- [ ] Søgeresultater vises opdelt i tabs: **Venner** / **Brugere** / **Spisestedet**.

---

## Tekniske detaljer

- **API:** `GET /api/directory/search?query={term}&excludeFriendsOf={id}` (`DirectoryController`)
- **Service:** `DirectoryService.search(query, currentUserId)`
- **Frontend:** `ngOnInit()` kalder `load('')` og `loadFriends()` parallelt

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | `GET /api/directory/search` har ingen `[Authorize]` — enhver kan søge i kataloget | 🔴 Høj |

---

## Relaterede stories

- [UC-05-S02 — Vis og filtrer søgeresultater client-side](UC-05-S02-filtrer-soegeresultater-client-side.md)
- [UC-05-S03 — Markér og tilføj venner](UC-05-S03-marker-og-tilfoj-venner.md)
- [UC-05-S07 — Beskyt venne- og katalog-endpoints med autorisation](UC-05-S07-beskyt-endpoints-autorisation.md)
