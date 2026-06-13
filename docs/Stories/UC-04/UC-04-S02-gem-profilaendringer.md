# UC-04-S02 — Gem profilændringer

**Use Case:** [UC-04 — Opdater Profil](../usecases/UC-04-opdater-profil.md)  
**Type:** Normalforløb (trin 4–14)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind bruger  
Vil jeg kunne ændre mit navn, e-mail og/eller telefonnummer og gemme ændringerne  
Så mine profiloplysninger er opdaterede.

---

## Acceptkriterier

- [ ] Brugeren kan redigere navn, e-mail og telefon i formularen på `/profile`.
- [ ] Knappen "Gem ændringer" sender `PUT /api/participants/{id}/profile` med `{ name, email?, phone? }`.
- [ ] API returnerer `HTTP 200` med opdateret `ParticipantDto`.
- [ ] En grøn succesbesked vises i 3 sekunder efter gem.
- [ ] Formularfelterne viser de opdaterede værdier efter gem.

---

## Tekniske detaljer

- **API:** `PUT /api/participants/{id}/profile` (`ParticipantsController.UpdateProfile()`)
- **Service:** `ParticipantService.UpdateProfileAsync()`
- **Frontend:** `ProfileService.updateProfile()` → `saveSuccess` signal + `setTimeout(3000)`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | `PUT`-endpoint har ingen `[Authorize]` — enhver kan opdatere enhver profil | 🔴 Høj |
| G2 | Ingen ejerskabsvalidering — JWT `sub` tjekkes ikke mod `{id}` i URL | 🔴 Høj |

---

## Relaterede stories

- [UC-04-S01 — Hent og vis profil](UC-04-S01-hent-og-vis-profil.md)
- [UC-04-S03 — Håndter tomt navn ved profilopdatering](UC-04-S03-haandter-tomt-navn.md)
- [UC-04-S05 — Beskyt profil-endpoints med autorisation (gap G1+G2)](UC-04-S05-beskyt-profil-endpoints.md)
