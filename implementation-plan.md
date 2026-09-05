# Implementation plan — UC-20

## Task

- Use case: `docs/usecases/UC-20-realistisk-merchant-takeaway-demo.md`
- Type: `NEW_USE_CASE`
- Goal: Erstat den simple Pizzeria Roma-side med en realistisk takeaway-demo, som sender deltagerens draft til det eksisterende PayNSync-reservationsflow.
- Approval: Product Owner har efter gennemlæsning bedt om implementering af UC-20.

## Current state

- Merchant Demo er én statisk `index.html` med otte checkbox-produkter og inline CSS/JavaScript.
- Siden kalder allerede `POST /api/merchant-orders` og følger `paymentRedirectUrl` til MobilePay/Vipps.
- API-kontrakten understøtter normaliserede linjer og `RawMerchantPayloadJson`.
- Der findes ingen produktmængder, tilvalg, rigtig kurv eller stabile katalog-id'er.

## Scope and decisions

- Behold Merchant Demo som en selvstændig statisk webapp; ingen ny Angular/.NET solution og ingen dependencies.
- Tilføj et lokalt Pizzeria Roma-katalog med stabile kategori-, produkt- og tilvalgs-id'er.
- Tilføj produktdialog, mængder, valgfrie tilvalg, særskilte kurvlinjer og summering.
- Send produktets stabile ID som `lineId`; læsbare tilvalg medtages i linjenavnet, og strukturerede produkt-/tilvalgs-id'er gemmes i `rawMerchantPayloadJson`.
- Bevar separate kurvlinjer ved forskellige tilvalg.
- Generér én stabil merchant draft-reference pr. browsersession og gruppeordre, så retry ikke opfinder en ny reference.
- PayNSync API er eneste integrationsgrænse. Demoen må ikke kalde Vipps/MobilePay eller en merchant-ordrekø.

## Affected files

- `src/Frontend.MerchantDemo/index.html`
- `src/Frontend.MerchantDemo/styles.css`
- `src/Frontend.MerchantDemo/order-model.js`
- `src/Frontend.MerchantDemo/app.js`
- `src/Frontend.MerchantDemo/app.test.js`
- `src/Frontend.MerchantDemo/package.json`
- `src/Frontend.MerchantDemo/package-lock.json`
- UC-20/current-state dokumentation efter verification.

## Risks

| Risk | Mitigation |
|---|---|
| Tilvalg mistes i den nuværende API-linjemodel | Medtag dem læsbart i linjenavnet og struktureret i `rawMerchantPayloadJson` |
| Dubletlinjer slås sammen | Hver konfiguration får sit eget line-instance-id og bevares separat |
| Forkert total sendes til betaling | Brug én delt, testet beregningsfunktion til kurv og payload |
| Reload/retry giver ny draft-reference | Gem reference i `sessionStorage` pr. order/token |
| Demo frigiver ordre for tidligt | Ingen order-hub/callback-kald; kun eksisterende draft-endpoint |

## Verification

1. Kør Node unit tests uden eksterne services.
2. Kør statisk syntax-check af browser-JavaScript.
3. Gennemgå request-payload, redirect og fejlhåndtering.
4. Gennemgå mobil/responsivt layout og accessibility-semantik.
5. Opdatér UC-20 og berørt current-state efter grøn verification.

## Out of scope

- CMS eller databasebaseret produktkatalog.
- Almindelig kortbetaling.
- PayNSync Order Hub UI og merchant-statusflow.
- Merchant-specifik adapter eller produktionsklart POS-format.
