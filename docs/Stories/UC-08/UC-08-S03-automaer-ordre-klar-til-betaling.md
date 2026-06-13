# UC-08-S03 — Automær ordre som klar til betaling når alle har bestilt

**Use Case:** [UC-08 — Bestil via Merchant-link](../../usecases/UC-08-bestil-via-merchant-link.md)  
**Type:** Alternativt forløb (A1)  
**Status:** ✅ Implementeret  

---

## Beskrivelse

Som host for en gruppeordre  
Vil jeg automatisk modtage en besked når alle deltagere har bestilt  
Så jeg ved hvornår jeg kan gennemføre betalingen.

---

## Acceptkriterier

- [ ] Når den sidstæ deltager indsender sin bestilling, tjekker systemet om alle ikke-merchant deltagere har `Status = "OrderSubmitted"`.
- [ ] Hvis alle har bestilt, sættes `Order.Status` til `"ReadyToPay"`.
- [ ] Host modtager en systembesked: *"Alle deltagere har bestilt — du kan nu gennemføre betalingen."*

---

## Tekniske detaljer

- **Service:** `CheckAndSetReadyToPayAsync(orderId)` — kaldes efter hvert vellykket submit
- Kun deltagere af type `Person` (ikke `Merchant`) tages i betragtning

---

## Relaterede stories

- [UC-08-S02 — Indsend bestilling fra merchant-siden](UC-08-S02-indsend-bestilling-fra-merchant-siden.md)
