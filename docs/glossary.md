# Glossary

Forklaring af alle centrale begreber i PayNSync.  
Skrevet til at give en ny udvikler eller LLM en præcis forståelse af hvad tingene hedder, og hvad de betyder.

---

## Mennesker og roller

### Participant
En person eller et spisested i systemet. Alt samles i én tabel med en `Type`-markering.

Der er to typer:
- **Person** — en slutbruger der kan logge ind, oprette gruppeordrer og betale.
- **Merchant** — et spisested/restaurant. Har ekstra felter som `CompanyName`, `GroupOrderUrl`, `CvrNumber` osv.

> Kode: `Participant`, enum `ParticipantType` (`Person = 0`, `Merchant = 1`)

---

### Host (Vært)
Den person, der oprettede en gruppeordre. Host er altid `Participant` med `Type = Person`.

Host har eneret til at:
- Godkende og gennemføre betalingen (`/approve`)
- Annullere ordren (`/cancel`)
- Afslutte via det gamle flow (`/complete`, `/pay`)

Host-tjek sker ved at udlede den aktuelle deltagers ID fra det validerede JWT-claim `NameIdentifier`/`sub` og sammenligne det med `Order.CreatedByParticipantId`. Et legacy `requestingParticipantId` i request-body ignoreres ved autorisation. Manglende identitet giver 401; en autentificeret ikke-host får 403.

> Kode: `Order.CreatedByParticipantId`  
> Dansk: "Vært"

---

### Merchant
Et spisested eller en forretning registreret i systemet som `Participant` med `Type = Merchant`.

Merchants har egne felter udover dem en Person har:
- `CompanyName` — firmanavn (påkrævet ved oprettelse)
- `CvrNumber`, `VatNumber` — CVR og momsnummer
- `ContactPerson`, `ContactEmail`, `ContactPhone` — kontaktoplysninger
- `CompanyAddress` — adresse
- `GroupOrderUrl` — URL til merchantens bestillingsside (bruges i deltagerlinks)
- `PaymentReference`, `PayoutAccountInfo`, `PaymentProvider` — betalingsoplysninger

> Kode: `Participant` med `Type = Merchant`

---

### Invited Participant (Inviteret deltager)
En `Person` som Host har inviteret til en gruppeordre. Tilføjes ved ordreoprettelse med status `Invited`.

Deltageren modtager automatisk en besked med et personligt bestillingslink til merchantens side.

> Kode: `OrderParticipant.Status = "Invited"`

---

## Ordrekoncepter

### Group Order (Gruppebetaling / Gruppeordre)
En samlet bestilling hos en merchant, fordelt på flere deltagere. Hver deltager betaler sin egen del direkte.

Det er den centrale forretningsenhed i PayNSync.

> Kode: `Order`

---

### Order
Dataobjektet der repræsenterer en gruppeordre. Indeholder:
- Hvem der oprettede den (`CreatedByParticipantId`)
- Titel og kategori
- Hvilken merchant der er tilknyttet (`MerchantParticipantId`)
- Liste af deltagere (`OrderParticipants`)
- Ordrestatus (`Status`)
- Beskeder (`Messages`)
- Merchant-bestillinger (`MerchantOrderDrafts`)
- Et `JoinToken` til fremtidig brug

> Kode: `Order`, `OrderDto`, `OrderSummaryDto`, `OrderOverviewDto`

---

### OrderParticipant
Forbindelsesrecord mellem en `Order` og en `Participant`. Én pr. deltager pr. ordre.

Indeholder:
- `Status` — deltagerens aktuelle tilstand i ordren
- `ParticipantToken` — unikt GUID der identificerer deltageren i merchant-linket

> Kode: `OrderParticipant`, `OrderParticipantDto`

---

### JoinToken
Et GUID genereret på selve `Order`-objektet. Tanken er at det kan bruges til et "join via link"-flow, hvor nogen kan tilmelde sig ordren via et link. 

Genereres ved ordreoprettelse men bruges ikke endnu — der er ingen endpoint der accepterer et join via dette token.

> Kode: `Order.JoinToken`

---

### ParticipantToken
Et unikt GUID pr. `OrderParticipant`. Bruges i det personlige bestillingslink der sendes til deltageren:

```
{merchant.GroupOrderUrl}?orderId=X&merchantId=Y&participantToken=Z
```

Når deltageren bestiller via merchantens side, sendes dette token med for at bevise hvem de er. `MerchantOrderService` validerer token'et mod databasen.

> Kode: `OrderParticipant.ParticipantToken`  
> Unikt index i databasen

---

### MerchantOrderDraft
Den bestilling en deltager har lavet via merchantens bestillingsside. Indeholder de konkrete varer/retter og beløb.

