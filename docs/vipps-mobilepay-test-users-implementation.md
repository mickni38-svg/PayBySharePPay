# Implementering: Vipps MobilePay testbrugere + Merchant Demo bestilling + PayNSync capture-loop

## Formål

Implementér et komplet testflow hvor flere Vipps MobilePay testbrugere kan bestille individuelt via `Frontend.MerchantDemo`, godkende deres egen betalingsreservation i MobilePay test app, og hvor PayNSync efterfølgende kan capture alle deltagernes reserverede betalinger én efter én, når host godkender den samlede gruppeordre.

Dette dokument beskriver hvad der skal implementeres, ændres og testes.

---

## Besluttet PayNSync v1-flow


## Seneste præcisering: Merchant Demo starter PayNSync payment-flow

Merchant Demo har ikke selv MobilePay/Vipps implementeret, og det skal den heller ikke have direkte.

Den skal implementere følgende:

```text
Merchant Demo knap
  → POST /api/merchant-orders til PayNSync
  → modtag redirectUrl
  → window.location.href = redirectUrl
```

PayNSync backend skal implementere følgende:

```text
POST /api/merchant-orders
  → valider participantToken
  → gem MerchantOrderDraft + MerchantOrderLine
  → opret/genbrug ParticipantPayment
  → kald Vipps/MobilePay ePayment create payment
  → gem ProviderPaymentId/reference
  → returnér redirectUrl til Merchant Demo
```

Deltagerens swipe sker herefter i MobilePay/Vipps app/test flow. PayNSync overtager først igen, når Vipps sender webhook.

```text
Vipps webhook AUTHORIZED/RESERVED
  → find ParticipantPayment via reference/ProviderPaymentId
  → status = Reserved
  → hvis alle deltagere er Reserved: Order.Status = ReadyToPay
```

Vigtigt:

- Merchant Demo må ikke kalde Vipps direkte.
- Merchant Demo må ikke have `client_id`, `client_secret`, subscription key eller access token.
- PayNSync backend er eneste sted hvor Vipps API kaldes.
- `testPhoneNumber` kan sendes med til PayNSync for sandbox-test, men må ikke bruges i capture-loopet.
- Capture-loopet bruger kun `ProviderPaymentId` / Vipps reference.


PayNSync v1 bruger denne model:

```text
Merchant = menu, kurv og ordrelinjer
PayNSync = gruppeflow, reservation, status, capture og final merchant callback
Vipps/MobilePay = brugerens betalingsgodkendelse
```

Vigtigt:

- Merchant sender kun deltagerens draft-ordre til PayNSync.
- PayNSync opretter Vipps/MobilePay reservation pr. deltager.
- Deltageren skal selv godkende reservationen i MobilePay test app.
- MobilePay swipe/godkendelse betyder kun `Reserved`.
- Merchant må ikke modtage/frigive den endelige ordre ved individuel swipe.
- Når alle deltagere er `Reserved`, bliver ordren `ReadyToPay`.
- Host klikker `Godkend samlet ordre`.
- PayNSync capturer hver deltagerbetaling én efter én via `ProviderPaymentId` / Vipps reference.
- Først når alle betalinger er `Captured`, sender PayNSync én samlet `GroupOrderPaid` JSON til merchant.

---

## Ikke-mål

Implementeringen skal ikke:

- lave ét samlet Vipps/MobilePay-beløb for hele gruppen
- trække penge ud fra telefonnummer i capture-loopet
- sende ordren til merchant efter første deltager har godkendt MobilePay
- lave merchant-specifikke payload adapters i v1
- forsøge at matche alle merchants' interne ordreformater

---

## Hovedscenarie

### Aktører

- Host: opretter gruppeordre i PayNSync
- Deltager 1: Vipps/MobilePay testbruger
- Deltager 2: Vipps/MobilePay testbruger
- Deltager 3: Vipps/MobilePay testbruger
- Merchant Demo: simuleret restaurant-side
- PayNSync API
- Vipps MobilePay sandbox

### Flow

