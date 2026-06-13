# UC-02-S07 — Tilføj password-felt til login-formular

**Use Case:** [UC-02 — Log ind](../usecases/UC-02-log-ind.md)  
**Type:** Gap-story (G1)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som en registreret bruger  
Vil jeg kunne indtaste min adgangskode i login-formularen  
Så min konto er reelt beskyttet og ikke tilgængelig for alle der kender min e-mail.

---

## Baggrund

`LoginRequest.Password` er defineret i API'et og BCrypt-verifikationen er implementeret server-side, men login-formularen i Angular har kun et e-mail-felt. `AuthService.login()` sender aldrig et password. Reelt kan enhver logge ind med en hvilken som helst registreret e-mail (G1 i UC-02).

---

## Acceptkriterier

- [ ] Login-formularen har et adgangskode-felt (type `password`).
- [ ] `AuthService.login(email, password)` sender `{ email, password }` i POST-body.
- [ ] Brugere med `PasswordHash` skal angive korrekt adgangskode for at logge ind.
- [ ] Brugere uden `PasswordHash` (legacy seed-merchants) kan stadig logge ind uden password.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| Login-komponent (Angular) | Tilføj `password`-felt til reaktiv formular |
| `AuthService.login()` | Udvid metode-signatur med `password: string` og inkluder i POST-body |

---

## Relaterede stories

- [UC-02-S02 — Log ind med adgangskode](UC-02-S02-log-ind-med-adgangskode.md)
- [UC-02-S08 — Tillad merchant at logge ind](UC-02-S08-merchant-login.md)
