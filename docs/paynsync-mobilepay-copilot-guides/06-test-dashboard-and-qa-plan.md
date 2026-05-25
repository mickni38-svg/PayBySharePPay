# 06 — Test Dashboard og QA-plan for 3-4 testere

## Mål
Lav et simpelt test-dashboard og en QA-plan, så 3-4 testere kan køre hele PayNSync-flowet igennem i MobilePay/Vipps sandbox i flere måneder.

## Test dashboard
Lav en intern testside eller API endpoint, fx:

```http
GET /api/test-dashboard/group-payments/{groupPaymentId}
```

Dashboardet skal vise:

- GroupPayment/Order id
- Merchant navn
- Host
- Antal deltagere
- Samlet expected amount
- Antal Created
- Antal ReservationStarted
- Antal Reserved
- Antal CapturePending
- Antal Captured
- Antal Failed
- Antal Cancelled

Pr. deltager:

- ParticipantPaymentId
- Participant name/email/test-user-id
- AmountMinorUnits
- Currency
- Current status
- ProviderPaymentId
- Last error code/message
- Reservation timestamp
- Capture timestamp
- Seneste 10 PaymentEventLog entries

## Test actions
Tilføj test actions hvis projektets UI understøtter det:

- Start reservation link igen
- Refresh provider status
- Retry capture
- Cancel reservation
- Simulate webhook, kun i Development/Test miljø

## Miljøbeskyttelse
Test actions må kun være aktive når:

```csharp
IHostEnvironment.IsDevelopment() || environment == "Test"
```

De må ikke være aktive i Production.

## QA testscenarier
Opret dokumentation/checkliste i repoet:

### Scenario 1 — Happy path
1. Host opretter gruppebetaling.
2. 3 deltagere åbner hver deres betalingslink.
3. Alle reserverer betaling i MobilePay/Vipps test app.
4. Dashboard viser alle som `Reserved`.
5. Host klikker `Execute payment`.
6. Alle ender i `Captured`.

### Scenario 2 — Én deltager mangler
1. Host opretter gruppebetaling med 3 deltagere.
2. Kun 2 deltagere reserverer.
3. Host forsøger execute.
4. Systemet skal tydeligt vise, hvem der mangler.

### Scenario 3 — Capture failure
1. Simuler eller mock capture-fejl på én deltager.
2. Execute stopper ved fejl.
3. Allerede captured betalinger bevares.
4. Fejlet betaling kan retry'es.

### Scenario 4 — Cancel remaining
1. Nogle deltagere har reserveret.
2. Host afbryder ordren.
3. Alle reservationer i status `Reserved` cancel'es.
4. Dashboard viser `Cancelled`.

### Scenario 5 — Duplicate webhook
1. Send samme webhook to gange.
2. State må ikke blive forkert.
3. Event må gerne logges to gange eller deduplikeres, men domænestatus skal være korrekt.

### Scenario 6 — Out-of-order webhook
1. Send `Captured` webhook.
2. Send bagefter en gammel `Reserved` webhook.
3. Status må ikke gå tilbage fra `Captured` til `Reserved`.

## Loggingkrav
Sørg for structured logs med:

- GroupPaymentId
- ParticipantPaymentId
- ProviderPaymentId
- CorrelationId
- EventType
- OldStatus
- NewStatus
- ErrorCode

## Definition of Done
- Testere kan se state uden at kigge i databasen.
- Host kan se hvem der mangler.
- Fejl kan diagnosticeres fra dashboard/logs.
- QA-scenarier er dokumenteret i repoet.
