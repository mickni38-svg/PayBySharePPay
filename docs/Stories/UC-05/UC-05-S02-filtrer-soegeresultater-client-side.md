# UC-05-S02 — Vis og filtrer søgeresultater client-side

**Use Case:** [UC-05 — Find Deltagere og Tilføj Ven](../../usecases/UC-05-find-deltagere-tilfoj-ven.md)  
**Type:** Alternativt forløb (A3)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der søger i kataloget  
Vil jeg se resultater filtrere sig i realtid mens jeg skriver i søgefeltet  
Så jeg hurtigt kan indsnævre listen uden at vente på serveren.

---

## Acceptkriterier

- [ ] Søgefeltet filtrerer de allerede hentede resultater client-side på `displayName`, `handle` og `subtitle`.
- [ ] Filtrering sker uden nyt API-kald ved hvert tastetryk.
- [ ] Nyt API-kald til serveren sker kun når brugeren trykker på søge-knappen (`onSearch()`).
- [ ] Tabs (**Venner** / **Brugere** / **Spisestedet**) viser korrekt filtrerede resultater.

---

## Tekniske detaljer

- **Frontend:** `filtered()` computed signal filtrerer `entries` lokalt
- **Tabs:** `merchantTabEntries` computed signal viser merchants der ikke allerede er venner

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G5 | Client-side filtrering henter ikke opdaterede data fra server ved hvert tastetryk — nye brugere dukker ikke op før næste `onSearch()` | 🟢 Lav |

---

## Relaterede stories

- [UC-05-S01 — Søg efter deltagere i kataloget](UC-05-S01-soeg-efter-deltagere-i-kataloget.md)
- [UC-05-S03 — Markér og tilføj venner](UC-05-S03-marker-og-tilfoj-venner.md)
