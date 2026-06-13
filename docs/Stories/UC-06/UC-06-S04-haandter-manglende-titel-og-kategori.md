# UC-06-S04 — Håndter manglende titel og kategori

**Use Case:** [UC-06 — Opret Ordre](../../usecases/UC-06-opret-ordre.md)  
**Type:** Undtagelsesforløb (E1)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en host der forsøger at oprette en ordre uden titel eller kategori  
Vil jeg se en fejlbesked  
Så jeg ved at mindst ét af felterne skal udfyldes.

---

## Acceptkriterier

- [ ] Hvis både `title` og `category` er tomme, returnerer API `HTTP 400 Bad Request`.
- [ ] Fejlbeskeden er: *"En ordre skal have en titel eller kategori."*
- [ ] Frontend viser fejlbeskeden i wizard-trin 1.
- [ ] Formularen forbliver udfyldt så host kan rette fejlen.

---

## Tekniske detaljer

- **Service:** `ArgumentException("En ordre skal have en titel eller kategori.")` kastes
- **Middleware:** `ExceptionHandlingMiddleware` mapper til `HTTP 400`

---

## Relaterede stories

- [UC-06-S01 — Opret gruppeordre med merchant](UC-06-S01-opret-ordre-med-merchant.md)
