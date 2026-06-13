# UC-04-S04 — Håndter netværksfejl ved profilopdatering

**Use Case:** [UC-04 — Opdater Profil](../usecases/UC-04-opdater-profil.md)  
**Type:** Undtagelsesforløb (E3)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der forsøger at gemme profilændringer  
Vil jeg se en fejlbesked, hvis API'et er utilgængeligt eller der opstår en netværksfejl  
Så jeg ved at ændringerne ikke er gemt og kan prøve igen.

---

## Acceptkriterier

- [ ] Hvis `PUT`-kaldet fejler (netværksfejl, timeout, 5xx), sættes `saveError` signal til `true`.
- [ ] En rød fejlbesked vises til brugeren.
- [ ] Formularfelterne forbliver udfyldt med brugerens indtastede værdier.

---

## Tekniske detaljer

- **Frontend:** `ProfileComponent` error-handler → `saveError.set(true)`

---

## Relaterede stories

- [UC-04-S02 — Gem profilændringer](UC-04-S02-gem-profilaendringer.md)
