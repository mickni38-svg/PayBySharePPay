# UC-02-S01 — Log ind med e-mail

**Use Case:** [UC-02 — Log ind](../usecases/UC-02-log-ind.md)  
**Type:** Normalforløb  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en registreret bruger  
Vil jeg kunne logge ind ved at indtaste min e-mail  
Så jeg kan tilgå min konto og deltage i gruppeordrer.

---

## Acceptkriterier

- [ ] Brugeren kan åbne `/login` og se en formular med et e-mail-felt og en "Log ind"-knap.
- [ ] `POST /api/auth/login` kaldes med `{ email }` når formularen indsendes.
- [ ] API returnerer `HTTP 200` med `{ token, participantId, name, expiresAt }`.
- [ ] Token og brugerinfo gemmes i `localStorage` (`sbys_token`, `sbys_user`).
- [ ] `AuthService.isLoggedIn` signal sættes til `true`.
- [ ] Brugeren navigeres til `/home` efter succesfuldt login.

---

## Tekniske detaljer

- **API:** `POST /api/auth/login` (`AuthController.Login()`)
- **Opslag:** `SearchParticipantsAsync()` + `GetByEmailAsync()`
- **Auth:** Anonym
- **Session-lagring:** `AuthService._storeSession()` → `localStorage`
- **Navigation:** `window.location.href = '/home'` (hard reload)

---

## Relaterede stories

- [UC-02-S02 — Log ind med adgangskode](UC-02-S02-log-ind-med-adgangskode.md)
- [UC-02-S03 — Håndter ukendt e-mail ved login](UC-02-S03-haandter-ukendt-email.md)
- [UC-02-S05 — Håndter udløbet token og omdiriger til login](UC-02-S05-haandter-udloebet-token.md)
