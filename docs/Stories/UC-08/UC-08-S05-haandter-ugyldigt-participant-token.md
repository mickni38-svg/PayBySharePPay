# UC-08-S05 — Håndter ugyldigt participant-token

**Use Case:** [UC-08 — Bestil via Merchant-link](../../usecases/UC-08-bestil-via-merchant-link.md)  
**Type:** Undtagelsesforløb (E1) / Gap-story (G1)  
**Status:** ❌ Ikke korrekt implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som en bruger der forsøger at bestille med et ugyldigt eller forkert token  
Vil jeg modtage en klar `401 Unauthorized`-fejl  
Så jeg ikke ser en generisk `500 Internal Server Error`.

---

## Baggrund

Hvis `participantToken` ikke matcher en `OrderParticipant`, kaster servicen `UnauthorizedAccessException`. `ExceptionHandlingMiddleware` mapper ikke denne exception — den bobler op som `HTTP 500` i stedet for `HTTP 401` eller `HTTP 403`. Dette er både et sikkerhedsmæssigt og brugermæssigt problem (G1 i UC-08).

---

## Acceptkriterier

- [ ] `ExceptionHandlingMiddleware` mapper `UnauthorizedAccessException` til `HTTP 401 Unauthorized`.
- [ ] Response-body indeholder en beskrivende fejlbesked: *"Ugyldigt eller udløbet bestillingslink."*
- [ ] Merchant Demo viser en brugervenlig fejlbesked når token er ugyldigt.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `ExceptionHandlingMiddleware.cs` | Tilføj mapping: `UnauthorizedAccessException` → `HTTP 401` |

---

## Relaterede stories

- [UC-08-S02 — Indsend bestilling fra merchant-siden](UC-08-S02-indsend-bestilling-fra-merchant-siden.md)