```text
1. Host opretter gruppeordre i PayNSync
2. Host vælger merchant demo og deltagere
3. PayNSync opretter personligt merchant-link pr. deltager
4. Deltager åbner merchant demo-link
5. Deltager vælger menuvarer
6. Deltager klikker "Bekræft ordre og reservér betaling"
7. Merchant Demo sender draft-ordre til PayNSync
8. PayNSync gemmer draft + ordrelinjer
9. PayNSync opretter Vipps/MobilePay payment/reservation
10. Deltager redirectes til Vipps/MobilePay test approval flow
11. Deltager godkender i MobilePay test app
12. Vipps webhook opdaterer ParticipantPayment til Reserved
13. Når alle deltagere er Reserved, sættes Order.Status = ReadyToPay
14. Host klikker "Godkend samlet ordre"
15. PayNSync looper igennem alle Reserved ParticipantPayments
16. PayNSync kalder Vipps capture for hver betaling/reference
17. Hver betaling sættes Captured ved success
18. Når alle er Captured, sættes Order.Status = Paid
19. PayNSync sender én samlet GroupOrderPaid JSON til merchant demo
```

---

## Vigtig regel om telefonnummer

Capture-loopet må ikke bruge telefonnummer eller MobilePay-id til at trække penge.

Når deltageren godkender i MobilePay test app, knyttes betalingen hos Vipps/MobilePay til den rigtige bruger. PayNSync skal derefter bruge den eksisterende payment reference ved capture.

PayNSync skal gemme og bruge:

```text
ParticipantPayment.ProviderPaymentId / Reference
ParticipantPayment.AmountMinorUnits
ParticipantPayment.Currency
ParticipantPayment.Status
```

Ikke:

```text
PhoneNumber + amount som senere trækkes
```

Telefonnummer kan kun være relevant ved oprettelse eller testidentifikation, men ikke som capture-nøgle.

---

## Data- og statusmodel

### ParticipantPayment

Kontrollér at `ParticipantPayment` mindst understøtter:

```csharp
public int Id { get; set; }
public int OrderId { get; set; }
public int ParticipantId { get; set; }
public long AmountMinorUnits { get; set; }
public string Currency { get; set; } = "DKK";
public string? ProviderPaymentId { get; set; }
public string Status { get; set; }
public DateTime? ReservationStartedAtUtc { get; set; }
public DateTime? ReservedAtUtc { get; set; }
public DateTime? CaptureStartedAtUtc { get; set; }
public DateTime? CapturedAtUtc { get; set; }
public byte[] RowVersion { get; set; }
```

### MerchantOrderDraft

Kontrollér eller udvid `MerchantOrderDraft` så den kan gemme:

```csharp
public int Id { get; set; }
public int OrderId { get; set; }
public int ParticipantId { get; set; }
public string? MerchantDraftId { get; set; }
public decimal SubtotalAmount { get; set; }
public decimal TotalAmount { get; set; }
public string Currency { get; set; } = "DKK";
public string Status { get; set; }
public string? RawMerchantPayloadJson { get; set; }
public List<MerchantOrderLine> Lines { get; set; }
```

`RawMerchantPayloadJson` er vigtig, fordi merchantens originale draft kan indeholde felter, som PayNSync ikke normaliserer endnu.

---

## Statusregler

### Deltagerstatus

```text
Invited
  -> OrderSubmitted
```

`OrderSubmitted` betyder kun:

```text
Deltagerens ordrelinjer er gemt hos PayNSync.
```

Det betyder ikke at betaling er klar.

### Betalingsstatus

```text
Created
  -> ReservationStarted
  -> Reserved
  -> CapturePending
  -> Captured
```

### Ordrestatus

```text
Collecting
  -> ReadyToPay
  -> HostApproved
  -> Capturing
  -> Paid
```

`ReadyToPay` må først sættes når alle relevante deltagere har en `ParticipantPayment` med status `Reserved`.

Den må ikke sættes kun fordi alle deltagere har `OrderSubmitted`.

---

## Ændring 1: Merchant Demo skal kunne starte reservation

### Frontend.MerchantDemo

Opdater merchant demo-siden så knappen ikke hedder `Betal`.

Brug fx:

```text
Bekræft ordre og reservér betaling
```

Under knappen vises forklaring:

```text
Du bliver sendt til MobilePay for at godkende en reservation.
Beløbet trækkes først, når alle i gruppen har bestilt, og værten godkender den samlede ordre.
```

### Request til PayNSync

Når deltageren klikker knappen, skal merchant demo sende draft-ordre til PayNSync:

```http
POST /api/merchant-orders
```

Eksempel:

```json
{
  "orderId": 123,
  "participantToken": "abc123",
  "merchantDraftId": "demo-draft-123-7",
  "currency": "DKK",
  "subtotalAmount": 168.00,
  "totalAmount": 168.00,
  "lines": [
    {
      "sku": "burger-01",
      "name": "Burger",
      "quantity": 1,
      "unitPrice": 139.00,
      "lineTotal": 139.00
    },
    {
      "sku": "cola-01",
      "name": "Cola",
      "quantity": 1,
      "unitPrice": 29.00,
      "lineTotal": 29.00
    }
  ]
}
```

