# UC-05 — Find Deltagere og Tilføj Ven

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-05 |
| Navn | Find Deltagere og Tilføj Ven |
| Primær aktør | Logget-ind bruger (Person) |
| Formål | Søge efter andre brugere og spisestedet i kataloget, og tilføje dem som venner |
| Trigger | Brugeren navigerer til `/find-participants` |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Person** | Bruger der søger efter og tilføjer venner |
| **API** | `Api.PayBySharePay` — leverer katalog-søgning og vennehåndtering |
| **Database** | SQL Server — `Participant` og `FriendRelation`-tabeller |

---

## Prækonditioner

- Brugeren er logget ind.
- `AuthService.currentUserId()` returnerer brugerens ID.

---

## Postkonditioner (succes)

- Valgte brugere er tilføjet som venner (`FriendRelation`-records oprettet i databasen).
- De tilføjede brugere fjernes fra søgeresultatlisten.
- Vennelisten opdateres automatisk næste gang siden åbnes.

---

## Normalforløb — Søg og tilføj ven

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Navigerer til `/find-participants` |
| 2 | Frontend | `ngOnInit()` kalder `load('')` og `loadFriends()` parallelt |
| 3 | Frontend | `DirectoryService.search('', currentUserId)` → `GET /api/directory/search?query=&excludeFriendsOf={id}` |
| 4 | Frontend | `FriendService.getFriends(currentUserId)` → `GET /api/friends/{id}` |
| 5 | Frontend | Viser tre tabs: **Venner** / **Brugere** / **Spisestedet** — "Venner" er default |
| 6 | Bruger | Skriver i søgefeltet og trykker søg |
| 7 | Frontend | `onSearch()` → `load(searchTerm)` → nyt `GET /api/directory/search?query={term}&excludeFriendsOf={id}` |
| 8 | Frontend | Resultater filtreres client-side i `filtered()` computed signal |
| 9 | Bruger | Klikker på én eller flere brugere for at markere dem (`toggleSelect()`) |
| 10 | Bruger | Trykker "Tilføj" |
| 11 | Frontend | `addSelected()` kalder `FriendService.addFriend()` for hver valgt bruger |
| 12 | Frontend | `POST /api/friends` med `{ initiatorId, receiverId }` for hvert kald |
| 13 | API | `FriendsController.AddFriend()` kalder `ParticipantService.AddFriendAsync()` |
| 14 | Service | Tjekker at initiator ≠ receiver |
| 15 | Service | Tjekker at begge deltagere eksisterer |
| 16 | Service | Tjekker at relationen ikke allerede eksisterer via `RelationExistsAsync()` |
| 17 | Service | Opretter `FriendRelation`-record i databasen |
| 18 | API | Returnerer `HTTP 204 No Content` |
| 19 | Frontend | Tilføjede brugere fjernes fra entries-listen |

---

## Alternative forløb

### A1 — Vis venneliste (tab "Venner")
- **Trin 5:** Bruger ser tab "Venner" som default.
- Viser `friendEntries` — allerede hentede venner opdelt i personer og spisestedet.
- Ingen søgefunktion på vennelisten — den er fast.

### A2 — Skift til tab "Spisestedet"
- **Trin 6:** Bruger klikker på "Spisestedet"-tab.
- `merchantTabEntries` computed: viser merchants der *ikke* allerede er venner.
- Samme tilføj-flow som normalforløbet.

### A3 — Søgning filtrerer client-side
- Søgning i søgefeltet sender **ikke** altid et nyt API-kald.
- `filtered()` computed signal filtrerer de allerede hentede `entries` lokalt på `displayName`, `handle` og `subtitle`.
- Nyt API-kald sker kun ved `onSearch()` (tryk på søge-knap).

---

## Undtagelsesforløb

### E1 — Bruger allerede ven
- **Trin 16:** `RelationExistsAsync()` returnerer `true`.
- Service kaster `InvalidOperationException("Venrelationen eksisterer allerede.")`.
- `ExceptionHandlingMiddleware` mapper til `HTTP 409 Conflict`.
- Frontend: `hasError = true`, viser: *"En eller flere venner kunne ikke tilføjes. Prøv igen."*

### E2 — Bruger tilføjer sig selv
- **Trin 14:** `dto.InitiatorId == dto.ReceiverId`.
- Service kaster `InvalidOperationException("En bruger må ikke tilføje sig selv som ven.")`.
- Returnerer `HTTP 409`.

### E3 — Deltager ikke fundet
- **Trin 15:** `GetByIdAsync()` returnerer null.
- Service kaster `KeyNotFoundException`.
- Returnerer `HTTP 404`.

