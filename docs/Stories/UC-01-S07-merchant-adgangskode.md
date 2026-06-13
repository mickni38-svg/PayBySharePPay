# UC-01-S07 — Tilføj adgangskode til merchant-registrering

**Use Case:** UC-01 — Opret Bruger  
**Type:** Gap-story (G1)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som et spisested der registrerer sig  
Vil jeg kunne vælge en adgangskode ved oprettelse  
Så jeg kan logge ind med e-mail og adgangskode ligesom private brugere.

---

## Baggrund

I dag oprettes merchants uden adgangskode (`PasswordHash` er `null`). Det betyder, at merchants ikke kan logge ind via det normale login-flow. Dette er et sikkerhedsmæssigt og funktionelt gap (G1 i UC-01).

---

## Acceptkriterier

- [ ] Merchant-registreringsformularen indeholder felterne: Adgangskode (påkrævet, min. 6 tegn) og Gentag adgangskode (påkrævet).
- [ ] Frontend validerer at de to adgangskoder matcher.
- [ ] `RegisterMerchantRequest` udvides med `Password`-felt.
- [ ] `ParticipantService.CreateMerchantAsync()` BCrypt-hasher og gemmer adgangskoden.
- [ ] Merchant kan efterfølgende logge ind via `POST /api/auth/login` med e-mail og adgangskode.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `RegisterMerchantRequest.cs` | Tilføj `Password` (string, `[MinLength(6)]`) |
| `ParticipantService.CreateMerchantAsync()` | Hash og gem `PasswordHash` |
| Merchant-registreringsformular (Angular) | Tilføj adgangskode-felter med match-validering |

---

## Relaterede stories

- [UC-01-S04 — Registrer merchant](UC-01-S04-registrer-merchant.md)
