# UC-03-S03 — Implementer server-side token invalidering

**Use Case:** [UC-03 — Log ud](../usecases/UC-03-log-ud.md)  
**Type:** Gap-story (G1)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som en bruger der logger ud  
Vil jeg at mit JWT-token øjeblikkeligt bliver ugyldigt på serveren  
Så ingen kan misbruge et stjålet token efter logout.

---

## Baggrund

JWT er stateless — et udstedt token er teknisk gyldigt til udløb, selv efter logout. Klient-side logout fjerner kun token fra `localStorage`, men tokenet kan stadig bruges af en tredjepart der har opsnappet det. Ingen blacklist eller revocation er implementeret (G1 i UC-03).

---

## Acceptkriterier

- [ ] Når `AuthService.logout()` kaldes, sendes et `POST /api/auth/logout`-kald til API'et med det aktuelle token.
- [ ] API'et gemmer token-`jti` (JWT ID) på en blacklist (f.eks. i database eller cache) til tokenets udløbstidspunkt.
- [ ] Efterfølgende API-kald med det invaliderede token afvises med `HTTP 401`, selv om signaturen er gyldig.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `AuthController` | Tilføj `POST /api/auth/logout` endpoint |
| `JwtTokenService` / middleware | Valider `jti` mod blacklist ved hvert request |
| Database/cache | Gem invaliderede `jti`-værdier med TTL |
| `AuthService.logout()` (Angular) | Send logout-kald til API inden lokal rydning |

---

## Relaterede stories

- [UC-03-S01 — Manuel logout fra profilside](UC-03-S01-manuel-logout-fra-profilside.md)
- [UC-03-S02 — Automatisk logout ved udløbet token](UC-03-S02-automatisk-logout-ved-udloebet-token.md)