### Response fra PayNSync


### Merchant Demo JavaScript-flow

Når PayNSync returnerer `redirectUrl`, skal Merchant Demo gøre:

```javascript
const response = await fetch(`${apiBaseUrl}/api/merchant-orders`, {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify(request)
});

const result = await response.json();

if (result.redirectUrl) {
  window.location.href = result.redirectUrl;
} else {
  // Fake provider/dev: vis status direkte
  showReservationStatus(result);
}
```

Merchant Demo skal altså ikke vise et lokalt “swipe”. Den skal sende brugeren videre til Vipps/MobilePay approval flow.


PayNSync bør returnere reservation/payment info:

```json
{
  "status": "ReservationStarted",
  "orderId": 123,
  "participantPaymentId": 456,
  "providerPaymentId": "PNS-123-7",
  "redirectUrl": "https://...",
  "message": "Ordren er gemt. Godkend reservationen i MobilePay."
}
```

Merchant demo skal redirecte brugeren til `redirectUrl`, hvis den findes.

Hvis fake provider bruges, kan demoen navigere direkte til statusvisning.

---

## Ændring 2: PayNSync skal oprette Vipps payment efter draft

Når `MerchantOrderService.InitOrderAsync()` modtager en draft:

1. Valider `ParticipantToken`.
2. Find `OrderParticipant`.
3. Gem `MerchantOrderDraft` og `MerchantOrderLine`.
4. Gem eventuelt `RawMerchantPayloadJson`.
5. Sæt `OrderParticipant.Status = OrderSubmitted`.
6. Beregn `AmountMinorUnits = TotalAmount * 100`.
7. Kald orchestration/payment service for at starte reservation.
8. Opret eller genbrug `ParticipantPayment` idempotent.
9. Kald `IPaymentProvider.ReserveAsync()`.
10. Returnér `redirectUrl` til merchant demo.

Vigtigt:

- En ny draft for samme deltager og ordre må erstatte tidligere draft, hvis betalingen ikke allerede er `Reserved` eller `Captured`.
- Hvis der allerede findes en aktiv reservation, skal flowet være idempotent og ikke oprette dubletbetalinger.
- Hvis betalingen er `Captured`, må draften ikke ændres.

---

## Ændring 3: Vipps provider skal returnere redirectUrl/reference

Kontrollér at `MobilePaySandboxPaymentProvider.ReserveAsync()` returnerer:

```csharp
public sealed class PaymentReserveResult
{
    public bool Success { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
```

`ProviderPaymentId` skal være den reference, som senere bruges til capture.

Eksempel på reference-strategi:

```text
PNS-{OrderId}-{ParticipantId}-{ParticipantPaymentId}
```

Reference skal være unik og stabil.

---

## Ændring 4: Webhook må kun sætte Reserved

Vipps callback/webhook skal ikke sende ordre til merchant.

Når Vipps sender `AUTHORIZED` eller tilsvarende reserve-event:

```text
Find ParticipantPayment via ProviderPaymentId/reference
Sæt status = Reserved
Sæt ReservedAtUtc
Log PaymentEventLog
Tjek om alle betalinger i ordren er Reserved
Hvis ja: Order.Status = ReadyToPay og send host-besked
```

Vigtigt:

```text
Webhook må aldrig sende final order til merchant.
Webhook må aldrig sætte Order.Status = Paid.
Webhook må aldrig capture.
```

---

## Ændring 5: ReadyToPay skal baseres på Reserved betalinger

Ret eller erstat eksisterende logik hvor `ReadyToPay` sættes alene ud fra `OrderSubmitted`.

Ny regel:

```csharp
bool allReady = participants
    .Where(p => p.Participant.Type != Merchant)
    .All(p => participantPayments.Any(pp =>
        pp.OrderId == order.Id &&
        pp.ParticipantId == p.ParticipantId &&
        pp.Status == ParticipantPaymentStatus.Reserved));
```

Når `allReady == true`:

```text
Order.Status = ReadyToPay
Send host message: "Alle har bestilt og reserveret betaling. Du kan nu godkende den samlede ordre."
```

---

## Ændring 6: Host approve skal capture betalinger én efter én

Når host kalder:

```http
POST /api/orders/{id}/approve
```

skal PayNSync:

