# UC-02-S04 — Håndter forkert adgangskode ved login

**Use Case:** [UC-02 — Log ind](../usecases/UC-02-log-ind.md)  
**Type:** Undtagelsesforløb (E2)  
**Status:** ✅ Implementeret (server-side) / ❌ Ikke aktivt (frontend sender ikke password)  

---

## Beskrivelse

Som en bruger der logger ind med en forkert adgangskode  
Vil jeg se en fejlbesked  
Så jeg ved, at adgangskoden er forkert og kan prøve igen.

---

## Acceptkriterier

- [ ] Hvis `BCrypt.Verify()` returnerer `false`, returnerer API `HTTP 401` med `{ error: "Forkert adgangskode." }`.
- [ ] Frontend viser en fejlbesked til brugeren.
- [ ] Fejlbeskeden skelner mellem ukendt e-mail og forkert adgangskode.

---

## Tekniske detaljer

- **API:** `BCrypt.Verify(request.Password, entity.PasswordHash)` → `false` → `HTTP 401`
- **Kendt gap (G3):** Frontend viser samme generiske besked (*"Ingen konto fundet med den e-mail."*) for både E1 og E2 — ingen skelnen

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | Password-felt mangler i login-formular — BCrypt-verifikation køres aldrig | 🔴 Høj |
| G3 | Fejlbesked skelner ikke mellem ukendt e-mail og forkert password | 🟡 Medium |

---

## Relaterede stories

- [UC-02-S02 — Log ind med adgangskode](UC-02-S02-log-ind-med-adgangskode.md)
- [UC-02-S03 — Håndter ukendt e-mail ved login](UC-02-S03-haandter-ukendt-email.md)
