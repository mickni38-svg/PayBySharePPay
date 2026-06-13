# UC-07-S03 — Se capture-status for ordre

**Use Case:** [UC-07 — Se Ordrer og Ordreoverblik](../../usecases/UC-07-se-ordrer-og-overblik.md)  
**Type:** Alternativt forløb  
**Status:** ✅ Implementeret (bruges ikke af Angular-frontend)  

---

## Beskrivelse

Som en logget-ind bruger  
Vil jeg kunne hente betalingsstatus pr. deltager for en ordre  
Så jeg kan se hvem der har betalt og hvem der mangler.

---

## Acceptkriterier

- [ ] `GET /api/orders/{id}/capture-status` returnerer `CaptureStatusDto` med status pr. deltager.
- [ ] Returnerer `HTTP 404` hvis ordre-ID ikke eksisterer.
- [ ] Endpointet er JWT-beskyttet.

---

## Tekniske detaljer

- **API:** `GET /api/orders/{id}/capture-status` — JWT-beskyttet
- **Response:** `CaptureStatusDto`

---

## Kendte gaps

| Gap | Beskrivelse | Prioritet |
|-----|-------------|-----------|
| G5 | Angular-frontend bruger `overview`-endpoint til betalingsstatus i stedet for `capture-status` — endpoint er ubrugt | 🟢 Lav |

---

## Relaterede stories

- [UC-07-S02 — Se ordreoverblik for én ordre](UC-07-S02-se-ordreoverblik.md)
