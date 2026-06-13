# UC-07-S05 — Tilføj real-time opdatering af ordreoverblik

**Use Case:** [UC-07 — Se Ordrer og Ordreoverblik](../../usecases/UC-07-se-ordrer-og-overblik.md)  
**Type:** Gap-story (G3)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🟡 Medium  

---

## Beskrivelse

Som en bruger der ser et ordreoverblik  
Vil jeg at siden opdateres automatisk når andre deltagere indsender bestillinger eller betaler  
Så jeg ikke behøver at genindlæse siden manuelt.

---

## Baggrund

Ordreoverblik-siden er statisk efter indlæsning. Hvis en deltager accepterer invitationen, indsender en bestilling eller betaler, ser de andre deltagere ikke ændringen før de genudfører `GET /api/orders/{id}/overview` (G3 i UC-07).

---

## Acceptkriterier

- [ ] Ordreoverblik-siden opdateres automatisk når ordrens data ændres.
- [ ] Løsning kan implementeres som periodisk polling (fx hvert 10. sekund) eller via WebSocket/SignalR.
- [ ] Opdateringen sker uden fuld side-genindlæsning.

---

## Tekniske ændringer

| Tilgang | Beskrivelse |
|---------|-------------|
| **Polling** | Angular-komponent sætter et interval der kalder `GET /api/orders/{id}/overview` periodisk |
| **SignalR** | Server pusher opdateringer til klienter der lytter på en ordre-kanal |

---

## Relaterede stories

- [UC-07-S02 — Se ordreoverblik for én ordre](UC-07-S02-se-ordreoverblik.md)
