# UC-04 -- Opdater Profil

**Version:** 2.0  
**Kilde:** Reverse-engineered fra kodebase  
**Sidst opdateret:** 2026-07  

---

## Overblik

| Felt | Vaerdi |
|------|--------|
| Use Case ID | UC-04 |
| Navn | Opdater Profil |
| Primaer aktoer | Logget-ind bruger (Person) |
| Formaal | AEndre navn, e-mail og/eller telefonnummer paa sin egen konto; samt (sandbox) mappe sin konto til en Vipps testbruger |
| Trigger | Brugeren navigerer til /profile og redigerer sine oplysninger |

---

## Aktoerer

| Aktoer | Rolle |
|--------|-------|
| **Person** | Bruger der vil opdatere sine profiloplysninger |
| **API** | Api.PayBySharePay -- modtager og gemmer aendringer |
| **Database** | SQL Server -- opdaterer Participant-record |

---

## Praekonditioner

- Brugeren er logget ind (JWT i localStorage)
- AuthService.currentUserId() returnerer brugerens ID

---

## Postkonditioner

- Participant-record er opdateret i databasen
- Profilsiden viser en groen succesbesked i 3 sekunder
- Brugerens navn, e-mail og telefon vises opdateret i formularen

---

## Normalforlob -- Rediger profiloplysninger

| Trin | Aktoer | Handling |
|------|--------|----------|
| 1 | Bruger | Navigerer til /profile |
| 2 | Frontend | ProfileComponent henter eksisterende profil via GET /api/participants/{id} |
| 3 | Frontend | Formularfelter udfyldes med: navn, e-mail, telefon |
| 4 | Bruger | Redigerer et eller flere felter |
| 5 | Bruger | Trykker Gem aendringer |
| 6 | Frontend | Validerer at navn ikke er tomt |
| 7 | Frontend | ProfileService.updateProfile(id, { name, email?, phone? }) -- PUT /api/participants/{id}/profile |
| 8 | API | ParticipantsController.UpdateProfile() validerer at navn ikke er tomt |
| 9 | API | Kalder ParticipantService.UpdateProfileAsync() |
| 10 | Service | Validerer navn, slaar Participant op via ID |
| 11 | Service | Opdaterer Name, Email, Phone paa entity |
| 12 | Service | Gemmer aendringer i databasen |
| 13 | API | Returnerer opdateret ParticipantDto (200 OK) |
| 14 | Frontend | Opdaterer formular og session; kalder auth.updateStoredName() |
| 15 | Frontend | Viser succesbesked i 3 sekunder |

---

## Normalforlob -- Map Vipps testbruger (sandbox)

| Trin | Aktoer | Handling |
|------|--------|----------|
| 1 | Bruger | Navigerer til /profile |
| 2 | Frontend | Henter liste over Vipps testpersoner via GET /api/participants/vipps-test-users |
| 3 | Frontend | Viser liste med radioknapper; allerede-valgte er disabled (eksklusivt valg) |
| 4 | Bruger | Vaelger en testperson fra listen |
| 5 | Frontend | PATCH /api/participants/{id}/vipps-test-user med { vippsTestUserId: X } |
| 6 | API | SetVippsTestUser kalder ParticipantService.SetVippsTestUserAsync |
| 7 | Service | Opdaterer VippsTestUserId paa Participant-entity |
| 8 | API | Returnerer 204 No Content |
| 9 | Bruger | Kan nu reservere betaling med den valgte testbrugers Vipps-konto |

---

## Alternative forlob

### A1 -- Tomt navn
- Trin 6: navn er tomt, 400 Bad Request
- Frontend viser fejlbesked

### A2 -- Bruger ikke fundet
- Trin 10: GetByIdAsync() returnerer null, 404 Not Found

### A3 -- Vipps testbruger allerede valgt af en anden
- Testpersonen vises disabled i UI saa en anden ikke kan vaelge den samme
- Eksklusivt valg haandteres ved at hente VippsTestUserId fra alle Participants og markere optagne som disabled

### A4 -- Fjern Vipps testbruger-mapping
- Bruger vaelger Ingen/null; PATCH med { vippsTestUserId: null }
- VippsTestUserId saettes til null paa entity

