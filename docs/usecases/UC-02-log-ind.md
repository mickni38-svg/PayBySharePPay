# UC-02 – Log Ind

**Version:** 2.0  
**Kilde:** Reverse-engineered fra kodebase  
**Sidst opdateret:** 2026-07  

---

## Overblik

| Felt | Vaerdi |
|------|--------|
| Use Case ID | UC-02 |
| Navn | Log Ind |
| Primaer aktoer | Registreret bruger (Person) |
| Formaal | Autentificere en bruger og udstede et JWT saa brugeren kan tilgaa beskyttede ressourcer |
| Trigger | Brugeren navigerer til `/login` og udfylder loginformular eller klikker Fortsaet med Google |

---

## Aktoerer

| Aktoer | Rolle |
|--------|-------|
| **Person** | Bruger der oensker at logge ind |
| **API** | `Api.PayBySharePay` validerer credentials og udsteder JWT |
| **Google Identity Services (GIS)** | Udsteder Google ID-token paa klienten (Google flow) |
| **Frontend** | Angular SPA sender token til API og gemmer JWT |

---

## Praekonditioner

- Brugeren har en eksisterende Participant-konto (oprettet via UC-01)
- Brugeren er IKKE allerede logget ind

---

## Postkonditioner

- Brugeren modtager et JWT (480 min. gyldighed)
- JWT gemmes i localStorage i Angular
- Brugeren omdirigeres til /home

---

## Normalforlob -- Email/Password login

1. Bruger navigerer til /login
2. Bruger indtaster e-mail og password
3. Angular sender POST /api/auth/login med { email, password }
4. API slaar Participant op paa e-mail
5. Hvis PasswordHash er sat: verificer password med BCrypt
6. API genererer JWT med ParticipantId og Name
7. API returnerer { token, participantId, name, expiresAt }
8. Frontend gemmer JWT i localStorage og navigerer til /home

---

## Normalforlob -- Google login

1. Bruger klikker Fortsaet med Google paa /login
2. Google Identity Services (GIS) viser Google-konto-vaelger
3. GIS returnerer et Google ID-token til Angular
4. Angular sender POST /api/auth/google-login med { idToken }
5. API validerer token via GoogleJsonWebSignature.ValidateAsync
6. API slaar ParticipantExternalLogin op paa Provider=Google + ProviderUserId
7. Hvis bruger ikke eksisterer: opret ny Participant og link via ParticipantExternalLogin
8. API genererer JWT med ParticipantId og Name
9. API returnerer { token, participantId, name, expiresAt }
10. Frontend gemmer JWT i localStorage og navigerer til /home

---

## Alternative forlob

### A1 -- Forkert password (email/password flow)
- Trin 5: password matcher ikke, API returnerer 401 Unauthorized
- Frontend viser fejlbesked

### A2 -- Ukendt e-mail (email/password flow)
- Trin 4: ingen Participant fundet, 401 Unauthorized
- Frontend viser fejlbesked

### A3 -- Google-konto er linket til anden PayNSync-bruger
- Trin 6: e-mail fra Google-token matcher eksisterende Participant men med anden provider, 409 Conflict
- Frontend viser fejl: E-mail er allerede tilknyttet en anden konto

### A4 -- Brugeren afviser Google-loginprompt
- GIS viser intet, flow stopper uden fejl

---

## Undtagelsesforlob

| Undtagelse | Haandtering |
|------------|-------------|
| Ugyldigt Google ID-token (expired, forkert audience) | 400 Bad Request, brugeren bedes proeve igen |
| Netvaerksfejl under GIS-validering | 400 Bad Request, fejl logges serverside |

---

## Datamodel

| Entitet | Aendring |
|---------|----------|
| Participant | Laeses (PasswordHash valideres, eller ny oprettes ved Google-login) |
| ParticipantExternalLogin | Laeses ved Google-login; oprettes ved foerste Google-login |

---

## API-endpoints

| Metode | URL | Auth | Beskrivelse |
|--------|-----|------|-------------|
| POST | /api/auth/login | Anonym | Email/password login |
| POST | /api/auth/google-login | Anonym | Google ID-token login |

---

## Implementeringsstatus

| Del | Status | Note |
|-----|--------|------|
| Email/password login (POST /api/auth/login) | implementeret | BCrypt-validering; 480 min JWT |
| Google login (POST /api/auth/google-login) | implementeret | GoogleJsonWebSignature.ValidateAsync, ParticipantExternalLogin |
| Frontend Angular login-side | implementeret | Standalone component, signaler, GIS-knap |
| JWT gemmes i localStorage | implementeret | AuthService i Angular |
| Apple Login | ikke implementeret | Ikke planlagt endnu |

---

## Kendte mangler og gaps

| Gap | Prioritet |
|-----|-----------|
| Ingen Refresh Token, brugeren logges automatisk ud efter 480 min | Medium |
| Apple Login ikke implementeret | Lav |
| Ingen rate limiting paa login-endpoint | Medium |

---

## Tekniske noter

- IExternalAuthService.GoogleLoginAsync haandterer hele Google-flow: validering, opslag, oprettelse og link
- Undtagelsestype ExternalLoginEmailConflictException bruges til at skelne e-mail-konflikt fra andre fejl
- GIS kraever HTTPS; lokal dev bruger mkcert-genererede certifikater
- JWT udstedt med JwtTokenService.GenerateToken(participantId, name); gyldighed 480 min

---

## Relaterede use cases

- UC-01: Opret Bruger (forudsaetning for email/password login)
- UC-03: Log Ud
- UC-04: Opdater Profil
