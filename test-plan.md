# Test plan — UC-20

## Automated tests

| Scenario | Expected result |
|---|---|
| Katalog-id'er | Kategori-, produkt- og tilvalgs-id'er er udfyldte og unikke |
| Produkt uden tilvalg | Linjetotal er grundpris × antal |
| Produkt med tilvalg | Tilvalgspris indgår pr. produkt og multipliceres med antal |
| Forskellige tilvalg på samme produkt | To separate kurvlinjer bevares |
| Ændring af mængde | Kurvtotal og payload-total opdateres ens |
| Draft-payload | Indeholder stabile line-id'er, total, currency og struktureret raw payload |
| Tom kurv | Checkout kan ikke oprette en draft-payload |

## Static verification

- `node --test app.test.js`
- `node --check app.js`
- `node --check order-model.js`
- Ingen nye dependencies eller eksterne kald i tests.

## Manual verification

1. Åbn demoen med `orderId`, `merchantId` og `participantToken` i querystring.
2. Tilføj samme pizza med to forskellige tilvalg og kontrollér separate kurvlinjer.
3. Ændr mængder og kontrollér totalen.
4. Klik PayNSync-checkout og verificér ét `POST /api/merchant-orders`.
5. Verificér redirect ved `paymentRedirectUrl` og kvittering uden redirect.
6. Verificér at ugyldige/manglende query-parametre deaktiverer checkout med en forståelig besked.

## Regression boundaries

- Ingen direkte Vipps/MobilePay-kald.
- Ingen callback eller Order Hub-kald.
- Ingen secrets, participant-token eller provider-reference vises i brugerfladen.
- Eksisterende API-feltnavne bevares.
