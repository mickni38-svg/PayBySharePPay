# UC-06-S08 — Implementer join via JoinToken

**Use Case:** [UC-06 — Opret Ordre](../../usecases/UC-06-opret-ordre.md)  
**Type:** Gap-story (G1)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som en bruger der har modtaget et join-link til en gruppeordre  
Vil jeg kunne tilmelde mig ordren via linket  
Så jeg kan deltage i ordren uden at blive manuelt inviteret af host.

---

## Baggrund

`Order.JoinToken` er et GUID der genereres ved oprettelse, men der eksisterer intet endpoint til at bruge det. Tokenet er ikke eksponeret i nogen DTO og kan ikke tilgås af frontend. Det er dermed en ikke-funktionel feature (G1 i UC-06).

---

## Acceptkriterier

- [ ] `GET /api/orders/join/{joinToken}` returnerer ordreoplysninger for en gyldig token.
- [ ] En ny `OrderParticipant` oprettes med status `Invited` og unikt `ParticipantToken`.
- [ ] `JoinToken` eksponeres i `OrderDto` så frontend kan generere et join-link.
- [ ] Endpoint håndterer ugyldigt/udløbet token med `HTTP 404`.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `OrderDto` | Inkluder `JoinToken`-felt |
| `OrdersController` | Tilføj `GET /api/orders/join/{joinToken}` endpoint |
| `OrderService` | Tilføj `JoinOrderByTokenAsync(joinToken, participantId)` metode |

---

## Relaterede stories

- [UC-06-S01 — Opret gruppeordre med merchant](UC-06-S01-opret-ordre-med-merchant.md)
- [UC-06-S07 — Tilføj deltagere til eksisterende ordre](UC-06-S07-tilfoj-deltagere-til-eksisterende-ordre.md)
