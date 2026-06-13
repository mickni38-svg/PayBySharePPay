# UC-05-S08 — Implementer ven-anmodning med accept-flow

**Use Case:** [UC-05 — Find Deltagere og Tilføj Ven](../../usecases/UC-05-find-deltagere-tilfoj-ven.md)  
**Type:** Gap-story (G4)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som en bruger der modtager en venneanmodning  
Vil jeg kunne acceptere eller afvise den  
Så jeg selv bestemmer hvem der er på min venneliste.

---

## Baggrund

I dag oprettes venskaber øjeblikkeligt uden at modparten accepterer. Der er ingen `Pending`/`Accepted`-status på `FriendRelation`-entiteten, og modparten får ingen notifikation. Dette betyder at enhver kan tilføje hvem som helst uden samtykke (G4 i UC-05).

---

## Acceptkriterier

- [ ] `FriendRelation` udvides med en `Status`-kolonne (`Pending` / `Accepted` / `Rejected`).
- [ ] `POST /api/friends` opretter en relation med status `Pending` i stedet for direkte `Accepted`.
- [ ] Modparten kan se indkomne venneanmodninger på en dedikeret side eller notifikation.
- [ ] `PUT /api/friends/{id}/accept` accepterer anmodningen og sætter status til `Accepted`.
- [ ] `DELETE /api/friends/{id}` afviser eller fjerner en anmodning/venskab.
- [ ] Vennelisten viser kun relationer med status `Accepted`.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `FriendRelation.cs` | Tilføj `Status`-felt (enum: `Pending`, `Accepted`, `Rejected`) |
| `ParticipantService.AddFriendAsync()` | Opret relation med `Status = Pending` |
| `FriendsController` | Tilføj `PUT /{id}/accept` og `DELETE /{id}` endpoints |
| Frontend | Vis indkomne anmodninger og accept/afvis-knapper |

---

## Relaterede stories

- [UC-05-S03 — Markér og tilføj venner](UC-05-S03-marker-og-tilfoj-venner.md)
- [UC-05-S04 — Vis venneliste](UC-05-S04-vis-venneliste.md)
