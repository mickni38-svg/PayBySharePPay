# PayNSync Copilot Claude – Master Instructions

Du skal arbejde i PayNSync-projektet og overholde:

- `project-overview-updated.md`
- `architecture-updated.md`
- `business-rules-updated.md`
- `vipps-mobilepay-test-users-implementation-updated.md`
- eksisterende `copilot-instructions` / instruction-filer i repoet

## Overordnet mål

Implementér PayNSync v1-flowet:

```text
Merchant Demo sender deltagerens draft-ordre til PayNSync.
PayNSync backend opretter Vipps/MobilePay reservation.
Merchant Demo redirecter deltageren til Vipps/MobilePay approval flow.
Deltager swiper/godkender i MobilePay/Vipps.
PayNSync modtager webhook og sætter betaling til Reserved.
Når alle er Reserved, bliver ordren ReadyToPay.
Host godkender.
PayNSync capturer alle reservationer én efter én.
Når alle er Captured, sender PayNSync én GroupOrderPaid payload til merchant.
```

## Absolutte regler

- Merchant Demo må ikke kalde Vipps/MobilePay API direkte.
- Merchant Demo må ikke kende `client_id`, `client_secret`, subscription key eller access token.
- PayNSync backend opretter Vipps/MobilePay payment.
- Selve swipet sker i MobilePay/Vipps app/test flow.
- Capture-loopet må ikke bruge telefonnummer.
- Capture-loopet skal bruge `ProviderPaymentId` / Vipps reference.
- Der må ikke laves ét samlet Vipps-beløb for hele gruppen.
- Der skal være én `ParticipantPayment` pr. deltager.
- `ReadyToPay` må ikke baseres på `OrderSubmitted` alene.
- `ReadyToPay` må kun sættes når alle relevante `ParticipantPayment` records er `Reserved`.
- Merchant må først modtage final `GroupOrderPaid` payload efter alle betalinger er `Captured`.

## Arbejdsform

Arbejd i små trin.

Før hver implementeringsprompt:

1. Læs relevante eksisterende filer.
2. Forklar kort hvad du vil ændre.
3. Lav minimale ændringer.
4. Bevar eksisterende arkitektur.
5. Tilføj/opdater tests hvor det giver mening.
6. Afslut med en kort liste over ændrede filer og teststatus.