### E4 — Netværksfejl ved søgning
- Frontend: `errorMessage.set('Kunne ikke hente deltagere. Prøv igen.')`.

---

## Datamodel

### Directory-søgning — `GET /api/directory/search`
| Query param | Type | Beskrivelse |
|-------------|------|-------------|
| `query` | string | Søgeterm (navn, handle, e-mail) |
| `excludeFriendsOf` | int? | Ekskluder eksisterende venner fra resultater |

### Tilføj ven — `POST /api/friends`
| Felt | Type | Påkrævet |
|------|------|----------|
| `initiatorId` | int | ✅ |
| `receiverId` | int | ✅ |

### `FriendRelation` entity (oprettet)
| Kolonne | Værdi |
|---------|-------|
| `InitiatorId` | Den bruger der tilføjer |
| `ReceiverId` | Den bruger der tilføjes |
| `CreatedAt` | Tidsstempel |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| `GET /api/directory/search` | GET | Anonym | 200 + liste af `DirectoryEntryDto` |
| `GET /api/directory/{id}/friends` | GET | Anonym | 200 + liste af venner |
| `GET /api/friends/{participantId}` | GET | Anonym | 200 + liste af venner |
| `POST /api/friends` | POST | Anonym | 204, 409 ved duplikat, 404 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend — find-participants side | ✅ | Søgefelt, tre tabs, avatar-farver og initialer |
| Frontend — directory-søgning | ✅ | `GET /api/directory/search` med `excludeFriendsOf` |
| Frontend — venneliste ved load | ✅ | `GET /api/friends/{id}` |
| Frontend — markér og tilføj venner | ✅ | `addSelected()` sender parallelle POST-kald |
| Frontend — tab-filtrering | ✅ | Computed signals pr. tab |
| API — `GET /api/directory/search` | ✅ | Søgning med ekskludering af venner |
| API — `GET /api/directory/{id}/friends` | ✅ | |
| API — `GET /api/friends/{participantId}` | ✅ | |
| API — `POST /api/friends` | ✅ | Duplikat-tjek i service |
| Auth-krav på endpoints | ❌ | Alle endpoints er uden `[Authorize]` |
| To-trins ven-accept (anmodning + accept) | ❌ | Ingen accept-flow — relation oprettes øjeblikkeligt |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **Ingen `[Authorize]` på nogen af endpoints** | 🔴 Høj | `FriendsController` og `DirectoryController` har ingen autentificeringskrav. Enhver kan tilføje og læse vennerelationer for en vilkårlig bruger. |
| G2 | **Ingen ejerskabsvalidering på `initiatorId`** | 🔴 Høj | `POST /api/friends` modtager `initiatorId` fra request-body. Der valideres ikke at dette ID matcher JWT'ens `sub`-claim. En autentificeret bruger kan oprette vennerelationer på vegne af andre brugere. |
| G3 | **Ingen UNIQUE-constraint på `FriendRelation`** | 🟡 Medium | Duplikat-tjek sker udelukkende i service-laget via `RelationExistsAsync()`. Ingen DB-constraint forhindrer dubletter ved race conditions. |
| G4 | **Ingen ven-anmodning med accept-flow** | 🟡 Medium | Venskab oprettes øjeblikkeligt uden at modparten accepterer. Ingen `Pending`/`Accepted`-status på relationen. |
| G5 | **Søgning sender nyt API-kald kun ved onSearch()** | 🟢 Lav | Søgefeltet filtrerer client-side i realtid, men henter ikke opdaterede data fra server ved hvert tastetryk. Nye brugere der er oprettet efter page-load vises ikke. |
| G6 | **Parallelle POST-kald ved tilføj** | 🟢 Lav | `addSelected()` sender ét HTTP-kald pr. valgt bruger. Ingen batch-endpoint. |

---

## Tekniske noter

- `DirectoryService.search()` sender `excludeFriendsOf={currentUserId}` — eksisterende venner filtreres server-side fra søgeresultaterne (ikke vist i søgelisten).
- `FriendService.getFriends()` bruger `GET /api/friends/{id}` — returnerer `ParticipantDto[]`.
- `DirectoryService.GetFriendsAsync()` bruger `GET /api/directory/{id}/friends` — returnerer `DirectoryEntryDto[]`. To separate endpoints med overlappende formål.
- Avatar-farver og initialer beregnes deterministisk client-side fra navn.

---

## Relaterede use cases

- [UC-04 — Opdater Profil](UC-04-opdater-profil.md)
- [UC-06 — Opret Ordre](UC-06-opret-ordre.md) *(ikke oprettet endnu)*
