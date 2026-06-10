# UC-03 — Log ud

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Branch:** Create-usecases  

---

## Overblik

| Felt | Værdi |
|------|-------|
| Use Case ID | UC-03 |
| Navn | Log ud |
| Primær aktør | Logget-ind bruger |
| Formål | Afslutte sessionen og rydde lokal login-state |
| Trigger | Bruger trykker "Log ud" på profilsiden, eller token udløber og API returnerer 401 |

---

## Aktører

| Aktør | Rolle |
|-------|-------|
| **Bruger** | Person der er logget ind og ønsker at logge ud |
| **Frontend** | `AuthService` rydder `localStorage` og nulstiller signals |
| **apiInterceptor** | Håndterer automatisk logout ved 401-svar |

---

## Prækonditioner

- Brugeren er logget ind (`sbys_token` og `sbys_user` er sat i `localStorage`).

---

## Postkonditioner (succes)

- `sbys_token` og `sbys_user` er fjernet fra `localStorage`.
- `AuthService._token` og `AuthService._user` signals er `null`.
- `isLoggedIn` computed signal er `false`.
- Brugeren er navigeret til `/login`.

---

## Normalforløb — Manuel logout (fra profilside)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Bruger | Navigerer til `/profile` |
| 2 | Bruger | Trykker "Log ud"-knap |
| 3 | Frontend | `AuthService.logout()` kaldes |
| 4 | Frontend | `localStorage.removeItem('sbys_token')` og `localStorage.removeItem('sbys_user')` |
| 5 | Frontend | `_token.set(null)` og `_user.set(null)` |
| 6 | Frontend | Router navigerer til `/login` |

---

## Alternativt forløb — Automatisk logout ved udløbet token (401)

| Trin | Aktør | Handling |
|------|-------|----------|
| 1 | Frontend | Et API-kald returnerer `HTTP 401 Unauthorized` |
| 2 | `apiInterceptor` | Fanger 401-svaret |
| 3 | `apiInterceptor` | Kalder `auth.logout()` |
| 4 | `apiInterceptor` | Navigerer til `/login` via Angular Router |

---

## Undtagelsesforløb

*Ingen — logout kan ikke fejle, da det kun er en lokal operation.*

---

## API-endpoints

*Ingen — logout er udelukkende en klient-side operation. Der er ingen server-side session eller token-invalidering.*

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| `AuthService.logout()` | ✅ | Rydder `localStorage` + nulstiller signals |
| `apiInterceptor` — 401 → auto-logout | ✅ | Kalder `auth.logout()` + navigerer til `/login` |
| Log ud-knap på profilside | ✅ | `ProfileComponent` kalder `auth.logout()` |
| Server-side token invalidering | ❌ | JWT blacklisting ikke implementeret |
| Redirect til oprindelig URL efter re-login | ❌ | Brugeren havner altid på `/home` efter re-login |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet | Beskrivelse |
|---|--------|-----------|-------------|
| G1 | **Ingen server-side token invalidering** | 🟡 Medium | JWT er stateless — et udstedt token er gyldigt til udløb, selv efter logout. Ingen blacklist eller revocation implementeret. |
| G2 | **Ingen redirect til oprindelig URL** | 🟢 Lav | Når en bruger logges ud automatisk ved 401, ved systemet ikke hvilken URL brugeren kom fra. Efter re-login havner brugeren altid på `/home`. |

---

## Tekniske noter

- `logout()` er en ren klient-side operation — der sendes intet til API'et.
- JWT'en forbliver teknisk gyldig på serveren indtil den udløber (HS256, ingen revocation list).
- `apiInterceptor` bruger Angular Router til navigation — ikke `window.location.href` som ved login.

---

## Relaterede use cases

- [UC-02 — Log ind](UC-02-log-ind.md)
- [UC-04 — Opdater Profil](UC-04-opdater-profil.md) *(ikke oprettet endnu)*
