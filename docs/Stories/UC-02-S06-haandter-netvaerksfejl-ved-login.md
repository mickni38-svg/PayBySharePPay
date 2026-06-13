# UC-02-S06 — Håndter netværksfejl ved login

**Use Case:** [UC-02 — Log ind](../usecases/UC-02-log-ind.md)  
**Type:** Undtagelsesforløb (E3)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der forsøger at logge ind  
Vil jeg se en venlig fejlbesked, hvis API'et er utilgængeligt  
Så jeg ved, at problemet er midlertidigt og kan prøve igen.

---

## Acceptkriterier

- [ ] Hvis API-kaldet fejler (netværksfejl, timeout, 5xx), vises: *"Noget gik galt. Prøv igen."*
- [ ] Formularen forbliver udfyldt så brugeren ikke skal taste alt ind igen.
- [ ] Der logges ikke sensitive data (password mv.) ved fejl.

---

## Tekniske detaljer

- **Fejlhåndtering:** Angular `error`-handler i login-komponenten

---

## Relaterede stories

- [UC-02-S01 — Log ind med e-mail](UC-02-S01-log-ind-med-email.md)
