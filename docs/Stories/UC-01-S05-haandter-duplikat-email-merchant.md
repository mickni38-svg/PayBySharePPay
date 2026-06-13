# UC-01-S05 — Håndter duplikat e-mail ved merchant-registrering

**Use Case:** [UC-01 — Opret Bruger](../usecases/UC-01-opret-bruger.md)  
**Type:** Fejlhåndtering  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som et spisested der forsøger at registrere sig med en e-mail der allerede er i brug  
Vil jeg se en klar fejlbesked direkte på formularen  
Så jeg ved, at e-mailen ikke er tilgængelig.

---

## Acceptkriterier

- [ ] Hvis `contactEmail` allerede er registreret, returnerer API `HTTP 409 Conflict`.
- [ ] Fejlbeskeden *"Et spisested med denne e-mail eksisterer allerede."* vises inline på merchant-formularen.
- [ ] Brugeren kan rette e-mail-feltet og forsøge igen.

---

## Tekniske detaljer

- **API:** `AuthController.RegisterMerchant()` tjekker `contactEmail` via `SearchParticipantsAsync()`
- **Response:** `HTTP 409` med relevant fejlbesked

---

## Relaterede stories

- [UC-01-S04 — Registrer merchant](UC-01-S04-registrer-merchant.md)
- [UC-01-S03 — Håndter duplikat e-mail ved personregistrering](UC-01-S03-haandter-duplikat-email-person.md)
