# UC-05-S07 — Beskyt venne- og katalog-endpoints med autorisation

**Use Case:** [UC-05 — Find Deltagere og Tilføj Ven](../../usecases/UC-05-find-deltagere-tilfoj-ven.md)  
**Type:** Gap-story (G1 + G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som systemets sikkerhedsansvarlige  
Vil jeg at alle venne- og katalog-endpoints kræver et gyldigt JWT og validerer ejerskab  
Så uvedkommende ikke kan læse vendelister eller oprette vennerelationer på vegne af andre brugere.

---

## Baggrund

`FriendsController` og `DirectoryController` har ingen `[Authorize]`-attribut. Enhver uautentificeret klient kan hente vendelister og tilføje venner for en vilkårlig bruger. Derudover valideres `initiatorId` i `POST /api/friends` ikke mod JWT'ens `sub`-claim (G1 + G2 i UC-05).

---

## Acceptkriterier

- [ ] `GET /api/directory/search` kræver gyldigt JWT (`[Authorize]`).
- [ ] `GET /api/friends/{participantId}` kræver gyldigt JWT.
- [ ] `POST /api/friends` kræver gyldigt JWT.
- [ ] `POST /api/friends` validerer at JWT'ens `sub`-claim matcher `initiatorId` i request-body.
- [ ] Uautoriserede kald returnerer `HTTP 401 Unauthorized`.
- [ ] Kald med mismatching `initiatorId` returnerer `HTTP 403 Forbidden`.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `FriendsController` | Tilføj `[Authorize]` på klasse-niveau |
| `DirectoryController` | Tilføj `[Authorize]` på klasse-niveau |
| `FriendsController.AddFriend()` | Valider `User.FindFirst("sub").Value == dto.InitiatorId.ToString()` |

---

## Relaterede stories

- [UC-05-S03 — Markér og tilføj venner](UC-05-S03-marker-og-tilfoj-venner.md)
- [UC-05-S04 — Vis venneliste](UC-05-S04-vis-venneliste.md)
