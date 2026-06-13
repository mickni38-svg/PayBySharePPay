# UC-06-S05 — Håndter ukendt deltager eller merchant ved oprettelse

**Use Case:** [UC-06 — Opret Ordre](../../usecases/UC-06-opret-ordre.md)  
**Type:** Undtagelsesforløb (E2 + E3)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en host der opretter en ordre med ugyldige deltager- eller merchant-IDs  
Vil jeg se en fejlbesked  
Så jeg ved at et eller flere IDs ikke er gyldige.

---

## Acceptkriterier

- [ ] Hvis et deltager-ID ikke eksisterer, returnerer API `HTTP 404 Not Found`.
- [ ] Hvis merchant-ID ikke eksisterer, returnerer API `HTTP 404 Not Found`.
- [ ] Frontend viser en fejlbesked til host.

---

## Tekniske detaljer

- **Service:** `KeyNotFoundException` kastes ved `GetByIdAsync()` = null
- **Middleware:** `ExceptionHandlingMiddleware` mapper til `HTTP 404`

---

## Relaterede stories

- [UC-06-S01 — Opret gruppeordre med merchant](UC-06-S01-opret-ordre-med-merchant.md)
- [UC-06-S03 — Tilføj deltagere til ordre ved oprettelse](UC-06-S03-tilfoj-deltagere-ved-oprettelse.md)