1. Kontrollere at requester er host.
2. Kontrollere at order status er `ReadyToPay`, `HostApproved`, `Capturing` eller `PartiallyFailed`.
3. Hente alle `ParticipantPayment` for ordren.
4. Filtrere dem med status `Reserved` eller retrybare `CaptureFailed`.
5. Sætte dem til `CapturePending`.
6. Loopes igennem betalingerne.
7. For hver betaling kaldes `IPaymentProvider.CaptureAsync(payment.ProviderPaymentId, amount)`.
8. Ved success: sæt `Captured`.
9. Ved fejl: sæt `CaptureFailed`, sæt ordre `PartiallyFailed`, stop loop eller returnér delvist resultat.
10. Når alle er `Captured`: sæt ordre `Paid`.
11. Byg og send `GroupOrderPaid` payload til merchant.

Pseudo:

```csharp
foreach (var payment in paymentsToCapture)
{
    await paymentState.SetCapturePendingAsync(payment.Id, correlationId);

    var result = await paymentProvider.CaptureAsync(new CaptureRequest
    {
        ProviderPaymentId = payment.ProviderPaymentId,
        AmountMinorUnits = payment.AmountMinorUnits,
        Currency = payment.Currency,
        CorrelationId = correlationId
    });

    if (!result.Success)
    {
        await paymentState.SetCaptureFailedAsync(payment.Id, result.ErrorMessage, correlationId);
        order.Status = OrderStatus.PartiallyFailed;
        await unitOfWork.SaveChangesAsync();
        return partialFailureResult;
    }

    await paymentState.SetCapturedAsync(payment.Id, result.ProviderTransactionId, correlationId);
}

if (allPaymentsCaptured)
{
    order.Status = OrderStatus.Paid;
    await merchantOrderSender.SendFinalGroupOrderAsync(order.Id, correlationId);
}
```

---

## Ændring 7: Send samlet GroupOrderPaid JSON til merchant

Når alle betalinger er `Captured`, skal PayNSync sende én samlet final order payload til merchantens `GroupOrderUrl` eller et nyt dedikeret endpoint.

I v1 skal PayNSync definere standardformatet. Merchant skal tilpasse sig dette format.

