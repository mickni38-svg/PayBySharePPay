# UC-09-S05 — Valider participantId mod JWT ved reservation

**Use Case:** [UC-09 — Reserver Betaling](../../usecases/UC-09-reserver-betaling.md)  
**Type:** Gap-story (G1)  
**Status:** ❌ Ikke implementeret  
**Prioritet:** 🔴 Høj  

---

## Beskrivelse

Som systemets sikkerhedsansvarlige  
Vil jeg at `participantId` i `POST /api/orders/{id}/reserve` valideres mod JWT'ens `sub`-claim  
Så ingen bruger kan reservere en betaling på vegne af en anden deltager.

---

## Baggrund

`POST /api/orders/{id}/reserve` modtager `participantId` i request-body. Selvom endpoint'et er JWT-beskyttet, valideres det ikke at `participantId` matcher det bruger-ID der er indkodet i JWT'ens `sub`-claim. En autentificeret bruger kan dermed reservere for en anden deltager (G1 i UC-09).

---

## Acceptkriterier

- [ ] `OrdersController.ReservePayment()` udleder `participantId` fra `User.FindFirst("sub").Value`.
- [ ] `participantId` fjernes fra request-body.
- [ ] Kald med mismatching token returnerer `HTTP 403 Forbidden`.

---

## Tekniske ændringer

| Fil | Ændring |
|-----|---------|
| `ReservePaymentRequest.cs` | Fjern `ParticipantId`-felt |
| `OrdersController.ReservePayment()` | Udled `participantId` fra JWT `sub`-claim |

---

## Relaterede stories

- [UC-09-S01 — Reserver betaling via Fake provider](UC-09-S01-reserver-betaling-via-fake-provider.md)
