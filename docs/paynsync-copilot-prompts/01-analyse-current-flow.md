# Prompt 01 – Analyse af eksisterende PayNSync-flow

Læs først:

- `project-overview-updated.md`
- `architecture-updated.md`
- `business-rules-updated.md`
- `vipps-mobilepay-test-users-implementation-updated.md`
- eksisterende `copilot-instructions`

## Opgave

Analyser eksisterende kode uden at ændre noget endnu.

Find og dokumentér kort:

1. Hvor `POST /api/merchant-orders` håndteres.
2. Hvordan `MerchantOrderService.InitOrderAsync()` virker i dag.
3. Hvordan merchant draft og order lines gemmes.
4. Hvor `ParticipantPayment` oprettes.
5. Hvor reservation starter.
6. Hvordan `IPaymentProvider.ReserveAsync()` bruges.
7. Hvordan `MobilePaySandboxPaymentProvider` virker.
8. Hvordan Vipps webhook håndteres.
9. Hvordan `ReadyToPay` sættes i dag.
10. Hvordan `/api/orders/{id}/approve` capturer betalinger i dag.
11. Hvordan merchant callback sendes i dag.
12. Hvilke tests der allerede findes for disse flows.

## Output

Lav en kort analyse med:

```text
Current flow
Gaps compared to updated PayNSync v1 rules
Files that likely need changes
Suggested implementation order
Risks
```

Du må ikke ændre kode i denne prompt.
