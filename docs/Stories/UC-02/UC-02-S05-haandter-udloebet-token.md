# UC-02-S05 — Håndter udløbet token og omdiriger til login

**Use Case:** [UC-02 — Log ind](../usecases/UC-02-log-ind.md)  
**Type:** Alternativt forløb (A2)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger med et udløbet JWT-token  
Vil jeg automatisk blive logget ud og sendt til login-siden  
Så jeg kan logge ind igen og fortsætte med at bruge appen.

---

## Acceptkriterier

- [ ] Når API returnerer `HTTP 401` på et beskyttet kald, kalder `apiInterceptor` `auth.logout()`.
- [ ] `localStorage` ryddes for `sbys_token` og `sbys_user`.
- [ ] `AuthService.isLoggedIn` signal sættes til `false`.
- [ ] Brugeren navigeres automatisk til `/login`.

---

## Tekniske detaljer

- **Interceptor:** `apiInterceptor` opfanger `HTTP 401`-responses
- **Logout:** `auth.logout()` rydder `localStorage` og opdaterer signal
- **Ingen token refresh:** Brugeren skal logge ind manuelt igen (gap G4)

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G4 | Token refresh ikke implementeret — brugeren skal logge ind igen efter udløb | 🟡 Medium |

---

## Relaterede stories

- [UC-02-S01 — Log ind med e-mail](UC-02-S01-log-ind-med-email.md)
