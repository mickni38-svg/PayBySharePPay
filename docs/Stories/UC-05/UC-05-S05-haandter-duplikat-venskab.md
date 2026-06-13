# UC-05-S05 — Håndter duplikat venskab

**Use Case:** [UC-05 — Find Deltagere og Tilføj Ven](../../usecases/UC-05-find-deltagere-tilfoj-ven.md)  
**Type:** Undtagelsesforløb (E1)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som en bruger der forsøger at tilføje en person der allerede er ven  
Vil jeg se en klar fejlbesked  
Så jeg forstår at relationen allerede eksisterer.

---

## Acceptkriterier

- [ ] Hvis venskabet allerede eksisterer, returnerer API `HTTP 409 Conflict`.
- [ ] Frontend viser: *"En eller flere venner kunne ikke tilføjes. Prøv igen."*
- [ ] De øvrige valgte brugere (der ikke er duplikater) tilføjes stadig.

---

## Tekniske detaljer

- **Service:** `RelationExistsAsync()` → `true` → `InvalidOperationException("Venrelationen eksisterer allerede.")`
- **Middleware:** `ExceptionHandlingMiddleware` mapper til `HTTP 409`
- **Frontend:** `hasError = true`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G3 | Ingen UNIQUE-constraint på `FriendRelation` i databasen — race conditions mulige ved parallelle kald | 🟡 Medium |

---

## Relaterede stories

- [UC-05-S03 — Markér og tilføj venner](UC-05-S03-marker-og-tilfoj-venner.md)
