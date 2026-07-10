# UC-16 -- Vipps Testbruger Mapping (Sandbox)

**Version:** 1.0  
**Kilde:** Reverse-engineered fra kodebase  
**Sidst opdateret:** 2026-07  
**Miljoee:** Kun sandbox/test -- ikke til produktion

---

## Overblik

| Felt | Vaerdi |
|------|--------|
| Use Case ID | UC-16 |
| Navn | Vipps Testbruger Mapping |
| Primaer aktoer | Logget-ind bruger (Person, sandbox-miljoee) |
| Formaal | Mappe en PayNSync-konto til en Vipps sandbox-testbruger saa betalinger kan simuleres korrekt i MobilePay/Vipps test-miljoeet |
| Trigger | Brugeren aabner profilsiden og vaelger en Vipps testperson fra listen |

---

## Baggrund

I Vipps/MobilePay sandbox-miljoeet er testbrugere praedefinerede telefonnumre. Den bruger der er logget ind i PayNSync skal mappe sit login til en af disse testbrugere for at Vipps kan associere betalingen med den rigtige sandbox-konto. Mappingen gemmes som en nullable self-referencing FK (VippsTestUserId) paa Participant-tabellen.

---

## Aktoerer

| Aktoer | Rolle |
|--------|-------|
| **Person** | Bruger i sandbox der vil vaelge sin testidentitet |
| **Frontend** | Angular profilside -- viser listen og sender PATCH |
| **API** | ParticipantsController -- haandterer laesning og opdatering |
| **Database** | SQL Server -- gemmer VippsTestUserId paa Participant |

---

## Praekonditioner

- Brugeren er logget ind
- Systemet korer i sandbox-miljoee (Payments:Provider = MobilePay eller lokal test)
- Der er oprettet Person-Participants i databasen der repraesenterer Vipps testbrugere (typisk via seed-data)

---

## Postkonditioner

- Participant.VippsTestUserId peger paa en anden Participants ID
- Fremtidige betalingsreservationer bruger testbrugerens telefonnummer (fra den mappede Participant) som Vipps-kundereference
- Den valgte testperson er markeret som optaget (disabled) for andre brugere i UI

---

## Normalforlob

| Trin | Aktoer | Handling |
|------|--------|----------|
| 1 | Bruger | Navigerer til /profile |
| 2 | Frontend | GET /api/participants/vipps-test-users henter alle Person-Participants |
| 3 | Service | Bygger liste med MappedByParticipantId for hver person (hvem har valgt hvem) |
| 4 | Frontend | Viser liste med radioknapper; allerede-valgte er disabled |
| 5 | Bruger | Vaelger en testperson |
| 6 | Frontend | PATCH /api/participants/{id}/vipps-test-user med { vippsTestUserId: X } |
| 7 | API | SetVippsTestUserAsync opdaterer VippsTestUserId paa den loggede brugers Participant |
| 8 | API | 204 No Content |
| 9 | Frontend | Opdaterer UI saa den valgte testperson er markeret aktiv |

---

## Alternative forlob

### A1 -- Fjern mapping
- Bruger vaelger Ingen/fjern-knap
- PATCH med { vippsTestUserId: null }
- VippsTestUserId saettes til null; testpersonen bliver tilgaengelig for andre igen

### A2 -- Testperson allerede valgt af anden bruger
- Testpersonen vises disabled i listen
- Brugeren kan ikke vaelge den

---

## Undtagelsesforlob

| Undtagelse | Haandtering |
|------------|-------------|
| Participant ikke fundet | 404 Not Found fra API (InvalidOperationException kastes i service) |

---

## Datamodel

### Participant-entity (selvrefererende FK)
| Felt | Type | Note |
|------|------|------|
| VippsTestUserId | int? | FK til Participant.Id; nullable; DeleteBehavior.ClientSetNull |

### VippsTestPersonDto (respons fra GET /api/participants/vipps-test-users)
| Felt | Type | Note |
|------|------|------|
| Id | int | Testpersonens Participant-ID |
| Name | string | Testpersonens navn |
| Phone | string? | Testpersonens telefonnummer (bruges af Vipps) |
| MappedByParticipantId | int? | ID paa den bruger der har valgt denne testperson; null = ledig |

---

## API-endpoints

| Metode | URL | Auth | Beskrivelse |
|--------|-----|------|-------------|
| GET | /api/participants/vipps-test-users | Anonym | Hent alle Participants med mapping-info |
| PATCH | /api/participants/{id}/vipps-test-user | Anonym | Saet eller fjern VippsTestUserId |

---

## Implementeringsstatus

| Del | Status | Note |
|-----|--------|------|
| Participant.VippsTestUserId self-ref FK | implementeret | DeleteBehavior.ClientSetNull; migration kort |
| GET /api/participants/vipps-test-users | implementeret | Returnerer VippsTestPersonDto med MappedByParticipantId |
| PATCH /api/participants/{id}/vipps-test-user | implementeret | Saetter eller nuller VippsTestUserId |
| Frontend profilside -- mapping UI | implementeret | Radioknapper med eksklusivt valg og disabled-tilstand |
| Brug af mapping ved Vipps reservation | delvist | Payment provider skal slaa telefon op fra VippsTestUserId-mappingen |

---

## Kendte mangler og gaps

| Gap | Prioritet |
|-----|-----------|
| Feature er kun til sandbox; bor fjernes eller skjules i produktion | Lav |
| Ingen [Authorize] paa PATCH-endpoint | Hoj |
| Ingen validering af at valgte VippsTestUserId faktisk eksisterer | Medium |

---

## Tekniske noter

- VippsTestUserId peger paa en anden Person-Participants ID (samme tabel, selvrefererende)
- Eksklusivt valg haandteres udelukkende i frontend via MappedByParticipantId; backend tillader i princippet at to brugere vaelger samme testperson
- Seed-data for testpersoner oprettes typisk via DevController (UC-15) eller manuelt i databasen
- Loesningen er en sandbox workaround; produktionsmiljoeet bruger rigtige Vipps-kundetelefonnumre

---

## Relaterede use cases

- UC-04: Opdater Profil (mapping haandteres paa profilsiden)
- UC-09: Reserver Betaling (bruger VippsTestUserId-mappingen)
- UC-15: Dev og Seed Tools (opretter testpersoner)
