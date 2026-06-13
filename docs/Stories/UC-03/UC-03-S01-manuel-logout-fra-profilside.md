# UC-03-S01 — Manuel logout fra profilside

**Use Case:** [UC-03 — Log ud](../usecases/UC-03-log-ud.md)  
**Type:** Normalforløb  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en logget-ind bruger  
Vil jeg kunne logge ud ved at trykke "Log ud" på profilsiden  
Så min session afsluttes og ingen andre kan bruge min konto på enheden.

---

## Acceptkriterier

- [ ] Brugeren kan se og trykke en "Log ud"-knap på `/profile`.
- [ ] `AuthService.logout()` kaldes når knappen trykkes.
- [ ] `sbys_token` og `sbys_user` fjernes fra `localStorage`.
- [ ] `AuthService._token` og `AuthService._user` signals sættes til `null`.
- [ ] `isLoggedIn` computed signal er `false` efter logout.
- [ ] Brugeren navigeres til `/login` efter logout.

---

## Tekniske detaljer

- **Komponent:** `ProfileComponent` kalder `auth.logout()`
- **Service:** `AuthService.logout()` — ren klient-side operation, ingen API-kald
- **Navigation:** Angular Router → `/login`

---

## Relaterede stories

- [UC-03-S02 — Automatisk logout ved udløbet token](UC-03-S02-automatisk-logout-ved-udloebet-token.md)
- [UC-03-S03 — Implementer server-side token invalidering (gap G1)](UC-03-S03-server-side-token-invalidering.md)