---

## Undtagelsesforlob

| Undtagelse | Haandtering |
|------------|-------------|
| Netvaerksfejl | Frontend saetter saveError.set(true) |
| Vipps testbruger-ID eksisterer ikke | 404 Not Found fra API |

---

## Datamodel

### Request -- PUT /api/participants/{id}/profile
| Felt | Type | Paakraevet | Validering |
|------|------|------------|------------|
| name | string | ja | Ikke-tom |
| email | string? | nej | Ingen format-validering i UpdateProfileRequest |
| phone | string? | nej | Ingen format-validering |

### Request -- PATCH /api/participants/{id}/vipps-test-user
| Felt | Type | Note |
|------|------|------|
| vippsTestUserId | int? | null for at fjerne mapping |

### Response -- ParticipantDto
| Felt | Type |
|------|------|
| id | int |
| type | string (Person) |
| name | string |
| email | string? |
| phone | string? |

---

## API-endpoints

| Endpoint | Metode | Auth | Response |
|----------|--------|------|----------|
| GET /api/participants/{id} | GET | Anonym | 200 + ParticipantDto, 404 |
| PUT /api/participants/{id}/profile | PUT | Anonym | 200 + ParticipantDto, 400, 404 |
| GET /api/participants/vipps-test-users | GET | Anonym | 200 + liste af testpersoner |
| PATCH /api/participants/{id}/vipps-test-user | PATCH | Anonym | 204, 404 |

---

## Implementeringsstatus

| Del | Status | Detaljer |
|-----|--------|----------|
| Frontend -- profilformular | implementeret | Navn, e-mail, telefon + gem-knap |
| Frontend -- hent profil ved load | implementeret | GET /api/participants/{id} via ProfileService |
| Frontend -- gem profil | implementeret | PUT /api/participants/{id}/profile |
| Frontend -- notifikations-toggle | implementeret | Lokal localStorage-praeferece (ingen API) |
| Frontend -- succesbesked (3 sek.) | implementeret | saveSuccess signal + setTimeout |
| Frontend -- navn synkroniseres i session | implementeret | auth.updateStoredName() |
| Frontend -- Vipps testbruger mapping UI | implementeret | Radioknapper med eksklusivt valg |
| API -- GET /api/participants/{id} | implementeret | Returnerer ParticipantDto |
| API -- PUT /api/participants/{id}/profile | implementeret | Validering + opdatering |
| API -- PATCH /api/participants/{id}/vipps-test-user | implementeret | SetVippsTestUserAsync |
| API -- GET /api/participants/vipps-test-users | implementeret | GetVippsTestPersonsAsync |
| Auth-krav paa endpoints | ikke implementeret | Alle endpoints uden [Authorize] |

---

## Kendte mangler og gaps

| # | Mangel | Prioritet |
|---|--------|-----------|
| G1 | Ingen [Authorize] paa profil-endpoints | Hoj |
| G2 | Ingen ejerskabsvalidering (bruger kan aendre andens profil) | Hoj |
| G3 | PasswordHash returneres i ParticipantDto | Hoj |
| G4 | Notifikationer gemmes kun i localStorage | Medium |
| G5 | Ingen e-mail-format-validering i UpdateProfileRequest | Lav |
| G6 | Vipps testbruger mapping er kun til sandbox-brug; ikke for produktion | Lav |

---

## Tekniske noter

- ProfileService og ProfileComponent er separate fra AuthService; bruger participantId fra AuthService.currentUserId()
- Vipps testbruger mapping: VippsTestUserId er en nullable self-referencing FK paa Participant med DeleteBehavior.ClientSetNull
- Eksklusivt valg af testbruger: frontend henter alle Participants VippsTestUserId og markerer allerede-valgte som disabled
- ParticipantsController har ingen class-level [Authorize]; dette er et gennemgaaende sikkerhedsproblem

---

## Relaterede use cases

- UC-02: Log Ind
- UC-03: Log Ud
- UC-05: Find Deltagere og Tilfoj Ven
- UC-16: Vipps Testbruger Mapping (sandbox-detaljer)
