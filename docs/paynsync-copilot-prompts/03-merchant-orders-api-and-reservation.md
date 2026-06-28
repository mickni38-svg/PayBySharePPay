# Prompt 03 – MerchantOrders API gemmer draft og starter reservation

Overhold:

- `00-master-instructions.md`
- `architecture-updated.md`
- `business-rules-updated.md`
- `vipps-mobilepay-test-users-implementation-updated.md`
- eksisterende copilot-instructions

## Mål

Opdater backend-flowet for `POST /api/merchant-orders`, så det:

1. Gemmer deltagerens draft-ordre.
2. Starter Vipps/MobilePay reservation via PayNSync backend.
3. Returnerer betalingsinformation inkl. `redirectUrl`.

## Request DTO

Sørg for at request understøtter:

```text
orderId
participantToken
merchantDraftId
currency
subtotalAmount
totalAmount
testPhoneNumber
lines[]
rawMerchantPayloadJson, hvis relevant
```

## Response DTO

Sørg for at response kan returnere:

```text
status
orderId
participantPaymentId
providerPaymentId
redirectUrl
message
```

Eksempel:

```json
{
  "status": "ReservationStarted",
  "orderId": 123,
  "participantPaymentId": 456,
  "providerPaymentId": "PNS-123-7-456",
  "redirectUrl": "https://...",
  "message": "Ordren er gemt. Godkend reservationen i MobilePay."
}
```

## MerchantOrderService-flow

I `MerchantOrderService.InitOrderAsync()` skal flowet være:

1. Valider `ParticipantToken`.
2. Find `Order`.
3. Find `OrderParticipant`.
4. Afvis hvis participant er merchant.
5. Gem `MerchantOrderDraft`.
6. Gem `MerchantOrderLine`.
7. Gem evt. `RawMerchantPayloadJson`.
8. Sæt `OrderParticipant.Status = OrderSubmitted`.
9. Beregn `AmountMinorUnits = TotalAmount * 100`.
10. Start reservation via eksisterende orchestration/payment service.
11. Returnér betalingsinformation inkl. `redirectUrl`.

## Meget vigtigt

- `OrderSubmitted` betyder kun at ordrelinjer er gemt.
- `OrderSubmitted` må ikke sætte hele ordren til `ReadyToPay`.
- `MerchantOrderService` må ikke sende final order til merchant.
- `MerchantOrderService` må ikke capture.
- Merchant opretter ikke MobilePay/Vipps-betalingen i v1.

## Idempotens

Hvis deltager indsender ny draft:

```text
Hvis betaling ikke er Reserved eller Captured:
    tillad erstatning af draft efter eksisterende regler og undgå dubletbetalinger.
Hvis betaling allerede er Captured:
    afvis ændring.
Hvis betaling allerede er Reserved:
    undgå dubletbetaling. Returnér eksisterende payment info eller kræv cancel/retry-flow.
```

## Tests

Tilføj/opdater tests for:

- Draft gemmes.
- Order lines gemmes.
- `OrderParticipant.Status = OrderSubmitted`.
- Reservation startes.
- Response indeholder `redirectUrl` når provider returnerer det.
- `ReadyToPay` sættes ikke kun pga. `OrderSubmitted`.

## Output

Giv kort opsummering:

```text
Changed files
Important behavior changes
Tests added/updated
How to test
```