Vigtige regler:
- Én draft pr. deltager pr. ordre
- Gen-indsendelse sletter den forrige draft
- Betalingsreservation startes automatisk når draftet indsendes

> Kode: `MerchantOrderDraft`, `MerchantOrderDraftDto`

---

### MerchantOrderLine
En enkelt varelinje i et `MerchantOrderDraft`. Indeholder navn, antal, enhedspris og linjetotal.

> Kode: `MerchantOrderLine`, `MerchantOrderLineDto`

---

## Betalingskoncepter

### Reservation (Reserve)
Det første trin i betalingsflowet. Beløbet "fryses" på deltagerens konto hos betalingsudbyderen — pengene er ikke trukket endnu, men de er reserveret.

Svarer til "autorisation" i kortterminologi.

Hos Vipps MobilePay hedder dette `AUTHORIZED`.  
Hos FakeProvider sker dette synkront.

> Kode: `ParticipantPayment.Status = Reserved`  
> Kode: `IPaymentProvider.ReserveAsync()`

---

### Capture
Det andet og afsluttende trin i betalingsflowet. Pengene trækkes fra deltagerens konto og overføres til merchant.

Capture sker først når Host godkender ordren via `/approve`. Alle reserverede betalinger captures i én operation.

> Kode: `ParticipantPayment.Status = Captured`  
> Kode: `IPaymentProvider.CaptureAsync()`

---

### ParticipantPayment
Den provider-understøttede betalingspost. Én pr. deltager pr. ordre.

Tracket hele livscyklussen: `Created → ReservationStarted → Reserved → CapturePending → Captured` (eller fejlstater).

Adskiller sig fra den ældre `Payment`-entitet ved at have:
- `AmountMinorUnits` (long, øre)
- `ProviderPaymentId` (ID hos Vipps/Fake)
- Fuld state machine
- Audit trail via `PaymentEventLog`

> Kode: `ParticipantPayment`

---

### Payment (legacy)
Den ældre simple betalingspost. Ingen provider-integration. Bruges stadig af det manuelle betalingsflow (`POST /api/payments`).

Gemmer blot beløb i kr (decimal) og status `"Completed"`.

> Kode: `Payment`, `PaymentDto`

---

### Pending Payment (Afventende betaling)
En deltager hvis betaling ikke er gennemført endnu. I praksis: en deltager med `OrderParticipant.Status = "Invited"` (har ikke bestilt) eller `ParticipantPayment.Status` i en ikke-afsluttet tilstand.

Frontend beregner "afventende deltagere" client-side i `computePendingSummary()` ved at tælle deltagere med status `Invited`.

---

### PaymentEventLog
Uforanderlig auditlog. Hver gang en `ParticipantPayment` skifter status, skrives en ny post med:
- Gammel og ny status
- Hvad der skete (`EventType`)
- Hvornår (`CreatedAtUtc`)
- Korrelations-ID til at spore en operation på tværs af log-poster

> Kode: `PaymentEventLog`

---

### RowVersion
EF Core optimistisk concurrency-token på `ParticipantPayment`. Forhindrer to samtidige opdateringer fra at overskrive hinanden.

> Kode: `ParticipantPayment.RowVersion`

---

## Statusbegreber

### Ordrestatus

| Kodeværdi | Dansk | Hvornår |
|-----------|-------|---------|
| `Collecting` | Samler bestillinger | Startsstatus — deltagere er ved at bestille |
| `ReadyToPay` | Klar til betaling | Alle deltagere har indsendt bestilling |
| `HostApproved` | Godkendt af vært | Host har kaldt `/approve`, capture er startet |
| `Capturing` | Gennemfører betalinger | Capture-loop kører |
| `Paid` | Betalt | Alle betalinger captured — terminal |
| `PartiallyFailed` | Delvis fejlet | Mindst én capture fejlede — kan retry |
| `Cancelled` | Annulleret | Host annullerede — terminal |
| `Completed` | Afsluttet | Legacy terminal-status fra det gamle flow |

---

### Deltager-status (OrderParticipant)

| Kodeværdi | Hvornår tildelt |
|-----------|-----------------|
| `Invited` | Tilføjet til ordren — har ikke bestilt endnu |
| `Accepted` | Host auto-tildeles denne ved ordreoprettelse |
| `OrderSubmitted` | Deltager har indsendt sin bestilling via merchant-siden |
| `Paid` | Betaling registreret (legacy flow) |
| `Declined` | Kun frontend-enum — ingen backend sætter denne |

---

### Betalingsstatus (ParticipantPaymentStatus)

