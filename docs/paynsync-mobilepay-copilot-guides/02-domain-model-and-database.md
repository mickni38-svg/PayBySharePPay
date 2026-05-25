# 02 — Domain Model og Database til Reservation/Capture Flow

## Mål
Udvid domænet og databasen, så hver deltager har sin egen betalingsreservation og capture-status.

## Vigtig forretningsregel
En afventende ordre har ikke nødvendigvis et endeligt betalt beløb. Beløbet bliver først reelt tilgængeligt, når deltagerens betaling er reserveret/godkendt og senere capturet.

## Payment states
Indfør enum eller tilsvarende:

```csharp
public enum ParticipantPaymentStatus
{
    Created = 0,
    ReservationStarted = 10,
    Reserved = 20,
    ReservationFailed = 30,
    CapturePending = 40,
    Captured = 50,
    CaptureFailed = 60,
    Cancelled = 70,
    Expired = 80,
    Refunded = 90
}
```

## Datamodel
Tilføj eller udvid tabeller/entities:

### ParticipantPayment
Skal indeholde:

- Id
- GroupPaymentId / OrderId
- ParticipantId / UserId
- MerchantId
- AmountMinorUnits
- Currency
- Status
- ProviderName
- ProviderPaymentId
- ProviderReference
- ReservationStartedAtUtc
- ReservedAtUtc
- CaptureStartedAtUtc
- CapturedAtUtc
- CancelledAtUtc
- LastErrorCode
- LastErrorMessage
- RowVersion/concurrency token hvis projektet bruger EF Core

### PaymentEventLog
Opret en append-only eventlog:

- Id
- GroupPaymentId / OrderId
- ParticipantPaymentId
- ProviderPaymentId
- EventType
- OldStatus
- NewStatus
- PayloadJson
- CorrelationId
- CreatedAtUtc

## State transition-regler
Tillad kun disse overgange:

```text
Created -> ReservationStarted
ReservationStarted -> Reserved
ReservationStarted -> ReservationFailed
ReservationStarted -> Cancelled
Reserved -> CapturePending
Reserved -> Cancelled
CapturePending -> Captured
CapturePending -> CaptureFailed
CaptureFailed -> CapturePending
Captured -> Refunded
```

Alle andre overgange skal afvises eller logges som invalid transition.

## Service
Opret en domain/application service, fx:

```csharp
public interface IParticipantPaymentStateService
{
    Task SetReservationStartedAsync(...);
    Task SetReservedAsync(...);
    Task SetReservationFailedAsync(...);
    Task SetCapturePendingAsync(...);
    Task SetCapturedAsync(...);
    Task SetCaptureFailedAsync(...);
    Task SetCancelledAsync(...);
}
```

Servicen skal:

- validere transition
- opdatere entity
- skrive `PaymentEventLog`
- være idempotent ved gentagne webhooks

## EF Core migration
Opret migration for nye felter/tabeller.

## Tests
Tilføj tests for:

- Created -> ReservationStarted -> Reserved
- Reserved -> CapturePending -> Captured
- CapturePending -> CaptureFailed -> CapturePending
- Dublet webhook med samme status ikke ødelægger state
- Invalid transition afvises

## Definition of Done
- Database kan gemme hele payment lifecycle pr. deltager.
- Alle statusændringer logges.
- State transitions er testet.
