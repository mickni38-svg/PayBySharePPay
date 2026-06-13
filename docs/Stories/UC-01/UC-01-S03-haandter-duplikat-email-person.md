# UC-01-S03 — Håndter duplikat e-mail ved personregistrering

**Use Case:** [UC-01 — Opret Bruger](../../usecases/UC-01-opret-bruger.md)  
**Type:** Fejlhåndtering  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der forsøger at registrere sig med en e-mail der allerede er i brug  
Vil jeg se en klar fejlbesked direkte på formularen  
Så jeg ved, at jeg skal bruge en anden e-mail eller logge ind i stedet.

---

## Acceptkriterier

- [ ] Hvis e-mailen allerede er registreret, returnerer API `HTTP 409 Conflict`.
- [ ] Fejlbeskeden *"En bruger med denne e-mail eksisterer allerede."* vises inline på registreringsformularen.
- [ ] Brugeren kan rette e-mail-feltet og forsøge igen uden at siden genindlæses.

---

## Tekniske detaljer

- **API:** `AuthController.Register()` kalder `SearchParticipantsAsync()` for at tjekke duplikat
- **Response:** `HTTP 409` med `{ error: "En bruger med denne e-mail eksisterer allerede." }`
- **Kendt gap (G-duplikat-check):** `SearchParticipantsAsync()` er en tekstsøgning — ikke et præcist e-mail-opslag. Der er ingen UNIQUE-constraint på `Email`-kolonnen, så race conditions er mulige.

---

## Relaterede stories

- [UC-01-S01 — Registrer privat bruger](UC-01-S01-registrer-privat-bruger.md)
- [UC-01-S05 — Håndter duplikat e-mail ved merchant-registrering](UC-01-S05-haandter-duplikat-email-merchant.md)
