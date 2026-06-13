# UC-01-S06 — Håndter netværksfejl ved registrering

**Use Case:** [UC-01 — Opret Bruger](../../usecases/UC-01-opret-bruger.md)  
**Type:** Fejlhåndtering  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der forsøger at registrere sig  
Vil jeg se en venlig fejlbesked, hvis API'et er utilgængeligt eller der opstår en netværksfejl  
Så jeg ved, at problemet er midlertidigt og kan prøve igen.

---

## Acceptkriterier

- [ ] Hvis API-kaldet fejler (netværksfejl, timeout, 5xx), vises beskeden *"Noget gik galt. Prøv igen."*
- [ ] Formularen forbliver udfyldt, så brugeren ikke skal taste alt ind igen.
- [ ] Der logges ikke sensitive data (adgangskode mv.) ved fejl.

---

## Tekniske detaljer

- **Fejlhåndtering:** Angular `error`-handler i registreringskomponenten
- Gælder for både Person- og Merchant-registrering

---

## Relaterede stories

- [UC-01-S01 — Registrer privat bruger](UC-01-S01-registrer-privat-bruger.md)
- [UC-01-S04 — Registrer merchant](UC-01-S04-registrer-merchant.md)