Eksempel:

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
        },
        {
          "sku": "cola-01",
          "name": "Cola",
          "quantity": 1,
          "unitPrice": 29.00,
          "lineTotal": 29.00
        }
      ]
    }
  ]
}
```

Merchant demo skal kunne vise/modtage denne payload, så testflowet tydeligt viser:

```text
Final group order received
Status: Paid
All participant orders captured
```

---

## Ændring 8: Merchant Demo skal kunne vise final group order

Tilføj i `Frontend.MerchantDemo` en simpel final-order modtager eller mock-visning.

Hvis merchant demo er statisk HTML/JS, kan den ikke selv modtage server-side POST direkte. Derfor er der tre mulige løsninger:

### Mulighed A: PayNSync API gemmer seneste merchant callback payload

Tilføj dev/test endpoint i PayNSync:

```http
GET /api/dev/merchant-callbacks/latest?orderId=123
```

Merchant demo kan poll'e dette endpoint og vise final order payload.

### Mulighed B: Merchant Demo får et lille backend endpoint

Lav en minimal test-server til merchant demo, som kan modtage:

```http
POST /api/paynsync/group-orders
```

### Mulighed C: PayNSync viser final payload i ordre-detalje

Tilføj i PayNSync frontend en dev/test sektion på ordre-detalje:

```text
Final Merchant Payload
```

Anbefaling for hurtig v1-test:

```text
Brug mulighed A eller C.
```

---

## Konfiguration

Kontrollér disse settings:

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

`CallbackBaseUrl` skal være offentligt tilgængelig for Vipps sandbox.

Ved lokal udvikling skal der typisk bruges ngrok eller tilsvarende tunnel:

```text
https://<ngrok-id>.ngrok-free.app/api/payments/vipps/callbacks/...
```

ReturnUrl skal sende brugeren tilbage til en PayNSync eller merchant demo status-side.

---

## Testdata

Opret/seeds mindst:

```text
Merchant: Pizzeria Roma Demo
Host: Test Host
Deltager 1: Vipps test user 1
Deltager 2: Vipps test user 2
Deltager 3: Vipps test user 3
```

Hver deltager skal have adgang til sin MobilePay/Vipps test app eller test approval flow.

Testbrugernes telefonnumre kan gemmes i testdata for identifikation, men capture-flowet skal stadig bruge `ProviderPaymentId/reference`.

---

## End-to-end testscenarie

### Scenario: 3 deltagere reserverer og host capturer

1. Log ind som host.
2. Opret gruppeordre med 3 deltagere og merchant demo.
3. Åbn deltager 1's merchant-link.
4. Vælg varer og klik `Bekræft ordre og reservér betaling`.
5. Godkend i MobilePay test app.
6. Kontrollér at deltager 1 får `ParticipantPayment.Status = Reserved`.
7. Gentag for deltager 2 og 3.
8. Kontrollér at order status bliver `ReadyToPay`.
9. Log ind som host.
10. Klik `Godkend samlet ordre`.
11. Kontrollér at PayNSync capturer betalingerne én efter én.
12. Kontrollér at alle `ParticipantPayment.Status = Captured`.
13. Kontrollér at `Order.Status = Paid`.
14. Kontrollér at final `GroupOrderPaid` payload er oprettet/sendt til merchant.

---

## Fejlscenarier der skal håndteres

### En deltager godkender ikke MobilePay

Forventet:

```text
ParticipantPayment = ReservationStarted
Order = Collecting
Host kan ikke approve endnu
Merchant får ikke final order
```

### En reservation fejler

Forventet:

```text
ParticipantPayment = ReservationFailed
Deltager kan prøve igen
Order bliver ikke ReadyToPay
```

### Capture fejler for én deltager

Forventet:

```text
Den fejlede betaling = CaptureFailed
Order = PartiallyFailed
Allerede Captured betalinger forbliver Captured
Merchant får ikke GroupOrderPaid før alle er Captured
Host kan retry capture
```

### Host prøver at approve før alle er Reserved

Forventet:

```text
API returnerer 409 Conflict eller tilsvarende domain error
Order forbliver Collecting
Ingen capture startes
```

### Merchant callback fejler

Forventet:

```text
Order kan stadig være Paid
Callback-fejl logges
Der bør være mulighed for retry af final merchant callback
```

---

## Acceptkriterier

Implementeringen er færdig når:

- Merchant demo kan sende draft-ordre til PayNSync.
- PayNSync gemmer ordrelinjer pr. deltager.
- PayNSync opretter Vipps/MobilePay reservation pr. deltager.
- Deltager kan godkende reservation i MobilePay test app.
- Vipps webhook sætter deltagerens betaling til `Reserved`.
- `ReadyToPay` sættes først når alle deltagerbetalinger er `Reserved`.
- Host kan godkende den samlede ordre.
- PayNSync capturer alle betalinger én efter én via `ProviderPaymentId/reference`.
- Capture-loopet bruger ikke telefonnummer.
- Når alle betalinger er `Captured`, bliver order status `Paid`.
- PayNSync bygger én samlet `GroupOrderPaid` JSON.
- Merchant demo/testvisning kan vise den samlede final order payload.
- Merchant modtager ikke endelig ordre før alle betalinger er captured.

---

## Forslag til implementeringsrækkefølge

1. Gennemgå eksisterende `MerchantOrderService`, `GroupPaymentOrchestrationService`, `ParticipantPaymentStateService` og `MobilePaySandboxPaymentProvider`.
2. Ret `ReadyToPay`-logikken så den baseres på `Reserved` betalinger.
3. Sørg for at `POST /api/merchant-orders` returnerer `redirectUrl` fra reservation.
4. Opdater merchant demo-knappen og redirect-flow.
5. Kontrollér Vipps webhook mapping til `Reserved`.
6. Ret host approve/capture-loop hvis nødvendigt.
7. Udvid merchant callback payload til `GroupOrderPaid` med ordrelinjer.
8. Tilføj dev/test-visning af final merchant payload.
9. Kør Fake provider test.
10. Kør Vipps sandbox test med rigtige testbrugere.

---

## Copilot/Claude instruktion

Når du implementerer dette, må du ikke ændre hele arkitekturen unødigt.

Bevar eksisterende lagdeling:

```text
Api -> Service -> DataStorage
Infrastructure.Payments implementerer IPaymentProvider
```

Brug eksisterende services hvor muligt:

```text
MerchantOrderService
GroupPaymentOrchestrationService
ParticipantPaymentStateService
MerchantCallbackService / ny IMerchantOrderSender
```

Før kodeændringer:

1. Find eksisterende flow for `POST /api/merchant-orders`.
2. Find eksisterende flow for `ReserveParticipantPaymentAsync`.
3. Find eksisterende flow for Vipps callback.
4. Find eksisterende flow for `/api/orders/{id}/approve`.
5. Lav minimal ændring så flowet følger reglerne i dette dokument.

