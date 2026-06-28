# Prompt 05 – ReadyToPay og host capture-loop

Overhold:

- `00-master-instructions.md`
- `architecture-updated.md`
- `business-rules-updated.md`
- `vipps-mobilepay-test-users-implementation-updated.md`
- eksisterende copilot-instructions

## Mål

Ret PayNSyncs ordrestatus og capture-loop, så:

1. `ReadyToPay` kun sættes når alle relevante participant payments er `Reserved`.
2. Host approve capturer hver eksisterende reservation én efter én.
3. Capture sker via `ProviderPaymentId` / Vipps reference.
4. Capture-loopet bruger ikke telefonnummer.
5. Capture-loopet opretter ikke nye betalinger.
6. Capture-loopet laver ikke ét samlet beløb.

## ReadyToPay-regel

Find eksisterende `CheckAndSetReadyToPayAsync()` eller tilsvarende.

Ny regel:

```csharp
bool allReady = orderParticipants
    .Where(p => p.Participant.Type != ParticipantType.Merchant)
    .All(p => participantPayments.Any(pp =>
        pp.OrderId == order.Id &&
        pp.ParticipantId == p.ParticipantId &&
        pp.Status == ParticipantPaymentStatus.Reserved));
```

Tilpas til projektets faktiske enums/strings.

Hvis alle er ready:

```text
Order.Status = ReadyToPay
Send host-besked:
"Alle har bestilt og reserveret betaling. Du kan nu godkende den samlede ordre."
```

Hvis alle har `OrderSubmitted`, men ikke alle har `Reserved`:

```text
Order forbliver Collecting eller afventende reservation.
```

## Host approve / capture-loop

Find:

```text
POST /api/orders/{id}/approve
GroupPaymentOrchestrationService.ApproveAndCaptureAllAsync()
```

Flow:

1. Kontrollér requester er host.
2. Kontrollér at order status er `ReadyToPay`, `HostApproved`, `Capturing` eller `PartiallyFailed`.
3. Hent alle `ParticipantPayment` for ordren.
4. Vælg betalinger med status `Reserved` eller retrybare `CaptureFailed`.
5. Sæt hver til `CapturePending`.
6. Loop betalingerne.
7. For hver betaling:
   - Brug `ProviderPaymentId`
   - Brug `AmountMinorUnits`
   - Brug `Currency`
   - Kald `IPaymentProvider.CaptureAsync()`
8. Ved success:
   - sæt `Captured`
   - gem provider transaction id hvis muligt
9. Ved fejl:
   - sæt `CaptureFailed`
   - sæt ordre `PartiallyFailed`
   - stop loop eller returnér delvis fejl efter eksisterende strategi
10. Når alle betalinger er `Captured`:
   - sæt ordre `Paid`
   - send final group order til merchant

## Vigtige regler

- Brug ikke telefonnummer i capture.
- Opret ikke nye betalinger i approve-flowet.
- Lav ikke ét samlet Vipps/MobilePay-beløb.
- Én deltagerbetaling = én capture.
- Allerede `Captured` betalinger skal springes over ved retry.
- Merchant callback må ikke sendes ved partial failure.

## Tests

Tilføj/opdater tests for:

- Alle `OrderSubmitted`, men ikke alle `Reserved` => ikke `ReadyToPay`.
- Alle participant payments `Reserved` => `ReadyToPay`.
- Host approve capturer hver `Reserved` payment via `ProviderPaymentId`.
- Capture-loop bruger ikke telefonnummer.
- Alle `Captured` => `Order.Status = Paid`.
- Capture failure => `Order.Status = PartiallyFailed`.
- Merchant callback sendes ikke ved partial failure.

## Output

Giv kort opsummering:

```text
Changed files
ReadyToPay behavior
Capture-loop behavior
Tests added/updated
```
