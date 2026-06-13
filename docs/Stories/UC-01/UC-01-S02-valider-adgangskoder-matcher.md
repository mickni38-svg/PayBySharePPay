# UC-01-S02 — Valider at adgangskoder matcher

**Use Case:** [UC-01 — Opret Bruger](../../usecases/UC-01-opret-bruger.md)  
**Type:** Frontend-validering  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der er ved at registrere sig  
Vil jeg se en tydelig fejlbesked, hvis mine to adgangskoder ikke stemmer overens  
Så jeg ikke ved et uheld opretter en konto med en forkert adgangskode.

---

## Acceptkriterier

- [ ] Hvis `adgangskode` og `gentag adgangskode` ikke er identiske, forhindres indsendelse af formularen.
- [ ] Fejlbeskeden *"Adgangskoderne stemmer ikke overens."* vises inline under det andet adgangskode-felt.
- [ ] Der foretages **intet** API-kald, så længe adgangskoderne ikke matcher.
- [ ] Fejlbeskeden forsvinder, når brugeren retter adgangskoden, så de matcher.

---

## Tekniske detaljer

- **Validering:** Frontend-only (Angular reaktiv formvalidering)
- **Ingen server-roundtrip** ved mismatch
- Berørt komponent: registreringsformular i `Frontend.PayBySharePay`

---

## Relaterede stories

- [UC-01-S01 — Registrer privat bruger](UC-01-S01-registrer-privat-bruger.md)
