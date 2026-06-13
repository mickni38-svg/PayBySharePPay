# UC-01-S04 — Registrer merchant (spisested)

**Use Case:** [UC-01 — Opret Bruger](../../usecases/UC-01-opret-bruger.md)  
**Type:** Merchant-registrering  
**Status:** ✅ Implementeret (med kendte gaps)  

---

## Beskrivelse

Som et spisested der vil modtage gruppeordrer  
Vil jeg kunne registrere min virksomhed med navn og firmaoplysninger  
Så jeg kan blive synlig for grupper der vil bestille mad.

---

## Acceptkriterier

- [ ] Brugeren kan vælge tab **Spisested** på `/register`-siden.
- [ ] Formularen indeholder: Navn (påkrævet), Firmanavn (påkrævet), CVR (valgfri), Kontaktperson (valgfri), E-mail (valgfri), Telefon (valgfri), Adresse (valgfri).
- [ ] `POST /api/auth/register-merchant` kaldes med de udfyldte felter.
- [ ] En ny `Participant` (type = `Merchant`) gemmes i databasen — **uden adgangskode**.
- [ ] API returnerer `HTTP 201` med JWT-token.
- [ ] Merchant navigeres til `/home` efter oprettelse.

---

## Tekniske detaljer

- **API:** `POST /api/auth/register-merchant` (`AuthController.RegisterMerchant()`)
- **Service:** `ParticipantService.CreateMerchantAsync()`
- **Auth:** Anonym
- **Ingen adgangskode:** `Participant.PasswordHash` sættes ikke for merchants

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G1 | Merchant oprettes uden adgangskode — kan ikke logge ind med password | 🔴 Høj |
| G3 | `GroupOrderUrl` sættes ikke ved oprettelse — merchant kan ikke bruges i ordrer | 🟡 Medium |

---

## Relaterede stories

- [UC-01-S05 — Håndter duplikat e-mail ved merchant-registrering](UC-01-S05-haandter-duplikat-email-merchant.md)
- [UC-01-S07 — Tilføj adgangskode til merchant-registrering (gap G1)](UC-01-S07-merchant-adgangskode.md)
- [UC-01-S08 — Sæt merchant GroupOrderUrl ved oprettelse (gap G3)](UC-01-S08-saet-merchant-grouporderurl-ved-oprettelse.md)
