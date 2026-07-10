# UC-03 -- Log Ud

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Sidst opdateret:** 2026-07  

---

## Overblik

| Felt | Vaerdi |
|------|--------|
| Use Case ID | UC-03 |
| Navn | Log Ud |
| Primaer aktoer | Logget-ind bruger (Person) |
| Formaal | Rydde den lokale session saa brugeren ikke laengere har adgang til beskyttede ressourcer |
| Trigger | Brugeren klikker logout i menuen, ELLER API returnerer 401 paa en autentificeret anmodning |

---

## Aktoerer

| Aktoer | Rolle |
|--------|-------|
| **Person** | Bruger der logger ud |
| **Frontend** | Angular SPA sletter lokal session og omdirigerer |
| **API** | Ingen aktiv rolle; JWT er stateless og invalideres ikke serverside |

---

## Praekonditioner

- Brugeren er logget ind (JWT i localStorage)

---

## Postkonditioner

- JWT og brugerinfo er slettet fra localStorage
- Angular-signaler (_token, _user) er sat til null
- Brugeren er omdirigeret til /login

---

## Normalforlob -- Manuel logout

1. Bruger klikker logout-knap i navbar/menu
2. Angular kalder AuthService.logout()
3. localStorage.removeItem(TOKEN_KEY) og removeItem(USER_KEY) kores
4. Signalerne _token og _user saettes til null
5. Brugeren omdirigeres til /login

---

## Normalforlob -- Automatisk logout (401)

1. Angular sender en autentificeret HTTP-anmodning med udloebet eller ugyldigt JWT
2. API returnerer 401 Unauthorized
3. ApiInterceptor fanger 401-fejlen
4. Interceptor kalder AuthService.logout()
5. Interceptor navigerer brugeren til /login

---

## Alternative forlob

### A1 -- Brugeren er allerede logget ud
- AuthService.logout() kores uden fejl; localStorage.removeItem er idempotent

---

## Undtagelsesforlob

| Undtagelse | Haandtering |
|------------|-------------|
| localStorage er ikke tilgaengeligt (private mode) | Logout mislykkedes stille; signal naes null; navigation sker stadig |

---

## Datamodel

Ingen serverside aendringer. Logout er rent klientside.

| Entitet | Aendring |
|---------|----------|
| localStorage (TOKEN_KEY, USER_KEY) | Slettes |
| Angular signaler (_token, _user) | Saettes til null |

---

## API-endpoints

Ingen. JWT er stateless. Der er ingen server-logout endpoint.

---

## Implementeringsstatus

| Del | Status | Note |
|-----|--------|------|
| Manuel logout via AuthService.logout() | implementeret | Sletter localStorage og signals |
| Automatisk logout ved 401 via ApiInterceptor | implementeret | api.interceptor.ts haandterer 401 |
| Redirect til /login | implementeret | Router.navigate(['/login']) i interceptor |
| Server-side token invalidering | ikke implementeret | JWT er stateless; ingen blacklist |

---

## Kendte mangler og gaps

| Gap | Prioritet |
|-----|-----------|
| Ingen server-side token invalidering/blacklist | Lav (ikke kritisk for MVP) |
| JWT udloeber ikke foer 480 min; token er gyldigt til sin expiresAt selv efter klientside logout | Medium |

---

## Tekniske noter

- Logout er rent klientside; backend har ingen notion om aktive sessioner
- API interceptor ligger i api.interceptor.ts og er registreret som HttpInterceptorFn
- Token-noeglen og bruger-noeglen er konstanter defineret i auth.service.ts

---

## Relaterede use cases

- UC-02: Log Ind (forudsaetning)
- UC-04: Opdater Profil
