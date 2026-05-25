# 05 — Host Execute Payment / Capture Flow

## Mål
Implementér hostens `Execute payment` flow, hvor alle reserverede deltagerbetalinger capture's én ad gangen.

## Forretningsregel
Host må kun starte execute/capture, når betalingerne er klar. Som minimum skal de deltagere, der skal betale, have status `Reserved`.

## Endpoint
Opret eller tilpas endpoint:

```http
POST /api/group-payments/{groupPaymentId}/execute-payment
```

eller brug eksisterende order endpoint hvis projektet allerede har det.

## Flow
Når host klikker `Execute payment`:

1. Hent gruppebetalingen/order.
2. Kontrollér at brugeren er host/ejer.
3. Find alle deltagerbetalinger med status `Reserved`.
4. Hvis ingen er reserved, returnér valideringsfejl.
5. Sæt hver betaling til `CapturePending` lige før capture-kald.
6. Kald `IPaymentProvider.CaptureAsync` én betaling ad gangen.
7. Ved success: sæt status `Captured`.
8. Ved fejl: sæt status `CaptureFailed`, log fejl og stop eller fortsæt efter konfigureret strategi.

## Capture strategy
Start med konservativ strategi:

```text
StopOnFirstFailure = true
```

Hvis én capture fejler:

- stop resten af captures
- vis fejl i dashboard/UI
- lad host retry'e fejlede betalinger
- allerede capturede betalinger skal ikke capture's igen

## Retry endpoint
Tilføj endpoint:

```http
POST /api/group-payments/{groupPaymentId}/participant-payments/{participantPaymentId}/retry-capture
```

Retry må kun være tilladt for status `CaptureFailed`.

## Cancel remaining endpoint
Tilføj endpoint:

```http
POST /api/group-payments/{groupPaymentId}/cancel-remaining-reservations
```

Denne skal cancel'e alle betalinger i status `Reserved`, hvis host ønsker at afbryde ordren.

## Idempotency
Capture skal bruge stabil idempotency key, fx:

```text
capture:{groupPaymentId}:{participantPaymentId}:{captureAttemptNumber}
```

Undgå at capture samme betaling to gange.

## UI/API response
Execute endpoint skal returnere samlet resultat:

```json
{
  "groupPaymentId": "...",
  "status": "PartiallyCaptured|Captured|Failed",
  "capturedCount": 2,
  "failedCount": 1,
  "pendingCount": 1,
  "items": []
}
```

## Tests
Tilføj tests for:

- execute afviser hvis ingen deltagere er reserved
- execute capture'r alle reserved betalinger
- execute stopper ved første capture-fejl
- retry virker kun for CaptureFailed
- allerede Captured betaling capture's ikke igen
- cancel remaining cancel'er kun Reserved betalinger

## Definition of Done
- Host kan starte capture-flow.
- Capture sker én deltager ad gangen.
- Fejl kan ses og retry'es.
- Allerede captured betalinger capture's ikke igen.
