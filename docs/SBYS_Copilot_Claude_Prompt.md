# SBYS Copilot / Claude Development Prompt

## Context
Læs hele den uploadede `SBYS_Project_Documentation.md` først.

Du skal hjælpe mig med at udvikle **ShareBySharePay (SBYS)** som en web frontend i mobilvisning, baseret på dokumentationen.

---

# Vigtige regler

- Følg dokumentationen som **source of truth**
- Byg løsningen **step by step**
- Start ikke med hele systemet på én gang
- Foreslå først en konkret implementeringsplan med rækkefølge
- Brug eksisterende backend/API/domain-models hvis de findes
- Hvis noget mangler, så foreslå minimale nødvendige ændringer
- Skriv hele filer når du ændrer kode
- Hold løsningen simpel og MVP-fokuseret
- Undgå over-engineering
- Brug API-first arkitektur
- Web frontend skal fungere godt i mobilvisning
- Merchant/spisested er ikke en normal bruger, men skal vises i samme directory-liste
- SBYS håndterer status/orchestration, ikke selve betalingsudbyderen
- Gruppeordre må først frigives når alle deltagere har reserveret/betalt

---

# Analysefase

## 1. Scan projektet
- Scan hele solution/project-strukturen
- Find eksisterende:
  - Backend/API
  - DTO’er
  - Entities
  - Services
  - Repositories
  - Frontend/pages/components
  - Authentication
  - Migrations
  - Seed-data

---

## 2. Sammenlign kodebase med dokumentationen

Lav en detaljeret gap-analyse opdelt i:

- Backend/API
- Domain entities
- Database/migrations
- Authentication/security
- Merchant integration
- Group payment flow
- Frontend/mobile web UI
- State/status handling
- Notifications
- Payment orchestration
- Missing DTOs/contracts
- Missing services/repositories
- Missing pages/components
- Technical debt / architecture risks

---

## 3. For hver kategori

Beskriv:

### Hvad findes allerede?
### Hvad mangler?
### Hvad bør refaktoreres?
### Prioritet:
- MVP Critical
- MVP Optional
- Later Phase

---

# Udviklingsplan

Lav derefter en konkret udviklingsplan i små trin.

Hvert trin skal:
- være implementerbart på under 1 arbejdsdag
- have klart scope
- liste hvilke filer der skal oprettes/ændres
- forklare afhængigheder
- forklare hvorfor rækkefølgen er vigtig

---

# Implementeringsregler

- Skriv aldrig pseudo-kode
- Skriv aldrig forkortede filer
- Returnér altid hele filer
- Respekter eksisterende namespaces og projektstruktur
- Lav ikke breaking changes uden forklaring
- Introducér ikke nye frameworks uden begrundelse
- Hold styling simpel og mobil-først
- Brug eksisterende patterns hvis de findes
- Hvis noget er uklart, så analyser først før du implementerer

---

# Vigtigt workflow

1. Analysér først
2. Lav gap-analyse
3. Lav udviklingsplan
4. Implementér KUN første trin
5. Vent derefter på næste instruktion

---

# Første mål (MVP)

Byg fundamentet til web frontend i mobilvisning:

- Navigation
- Login placeholder hvis auth ikke findes endnu
- Directory-side med users + merchants
- Create group payment-side
- Overview/status-side
- Merchant/group-order placeholder flow

---

# Første svar skal indeholde

- Hvad du fandt i projektet
- Hvad der mangler
- Hvilken rækkefølge du anbefaler
- Præcis hvilken fil du vil ændre først
- Hvorfor netop den fil er første skridt
