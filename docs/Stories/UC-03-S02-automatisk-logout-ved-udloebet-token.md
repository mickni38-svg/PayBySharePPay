# UC-03-S02 — Automatisk logout ved udløbet token

**Use Case:** [UC-03 — Log ud](../usecases/UC-03-log-ud.md)  
**Type:** Alternativt forløb  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger med et udløbet eller ugyldigt JWT-token  
Vil jeg automatisk blive logget ud og sendt til login-siden, når jeg forsøger at bruge appen  
Så systemet ikke viser mig beskyttede sider med en ugyldig session.

---

## Acceptkriterier

- [ ] Når et API-kald returnerer `HTTP 401`, opfanger `apiInterceptor` svaret.
- [ ] `apiInterceptor` kalder `auth.logout()`.
- [ ] Session ryddes fra `localStorage` (`sbys_token`, `sbys_user`).
- [ ] Brugeren navigeres til `/login` via Angular Router.

---

## Tekniske detaljer

- **Interceptor:** `apiInterceptor` — Angular HTTP-interceptor
- **Navigation:** Angular Router (ikke `window.location.href` som ved login)
- **Service:** `AuthService.logout()`

---

## Relaterede stories

- [UC-03-S01 — Manuel logout fra profilside](UC-03-S01-manuel-logout-fra-profilside.md)
- [UC-02-S05 — Håndter udløbet token og omdiriger til login](UC-02-S05-haandter-udloebet-token.md)
