# Betalingsflow: Fra deltager bestiller til pengene er trukket

**Dokument type:** Teknisk forklaring  
**Kilde:** Reverse-engineered fra kodebase  
**Sidst opdateret:** 2026-07  

---

## Kernepointe

Naar en deltager godkender en betaling i MobilePay/Vipps, er pengene **ikke trukket**.  
De er kun **reserveret** (blokeret paa kontoen).  
Pengene traekes foerst naar host aktivt godkender, og systemet looper igennem
alle deltageres reserverede betalinger og kalder `CaptureAsync()` een gang per deltager.

---

## Fase 1 — Deltager bestiller via MerchantDemo

**Kode:** `MerchantOrderService.InitOrderAsync()`

```
1. Gem bestilling i databasen (MerchantOrderDraft + MerchantOrderLines)
2. Saet OrderParticipant.Status = "OrderSubmitted"
3. Kald ReserveParticipantPaymentAsync(...)
   --> Vipps returnerer en RedirectUrl til MobilePay-popup
   --> ParticipantPayment.Status = "ReservationStarted"
4. Returner RedirectUrl til browseren
```

Deltager ser nu MobilePay-popuppen i sin telefon og godkender.  
Penge er **ikke trukket** — de er kun bedt om at blive reserveret.

---

## Fase 2 — MobilePay bekraefter reservationen (webhook)

**Kode:** `VippsCallbackController.VippsCallback()`

Naar deltager godkender i MobilePay-appen, sender Vipps/MobilePay
et HTTP POST til vores callback-URL med status `AUTHORIZED` eller `RESERVE`.

```csharp
case "AUTHORIZED":
case "RESERVE":
    // Saet betaling til Reserved -- pengene er nu blokeret paa deltagerens konto
    await _stateService.SetReservedAsync(payment.Id, correlationId, cancellationToken);

    // Tjek om ALLE deltagere nu er Reserved
    // -- hvis ja, saet Order.Status = "ReadyToPay" og send besked til host
    await _orderService.CheckAndSetReadyToPayByReservedAsync(payment.OrderId, cancellationToken);
    break;
```

`ParticipantPayment.Status` er nu `Reserved`.  
Pengene er **blokeret** paa kontoen, men **ikke trukket**.  
Ordren gaar til `ReadyToPay` foerst naar **alle** deltagere er `Reserved`.

---

## Fase 3 — Host godkender (capture starter)

**Kode:** `GroupPaymentOrchestrationService.ApproveAndCaptureAsync()`

Naar host trykker "Godkend" i appen, starter dette flow:

```
1. Find alle ParticipantPayments med Status = "Reserved"
2. Saet alle --> CapturePending  (forberedelse)
3. Saet Order.Status = "Capturing"
4. LOOP igennem alle reserverede betalinger:
      foreach (var payment in reservedPayments)
      {
          captureResult = await paymentProvider.CaptureAsync(captureRequest)
          // ^^ HER TRAEKES PENGENE FAKTISK fra deltagerens konto

          if (captureResult.Success)
              saet payment.Status = "Captured"
          else
          {
              saet payment.Status = "CaptureFailed"
              saet Order.Status   = "PartiallyFailed"
              STOP loopet  // resterende betalinger captures ikke
          }
      }
5. Hvis alle captured: saet Order.Status = "Paid"
```

---

## Status-maskinen som bevis

```
DELTAGER BESTILLER
      |
      v
ParticipantPayment: Created
      |
      v  (ReserveAsync kaldt -- MobilePay-popup vises)
ParticipantPayment: ReservationStarted
      |
      v  (Deltager godkender i MobilePay-appen -- webhook modtages)
ParticipantPayment: Reserved          <-- Penge BLOKERET, IKKE trukket
      |
      v  (Host godkender i PayNSync)
ParticipantPayment: CapturePending
      |
      v  (CaptureAsync() kaldes for DENNE deltager i loopet)
ParticipantPayment: Captured          <-- Penge TRUKKET
      |
      v  (Naar alle deltagere er Captured)
Order.Status: Paid                    <-- Ordre fuldt betalt
```

---

## Hvad sker der hvis en capture fejler?

Loopet stopper ved foerste fejl. Ordre-status saettes til `PartiallyFailed`.  
Allerede-captured betalinger kan ikke rulles tilbage automatisk.  
Ikke-captured betalinger forbliver `Reserved` (penge stadig blokeret).

```
Deltager A: Captured       <-- Pengene er trukket
Deltager B: CaptureFailed  <-- Pengene er IKKE trukket (stadig blokeret)
Deltager C: Reserved       <-- Loop stopper; naaede ikke hertil
Order.Status: PartiallyFailed
```

---

## Hvad MobilePay sender (og ikke sender)

| Vipps webhook-event        | Hvad det betyder                        | Haandtering i koden                        |
|----------------------------|-----------------------------------------|--------------------------------------------|
| `AUTHORIZED` / `RESERVE`   | Deltager godkendte i appen; penge blokeret | SetReservedAsync + CheckReadyToPay      |
| `CAPTURED`                 | Vipps bekraefter vores capture          | **Ignoreres** — capture styres af vores eget loop |
| `CANCELLED` / `ABORTED`    | Deltager afviste eller annullerede      | SetCancelledAsync                          |
| `EXPIRED` / `TERMINATED`   | Reservation udloeb                      | SetReservationFailedAsync                  |

> **Bemærk:** `CAPTURED`-eventet fra Vipps ignoreres bevidst.  
> Det er vores eget `ApproveAndCaptureAsync`-loop der styrer og bekraefter om capture lykkedes.

---

## Filer i kodebasen

| Fil | Rolle i flowet |
|-----|----------------|
| `Service.PayBySharePay/Services/MerchantOrderService.cs` | Fase 1: gem bestilling, start reservation |
| `Api.PayBySharePay/Controllers/VippsCallbackController.cs` | Fase 2: modtag webhook, saet Reserved |
| `Service.PayBySharePay/Services/OrderService.cs` | Fase 2: CheckAndSetReadyToPayByReservedAsync |
| `Service.PayBySharePay/Services/GroupPaymentOrchestrationService.cs` | Fase 3: capture-loop, saet Paid |
| `Service.PayBySharePay/Services/ParticipantPaymentStateService.cs` | Alle faser: state transitions + audit log |

---

## Relaterede dokumenter

- `docs/usecases/UC-08-bestil-via-merchant-link.md`
- `docs/usecases/UC-09-reserver-betaling.md`
- `docs/usecases/UC-10-godkend-og-capture.md`
- `docs/usecases/UC-13-payment-webhook.md`
- `docs/bugs/BUG-01-readytopay-besked-for-tidlig.md`
