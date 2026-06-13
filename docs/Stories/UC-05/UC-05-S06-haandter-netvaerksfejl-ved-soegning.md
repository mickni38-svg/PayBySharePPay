# UC-05-S06 — Håndter netværksfejl ved søgning

**Use Case:** [UC-05 — Find Deltagere og Tilføj Ven](../../usecases/UC-05-find-deltagere-tilfoj-ven.md)  
**Type:** Undtagelsesforløb (E4)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der søger efter deltagere  
Vil jeg se en venlig fejlbesked, hvis API'et er utilgængeligt  
Så jeg ved at problemet er midlertidigt og kan prøve igen.

---

## Acceptkriterier

- [ ] Hvis `GET /api/directory/search` fejler, vises: *"Kunne ikke hente deltagere. Prøv igen."*
- [ ] Fejlbeskeden vises i stedet for søgeresultatlisten.
- [ ] Brugeren kan prøve at søge igen uden at genindlæse siden.

---

## Tekniske detaljer

- **Frontend:** `errorMessage.set('Kunne ikke hente deltagere. Prøv igen.')` i error-handler

---

## Relaterede stories

- [UC-05-S01 — Søg efter deltagere i kataloget](UC-05-S01-soeg-efter-deltagere-i-kataloget.md)
