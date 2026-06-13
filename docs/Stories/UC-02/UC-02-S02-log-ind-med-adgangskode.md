# UC-02-S02 — Log ind med adgangskode

**Use Case:** [UC-02 — Log ind](../usecases/UC-02-log-ind.md)  
**Type:** Alternativt forløb (A1) / Gap-story (G1)  
**Status:** ❌ Ikke implementeret (frontend)  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som en registreret bruger med en adgangskode  
Vil jeg kunne logge ind med både e-mail og adgangskode  
Så min konto er beskyttet mod uautoriseret adgang.

---

## Baggrund

Server-siden er klar til BCrypt-verifikation: hvis `request.Password` er udfyldt og `PasswordHash` ikke er null, køres `BCrypt.Verify()`. Men login-formularen i frontend har **kun et e-mail-felt** — password sendes aldrig. Det betyder, at adgangskodebeskyttelsen reelt er deaktiveret (G1 i UC-02).

---

## Acceptkriterier

- [ ] Login-formularen indeholder et adgangskode-felt ud over e-mail-feltet.
- [ ] `AuthService.login()` sender `{ email, password }` i request body.
- [ ] API verificerer adgangskoden med BCrypt når `PasswordHash` er sat på brugeren.
- [ ] Ved korrekt adgangskode returneres `HTTP 200` med JWT.
- [ ] Ved forkert adgangskode returneres `HTTP 401` — se [UC-02-S04](UC-02-S04-haandter-forkert-adgangskode.md).

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| Login-komponent (Angular) | Tilføj `password`-felt til formularen |
| `AuthService.login()` | Inkluder `password` i POST-body |

---

## Relaterede stories

- [UC-02-S01 — Log ind med e-mail](UC-02-S01-log-ind-med-email.md)
- [UC-02-S04 — Håndter forkert adgangskode ved login](UC-02-S04-haandter-forkert-adgangskode.md)
