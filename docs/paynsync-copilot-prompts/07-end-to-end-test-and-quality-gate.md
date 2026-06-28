# Prompt 07 – End-to-end test og quality gate

Overhold:

- `00-master-instructions.md`
- `architecture-updated.md`
- `business-rules-updated.md`
- `vipps-mobilepay-test-users-implementation-updated.md`
- eksisterende copilot-instructions

## Mål

Verificér hele PayNSync v1-flowet med først Fake provider og derefter Vipps sandbox, hvis konfigurationen er klar.

## Fake provider test

Kør eller verificér dette flow:

1. Opret gruppeordre.
2. Vælg Merchant Demo.
3. Tilføj mindst 2-3 deltagere.
4. Åbn Merchant Demo-link for hver deltager.
5. Indsend ordre via `Bekræft ordre og reservér betaling`.
6. Fake provider sætter reservation som `Reserved`.
7. Når alle er indsendt/reserved, bliver order `ReadyToPay`.
8. Host klikker `Godkend samlet ordre`.
9. Fake provider capturer hver participant payment.
10. Order bliver `Paid`.
11. Final `GroupOrderPaid` payload bygges.
12. Merchant/testvisning kan vise final payload.

## Vipps sandbox test

Hvis config er klar:

```json
{
  "Payments": {
    "Provider": "MobilePay",
    "VippsMobilePay": {
      "BaseUrl": "https://apitest.vipps.no",
      "ClientId": "...",
      "ClientSecret": "...",
      "SubscriptionKey": "...",
      "MerchantSerialNumber": "...",
      "CallbackBaseUrl": "https://<public-url>"
    }
  }
}
```

Test:

1. Sæt `Payments:Provider = MobilePay`.
2. Brug offentlig `CallbackBaseUrl`, fx ngrok.
3. Opret gruppeordre med Merchant Demo.
4. Deltager åbner Merchant Demo-link.
5. Deltager vælger varer.
6. Deltager klikker `Bekræft ordre og reservér betaling`.
7. Merchant Demo modtager `redirectUrl`.
8. Browser redirecter til Vipps/MobilePay approval flow.
9. Deltager swiper/godkender i MobilePay test app.
10. Webhook sætter payment til `Reserved`.
11. Gentag for alle deltagere.
12. Kontrollér `ReadyToPay`.
13. Host approve.
14. Kontrollér capture-loop.
15. Kontrollér `Paid`.
16. Kontrollér final `GroupOrderPaid` payload.

## Fejlscenarier

Verificér eller tilføj tests for:

### Deltager godkender ikke

Forventet:

```text
ParticipantPayment = ReservationStarted
Order forbliver Collecting
Host kan ikke approve
Merchant får ikke final order
```

### Reservation fejler

Forventet:

```text
ParticipantPayment = ReservationFailed
Order bliver ikke ReadyToPay
Deltager kan prøve igen
```

### Capture fejler

Forventet:

```text
Fejlet payment = CaptureFailed
Order = PartiallyFailed
Allerede Captured payments forbliver Captured
Merchant får ikke GroupOrderPaid
Host kan retry
```

### Merchant callback fejler

Forventet:

```text
Order kan være Paid
Callback-fejl logges
Der bør kunne laves retry senere
```

## Quality gate

Kontrollér:

- Ingen Vipps secrets i frontend.
- Merchant Demo kalder ikke Vipps direkte.
- `ReadyToPay` baseres på `Reserved`, ikke `OrderSubmitted`.
- Capture-loop bruger `ProviderPaymentId`.
- Capture-loop bruger ikke telefonnummer.
- Der laves ikke ét samlet Vipps-beløb.
- Final merchant callback sendes kun efter `Paid`.
- Tests passerer.
- Ændringerne følger eksisterende arkitektur.
- Copilot-instructions er overholdt.

## Output

Afslut med:

```text
Implemented flow summary
Changed files
Tests run
Tests passing/failing
Manual test steps
Remaining TODOs
Risks before production
```
