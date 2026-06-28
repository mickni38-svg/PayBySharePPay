# Prompt 02 – Merchant Demo starter PayNSync reservation-flow

Overhold:

- `00-master-instructions.md`
- `architecture-updated.md`
- `business-rules-updated.md`
- `vipps-mobilepay-test-users-implementation-updated.md`
- eksisterende copilot-instructions

## Mål

Opdater `Frontend.MerchantDemo`, så demoen kan starte PayNSync betalingsreservation-flowet.

Merchant Demo skal ikke kalde Vipps/MobilePay direkte. Den skal kun sende deltagerens draft-ordre til PayNSync API og redirecte til den `redirectUrl`, som PayNSync returnerer.

## Krav

Find Merchant Demo HTML/JS.

Knappen skal hedde:

```text
Bekræft ordre og reservér betaling
```

Vis forklaring tæt på knappen:

```text
Du bliver sendt til MobilePay for at godkende en reservation.
Beløbet trækkes først, når alle i gruppen har bestilt, og værten godkender den samlede ordre.
```

Når brugeren klikker:

1. Læs `orderId` fra querystring.
2. Læs `participantToken` fra querystring.
3. Saml ordrelinjer fra kurven.
4. Send `POST /api/merchant-orders` til PayNSync API.
5. Medtag eventuelt `testPhoneNumber` til Vipps sandbox test.
6. Hvis response indeholder `redirectUrl`, redirect browseren:

```javascript
window.location.href = result.redirectUrl;
```

7. Hvis der ikke er `redirectUrl`, vis statusbesked, fx ved Fake provider.

## Eksempel request

```json
{
  "orderId": 123,
  "participantToken": "abc123",
  "merchantDraftId": "demo-draft-123-7",
  "currency": "DKK",
  "subtotalAmount": 168.00,
  "totalAmount": 168.00,
  "testPhoneNumber": "63550321",
  "lines": [
    {
      "sku": "burger-01",
      "name": "Burger",
      "quantity": 1,
      "unitPrice": 139.00,
      "lineTotal": 139.00
    }
  ]
}
```

## Vigtige regler

- Ingen Vipps secrets i frontend.
- Ingen direkte Vipps API-kald fra Merchant Demo.
- `testPhoneNumber` må kun bruges til at starte test-flowet, ikke til capture.
- Merchant Demo må ikke sende final order til merchant.

## Output

Implementér ændringen og giv kort opsummering:

```text
Changed files
How to test Merchant Demo flow
Known limitations
```
