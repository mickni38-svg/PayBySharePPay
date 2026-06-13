# UC-06-S07 — Tilføj deltagere til eksisterende ordre

**Use Case:** [UC-06 — Opret Ordre](../../usecases/UC-06-opret-ordre.md)  
**Type:** Gap-story (G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som en host med en aktiv gruppeordre  
Vil jeg kunne tilføje nye deltagere efter ordren er oprettet  
Så jeg ikke behøver at genstarte ordren hvis jeg glemte nogen.

---

## Baggrund

Deltagere kan kun tilføjes ved oprettelse via `participantIds[]` i `POST /api/orders`. Der findes intet endpoint til efterfølgende tilføjelse. Hvis host ønsker at tilføje en deltager, skal ordren slettes og genskabes (G2 i UC-06).

---

## Acceptkriterier

- [ ] `POST /api/orders/{id}/participants` tilføjer én eller flere deltagere til en eksisterende ordre.
- [ ] Nye deltagere tildeles status `Invited` og et unikt `ParticipantToken`.
- [ ] Bestillingslink (hvis merchant er tilknyttet) genereres og sendes som `Message` til nye deltagere.
- [ ] Endpoint er kun tilgængeligt for ordrens host.
- [ ] Returnerer `HTTP 204` ved succes.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `OrdersController` | Tilføj `POST /api/orders/{id}/participants` endpoint |
| `OrderService` | Tilføj `AddParticipantsAsync(orderId, participantIds)` metode |

---

## Relaterede stories

- [UC-06-S03 — Tilføj deltagere til ordre ved oprettelse](UC-06-S03-tilfoj-deltagere-ved-oprettelse.md)
