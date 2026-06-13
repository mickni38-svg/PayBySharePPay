# UC-04-S07 — Fjern PasswordHash fra ParticipantDto

**Use Case:** [UC-04 — Opdater Profil](../usecases/UC-04-opdater-profil.md)  
**Type:** Gap-story (G3)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som systemets sikkerhedsansvarlige  
Vil jeg at BCrypt-hashede adgangskoder aldrig returneres i API-responses  
Så fortrolige oplysninger ikke eksponeres i frontend-kode, netværkstrafik eller logs.

---

## Baggrund

`MapToDto()` inkluderer `PasswordHash` i `ParticipantDto`. Hashede adgangskoder returneres dermed i `GET /api/participants/{id}`- og `PUT`-responses og gemmes i Angular-komponenten. Selvom BCrypt-hash ikke er en klartekst-adgangskode, udgør eksponeringen et unødvendigt sikkerhedsmæssigt problem (G3 i UC-04).

---

## Acceptkriterier

- [ ] `ParticipantDto` indeholder ikke et `PasswordHash`-felt.
- [ ] `MapToDto()` mapper ikke `PasswordHash` til DTO'en.
- [ ] `GET /api/participants/{id}` og `PUT /api/participants/{id}/profile` returnerer aldrig `PasswordHash` i response-body.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `ParticipantDto.cs` | Fjern `PasswordHash`-felt |
| `MapToDto()` (eller tilsvarende mapper) | Fjern mapping af `PasswordHash` |

---

## Relaterede stories

- [UC-04-S01 — Hent og vis profil](UC-04-S01-hent-og-vis-profil.md)
- [UC-04-S05 — Beskyt profil-endpoints med autorisation](UC-04-S05-beskyt-profil-endpoints.md)
