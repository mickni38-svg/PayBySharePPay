# UC-04-S01 — Hent og vis profil

**Use Case:** [UC-04 — Opdater Profil](../usecases/UC-04-opdater-profil.md)  
**Type:** Normalforløb (trin 1–3)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind bruger  
Vil jeg se mine nuværende profiloplysninger når jeg åbner profilsiden  
Så jeg ved hvad der er registreret på min konto.

---

## Acceptkriterier

- [ ] Når brugeren navigerer til `/profile`, hentes profilen automatisk via `GET /api/participants/{id}`.
- [ ] Formularfelterne (navn, e-mail, telefon) udfyldes med de hentede data.
- [ ] Brugerens ID hentes fra `AuthService.currentUserId()`.
- [ ] Siden vises korrekt selvom e-mail og telefon er `null`.

---

## Tekniske detaljer

- **API:** `GET /api/participants/{id}` → returnerer `ParticipantDto`
- **Komponent:** `ProfileComponent.ngOnInit()`
- **Service:** `ProfileService`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | `GET /api/participants/{id}` har ingen `[Authorize]` — enhver kan hente enhver profil | 🔴 Høj |
| G3 | `PasswordHash` returneres i `ParticipantDto` og eksponeres i API-response | 🔴 Høj |

---

## Relaterede stories

- [UC-04-S02 — Gem profilændringer](UC-04-S02-gem-profilaendringer.md)
- [UC-04-S05 — Beskyt profil-endpoints med autorisation (gap G1+G2)](UC-04-S05-beskyt-profil-endpoints.md)
- [UC-04-S07 — Fjern PasswordHash fra ParticipantDto (gap G3)](UC-04-S07-fjern-passwordhash-fra-dto.md)
