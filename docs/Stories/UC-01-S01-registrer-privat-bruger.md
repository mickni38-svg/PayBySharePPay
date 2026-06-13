# UC-01-S01 — Registrer privat bruger

**Use Case:** UC-01 — Opret Bruger  
**Type:** Person (privat bruger)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en ikke-registreret person  
Vil jeg kunne oprette en konto med navn, e-mail og adgangskode  
Så jeg kan logge ind og deltage i gruppeordrer.

---

## Acceptkriterier

- [ ] Brugeren kan åbne `/register` og se registreringsformularen med tab **Bruger** valgt som default.
- [ ] Formularen indeholder felterne: Fulde navn (påkrævet), E-mail (påkrævet), Telefon (valgfri), Adgangskode (påkrævet), Gentag adgangskode (påkrævet).
- [ ] `POST /api/auth/register` kaldes med `{ name, email, phone?, password }`.
- [ ] En ny `Participant` (type = `Person`) gemmes i databasen med BCrypt-hashet adgangskode.
- [ ] API returnerer `HTTP 201` med `{ token, participantId, name, expiresAt }`.
- [ ] Token og brugerinfo gemmes i `localStorage` (`sbys_token`, `sbys_user`).
- [ ] Brugeren navigeres automatisk til `/home` efter oprettelse.

---

## Tekniske detaljer

- **API:** `POST /api/auth/register` (`AuthController.Register()`)
- **Service:** `ParticipantService.CreatePersonAsync()`
- **Auth:** Anonym (ingen JWT krævet)
- **Password hashing:** `BCrypt.Net.BCrypt.HashPassword()`
- **JWT:** Genereres via `JwtTokenService.GenerateToken(id, name)`

---

## Relaterede stories

- [UC-01-S02 — Valider adgangskoder matcher](UC-01-S02-valider-adgangskoder-matcher.md)
- [UC-01-S03 — Håndter duplikat e-mail ved personregistrering](UC-01-S03-haandter-duplikat-email-person.md)
- [UC-01-S04 — Registrer merchant (spisested)](UC-01-S04-registrer-merchant.md)
