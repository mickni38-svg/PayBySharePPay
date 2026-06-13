# UC-01-S08 — Sæt merchant GroupOrderUrl ved oprettelse

**Use Case:** [UC-01 — Opret Bruger](../usecases/UC-01-opret-bruger.md)  
**Type:** Gap-story (G3)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som et nyoprettet spisested  
Vil jeg have en unik bestillingslink (`GroupOrderUrl`) tildelt automatisk ved registrering  
Så jeg er klar til at modtage gruppeordrer uden manuel opsætning efterfølgende.

---

## Baggrund

`Participant.GroupOrderUrl` er `null` for alle nyoprettede merchants. Dette forhindrer merchants i at blive brugt i ordrer, da `GroupOrderUrl` kræves i order-flowet. I dag kræves manuel opdatering via seed/admin. (G3 i UC-01).

---

## Acceptkriterier

- [ ] Når en merchant oprettes, genereres og gemmes en unik `GroupOrderUrl` automatisk.
- [ ] `GroupOrderUrl` er tilgængelig umiddelbart efter `POST /api/auth/register-merchant` returnerer.
- [ ] URL-formatet er konsistent med eksisterende merchants (fx baseret på firmanavn eller GUID).

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `ParticipantService.CreateMerchantAsync()` | Generer og sæt `GroupOrderUrl` ved oprettelse |

---

## Relaterede stories

- [UC-01-S04 — Registrer merchant](UC-01-S04-registrer-merchant.md)
