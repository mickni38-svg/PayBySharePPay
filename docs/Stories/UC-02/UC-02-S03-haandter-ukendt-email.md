# UC-02-S03 — Håndter ukendt e-mail ved login

**Use Case:** [UC-02 — Log ind](../usecases/UC-02-log-ind.md)  
**Type:** Undtagelsesforløb (E1)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der forsøger at logge ind med en e-mail der ikke findes i systemet  
Vil jeg se en klar fejlbesked  
Så jeg ved, at e-mailen ikke er registreret.

---

## Acceptkriterier

- [ ] Hvis ingen `Person` med den angivne e-mail findes, returnerer API `HTTP 401`.
- [ ] Frontend viser: *"Ingen konto fundet med den e-mail."*
- [ ] Formularen forbliver udfyldt så brugeren kan rette e-mailen.

---

## Tekniske detaljer

- **API:** `SearchParticipantsAsync()` returnerer ingen match → `HTTP 401` med `{ error: "Ingen bruger fundet med denne email." }`
- **Frontend:** 401-handler i `AuthService` viser dansk fejlbesked

---

## Relaterede stories

- [UC-02-S01 — Log ind med e-mail](UC-02-S01-log-ind-med-email.md)
- [UC-02-S04 — Håndter forkert adgangskode ved login](UC-02-S04-haandter-forkert-adgangskode.md)