| Kodeværdi | Dansk label | Hvornår |
|-----------|-------------|---------|
| `Created` | Afventer | Post oprettet, ikke sendt til provider endnu |
| `ReservationStarted` | Åbnet menukort | Provider-kald igangsat |
| `Reserved` | Betaling reserveret | Beløb reserveret/autoriseret hos provider |
| `ReservationFailed` | Fejlet | Provider afviste reservationen |
| `CapturePending` | Afventer godkendelse | Host har godkendt, capture er sat i kø |
| `Captured` | Betaling gennemført | Penge trukket fra konto |
| `CaptureFailed` | Fejlet | Capture-kald fejlede hos provider |
| `Cancelled` | Annulleret | Reservation annulleret |
| `Expired` | Udløbet | Defineret i enum og terminaltilstand; ingen service-metode sætter denne status. Vipps `EXPIRED`/`TERMINATED`-callbacks sætter `ReservationFailed` (via `SetReservationFailedAsync`), ikke `Expired` |
| `Refunded` | Refunderet | Defineret men ikke implementeret |

---

## Tekniske begreber

### GroupOrderUrl
URL konfigureret på en `Merchant`-deltager. Peger på merchantens bestillingsside. Bruges til:
1. At bygge de personlige deltagerlinks (`?orderId=X&participantToken=Y`)
2. At sende merchant callback (HTTP POST) når alle betalinger er captured

Fallback: hvis ikke sat, bruges `AppSettings:MerchantDemoUrl` fra konfiguration.

> Kode: `Participant.GroupOrderUrl`

---

### MerchantCallback
HTTP POST sendt fra `MerchantCallbackService` til `Merchant.GroupOrderUrl` efter alle betalinger er captured.

Payload indeholder `orderId`, `merchantId`, `status: "Paid"` og betalingsstatus pr. deltager.  
Fejl i callback stopper ikke flowet.

---

### IPaymentProvider
Interface der abstraherer al kommunikation med betalingsudbyderen. Har fire metoder:
- `ReserveAsync` — reservér betaling
- `CaptureAsync` — gennemfør betaling
- `CancelAsync` — annullér reservation
- `GetStatusAsync` — forespørg status

To implementeringer: `FakePaymentProvider` og `MobilePaySandboxPaymentProvider`.

---

### Fake Provider
`FakePaymentProvider` — bruges i udvikling og tests. Ingen rigtige HTTP-kald. Returnerer success synkront.

Adfærd kan simuleres via `appsettings.json`:
- `SimulateReservationFailed` — reservation afvises
- `SimulateCaptureFailed` — capture fejler
- `SimulateReserveException` — kaster exception
- osv.

---

### Vipps MobilePay (MobilePay Sandbox Provider)
`MobilePaySandboxPaymentProvider` — rigtig integration mod Vipps MobilePay ePayment API.

Bruger OAuth2 client credentials til at hente access token (cachet, fornyes 5 min før udløb).  
Sender betalinger til brugerens MobilePay-app via `userFlow: "WEB_REDIRECT"`.  
Modtager status-opdateringer via webhook (`VippsCallbackController`).

---

### IdempotencyKey
En streng der sendes med til `IPaymentProvider` for at forhindre dobbelt-operationer.

Format:
- Reserve: `reserve-{paymentId}-{orderId}-{participantId}`
- Capture: `capture-{paymentId}-{orderId}`
- Cancel: `cancel-{paymentId}-{orderId}`

---

### CorrelationId
En streng der logges i `PaymentEventLog` for at knytte relaterede hændelser sammen.

Eksempel: `"webhook-FAKE-123-abc"` — alle log-poster fra den samme webhook-hændelse deler dette ID.

---

### ProviderPaymentId
Det ID som betalingsudbyderen (Vipps/Fake) tildeler betalingen. Bruges til:
- At finde den rigtige `ParticipantPayment` i webhook-callbacks
- At sende capture- og cancel-kald til udbyderen

Midlertidigt ID `"pending-{id}"` bruges fra reserve-start til provider svarer.

> Kode: `ParticipantPayment.ProviderPaymentId`

---

### Minor Units (Øre)
Beløb sendt til betalingsudbyderen gemmes som `long` i mindste valutaenhed (øre).

Eksempel: `9900` = 99,00 DKK.

Konvertering i koden: `amountMinorUnits = (long)(draft.TotalAmount * 100)`.

> Kode: `ParticipantPayment.AmountMinorUnits`

---

### Merchant Demo (Pizzeria Roma)
Statisk HTML-side (`Frontend.MerchantDemo/index.html`) der simulerer en rigtig merchants bestillingsside.

Bruges til at demonstrere og teste deltager-bestillingsflowet. Læser `orderId`, `merchantId` og `participantToken` fra URL-query-parametre og poster til `/api/merchant-orders`.

Kører på port 8081 i development og Local-miljø (startes automatisk af `MerchantDemoHostedService`).

---

