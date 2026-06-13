# UC-02-S08 — Tillad merchant at logge ind

**Use Case:** [UC-02 — Log ind](../usecases/UC-02-log-ind.md)  
**Type:** Gap-story (G2)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som et registreret spisested (merchant)  
Vil jeg kunne logge ind via login-siden  
Så jeg kan administrere min profil og modtage gruppeordrer.

---

## Baggrund

`AuthController.Login()` søger kun efter `Participant` af type `Person`. En merchant der forsøger at logge ind vil aldrig blive fundet i opslaget, og modtager `HTTP 401`. Dette er et ikke-dokumenteret og kritisk gap (G2 i UC-02).

---

## Acceptkriterier

- [ ] `AuthController.Login()` finder både `Person`- og `Merchant`-brugere ved e-mail-opslag.
- [ ] En merchant med en `PasswordHash` kan logge ind med e-mail og adgangskode.
- [ ] JWT udstedes og session gemmes på samme vis som for `Person`.
- [ ] En merchant uden `PasswordHash` kan **ikke** logge ind (kræver G1-fix i UC-01 er løst).

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `AuthController.Login()` | Fjern type-filter så både `Person` og `Merchant` kan matches ved e-mail-opslag |

---

## Relaterede stories

- [UC-02-S01 — Log ind med e-mail](UC-02-S01-log-ind-med-email.md)
- [UC-02-S07 — Tilføj password-felt til login-formular](UC-02-S07-tilfoej-password-felt-til-login.md)
- [UC-01-S07 — Tilføj adgangskode til merchant-registrering](UC-01-S07-merchant-adgangskode.md)
