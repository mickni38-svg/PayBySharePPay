# UC-04-S03 — Håndter tomt navn ved profilopdatering

**Use Case:** [UC-04 — Opdater Profil](../usecases/UC-04-opdater-profil.md)  
**Type:** Undtagelsesforløb (E1)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der forsøger at gemme en profil uden navn  
Vil jeg se en fejlbesked og ikke miste mine øvrige indtastede oplysninger  
Så jeg ved at navn er påkrævet og kan rette det.

---

## Acceptkriterier

- [ ] Hvis navn-feltet er tomt, forhindres API-kaldet (frontend-validering: `if (!this.name().trim()) return`).
- [ ] En fejlbesked vises til brugeren (`saveError` signal sættes til `true`).
- [ ] Server-side: `PUT`-endpoint returnerer `HTTP 400` hvis `request.Name` er tom.
- [ ] Formularen forbliver udfyldt så brugeren kan rette fejlen.

---

## Tekniske detaljer

- **Frontend:** `ProfileComponent` — guard med `if (!this.name().trim()) return`
- **API:** `ParticipantsController.UpdateProfile()` — `if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(...)`

---

## Relaterede stories

- [UC-04-S02 — Gem profilændringer](UC-04-S02-gem-profilaendringer.md)
