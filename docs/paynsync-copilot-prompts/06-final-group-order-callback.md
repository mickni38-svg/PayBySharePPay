# Prompt 06 – Final GroupOrderPaid payload til merchant

Overhold:

- `00-master-instructions.md`
- `architecture-updated.md`
- `business-rules-updated.md`
- `vipps-mobilepay-test-users-implementation-updated.md`
- eksisterende copilot-instructions

## Mål

Når alle betalinger er `Captured`, skal PayNSync bygge og sende én samlet final group order payload til merchant.

Merchant skal først modtage den endelige ordre efter:

```text
Alle deltagere har Reserved
Host har godkendt
Alle betalinger er Captured
Order.Status = Paid
```

## V1-princip

PayNSync definerer standard Group Order Contract.

Merchant skal mappe PayNSyncs JSON til sit eget system.

Der skal ikke laves merchant-specific adapters i v1.

## DTOs

Opret/brug gerne:

```text
PayNSyncFinalGroupOrderDto
PayNSyncFinalParticipantOrderDto
PayNSyncFinalOrderLineDto
```

Payload eksempel:

```json
{
  "eventType": "GroupOrderPaid",
  "paynsyncOrderId": 123,
  "merchantId": 45,
  "status": "Paid",
  "currency": "DKK",
  "totalAmount": 481.00,
  "paidAtUtc": "2026-06-28T12:45:00Z",
  "participants": [
    {
      "participantId": 7,
      "displayName": "Michael",
      "amount": 168.00,
      "paymentStatus": "Captured",
      "providerPaymentId": "PNS-123-7-456",
      "merchantDraftId": "demo-draft-123-7",
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
  ]
}
```

## Sender

Opret/brug gerne:

```text
IMerchantOrderSender
GenericMerchantWebhookSender
```

`GenericMerchantWebhookSender` skal sende PayNSyncs standard JSON til `Merchant.GroupOrderUrl`.

Hvis eksisterende `MerchantCallbackService` allerede gør noget tilsvarende, så udvid den i stedet for at opfinde en parallel arkitektur.

## Regler

- Callback må kun sendes efter `Order.Status = Paid`.
- Callback må kun sendes når alle relevante participant payments er `Captured`.
- Callback må ikke sendes ved `OrderSubmitted`.
- Callback må ikke sendes ved `ReservationStarted`.
- Callback må ikke sendes ved individuel `Reserved`.
- Callback må ikke sendes ved `PartiallyFailed`.
- Hvis callback fejler, skal fejlen logges tydeligt.
- Callback-fejl må ikke rulle betalingerne tilbage.
- Der bør senere kunne laves retry af callback.

## Merchant Demo testvisning

Hvis `Frontend.MerchantDemo` er statisk HTML/JS og ikke kan modtage server-side POST direkte, vælg en simpel teststrategi:

Anbefalet:

```text
PayNSync API gemmer seneste final merchant payload i dev/test.
Merchant Demo eller PayNSync order detail kan vise payloaden via GET endpoint.
```

Muligt endpoint:

```http
GET /api/dev/merchant-callbacks/latest?orderId=123
```

eller vis payload i PayNSync frontend order detail i dev/test.

## Tests

Tilføj/opdater tests for:

- Payload indeholder `eventType = GroupOrderPaid`.
- Payload indeholder participants med lines.
- Payload indeholder paymentStatus `Captured`.
- Payload sendes først efter alle payments er `Captured`.
- Payload sendes ikke ved partial failure.
- Callback-fejl logges uden at ændre `Paid` status tilbage.

## Output

Giv kort opsummering:

```text
Changed files
Final payload shape
Callback behavior
Tests added/updated
How to verify in demo
```