### ParticipantExternalLogin *(NYT)*
Entitet der knytter en `Participant` til et eksternt login-system (fx Google).

Indeholder:
- `Provider` — udbyderens navn, fx `"Google"` eller `"Apple"`
- `ProviderUserId` — brugerens unikke ID hos udbyderen (subject claim, fx Google sub)
- `Email` — e-mail returneret af udbyderen på tilknytningstidspunktet
- `CreatedAtUtc`

Bruges af `ExternalAuthService.GoogleLoginAsync()` til at slå en Google-bruger op og knytte den til en `Participant`.

Én `Participant` kan have flere `ParticipantExternalLogin`-poster (én pr. udbyder).

> Kode: `ParticipantExternalLogin`, `IParticipantExternalLoginRepository`

---

### ExternalAuthService / IExternalAuthService *(NYT)*
Service der håndterer login via eksterne OAuth-udbydere (i v1 kun Google).

Validerer Google ID-tokens via `GoogleJsonWebSignature.ValidateAsync()` fra pakken `Google.Apis.Auth`.  
Finder eksisterende `Participant` via `ParticipantExternalLogin`, eller opretter ny ved første Google-login.  
Kaster `ExternalLoginEmailConflictException` hvis e-mailen allerede er registreret med adgangskode — ingen automatisk kontosammenslåning.

Konfigurationsafhængighed: `Google:ClientId` i `appsettings.json`.

> Kode: `ExternalAuthService`, `IExternalAuthService`  
> Endpoint: `POST /api/auth/google-login`

---

### ExternalLoginEmailConflictException *(NYT)*
Custom exception kastet af `ExternalAuthService`, når en bruger forsøger Google-login med en e-mail der allerede er registreret med adgangskode.

Resulterer i HTTP 409 Conflict med en fejlbesked til klienten.  
Ingen automatisk sammenslåning af eksisterende konto med Google-login.

> Kode: `ExternalLoginEmailConflictException`

---

### VippsTestUserId *(NYT)*
Et nullable self-reference FK-felt på `Participant`. Peger på en anden `Participant` i databasen der repræsenterer brugeren i Vipps sandbox.

Bruges i sandbox-testflowet: når en deltager skal reservere betaling via Vipps MobilePay i testmiljøet, bruges testpersonens telefonnummer til at starte reservationen.

**Vigtigt:** Kun til sandbox-brug. Feltet har ingen effekt i produktionsflowet.

> Kode: `Participant.VippsTestUserId`  
> Endpoints: `GET /api/participants/vipps-test-users`, `PATCH /api/participants/{id}/vipps-test-user`

---

### Per-merchant Vipps-credentials *(NYT)*
Fire nullable felter på `Participant` (kun relevant for Merchant-type):

| Felt | Formål |
|------|--------|
| `VippsMerchantSerialNumber` | MSN (Merchant Serial Number) for dette salgssted |
| `VippsClientId` | Vipps API client ID |
| `VippsClientSecret` | Vipps API client secret |
| `VippsSubscriptionKey` | Vipps Ocp-Apim-Subscription-Key |

`null`-værdier bevirker at den globale konfiguration fra `appsettings.json` bruges i stedet.  
`VippsMerchantSerialNumber` er **påkrævet** ved registrering af ny merchant via `POST /api/auth/register-merchant`.

> Kode: `Participant.VippsMerchantSerialNumber`, `.VippsClientId`, `.VippsClientSecret`, `.VippsSubscriptionKey`

---

### RawMerchantPayloadJson *(NYT)*
Nullable felt på `MerchantOrderDraft`. Gemmer merchantens originale JSON-request-body som modtaget via `POST /api/merchant-orders`.

Formål: audit, debugging og fremtidig brug til merchant-specifikke adapters.  
Indholdet er uændret fra hvad merchant sendte — PayNSync normaliserer og gemmer data separat i `MerchantOrderLine`-records.

> Kode: `MerchantOrderDraft.RawMerchantPayloadJson`

---

### PayNSyncFinalGroupOrderDto *(NYT)*
Standard JSON-payload som PayNSync sender til merchant (`Merchant.GroupOrderUrl`) efter alle deltagerbetalinger er captured og ordren er `Paid`.

Indeholder:
- `eventType: "GroupOrderPaid"`
- `paynsyncOrderId`, `merchantId`, `status: "Paid"`, `currency`, `totalAmount`, `paidAtUtc`
- `participants[]` — liste af `PayNSyncFinalParticipantOrderDto`

Merchant mapper dette format til sit eget ordre-/POS-system. PayNSync forsøger ikke at tilpasse payloaden per merchant i v1.

> Kode: `PayNSyncFinalGroupOrderDto`, `PayNSyncFinalParticipantOrderDto`, `PayNSyncFinalOrderLineDto`
