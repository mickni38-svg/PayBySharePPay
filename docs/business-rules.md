# Business Rules

Regler udledt direkte fra kode (services, controllers, entities, state machine).  
Organiseret efter aktør og domæneområde.

---

## Besluttet kerneflow for betaling og ordrefrigivelse

PayNSync v1 bruger følgende ansvarsdeling:

| Område | Ansvarlig |
|--------|-----------|
| Menu, priser, kurv og ordrelinjer | Merchant |
| Gruppeordre, deltagere og participant tokens | PayNSync |
| Oprettelse af MobilePay/Vipps reservation | PayNSync |
| Deltagerens godkendelse/swipecall | Vipps/MobilePay app |
| Betalingsstatus, capture og retry | PayNSync |
| Endelig ordreaccept til merchant | PayNSync |

Grundregel:

> **En individuel MobilePay/Vipps-godkendelse reserverer kun deltagerens beløb. Den frigiver aldrig ordren til merchant. Merchant modtager først den samlede ordre, når alle deltagere har reserveret, host har godkendt, og PayNSync har captured alle betalinger.**

Merchant-knappen bør derfor ikke hedde `Betal`. Den bør hedde fx `Bekræft min ordre` eller `Gem ordre og reservér betaling`, fordi deltageren først godkender en reservation. Det endelige træk sker senere via capture.

---

## Deltagere (Participants)

### Typer og oprettelse

- En `Participant` er enten `Person` eller `Merchant` (`ParticipantType` enum — enkelt tabel, single-table inheritance).
- En **Person** kræver et navn (`Name` må ikke være tomt eller whitespace).
- En **Merchant** kræver et firmanavn (`CompanyName` må ikke være tomt eller whitespace).
- En **Merchant** kræver firmanavn, konto-email, password og Vipps MSN-nummer (`VippsMerchantSerialNumber`) ved registrering via `POST /api/auth/register-merchant`. Per-merchant kan også sættes `VippsClientId`, `VippsClientSecret` og `VippsSubscriptionKey`; null-værdier bevirker at global konfiguration bruges.
- Password hashes med BCrypt ved oprettelse.
- Konto-email er påkrævet ved registrering gennem auth-API'et og skal være unik på tværs af Person og Merchant.

### Login

- Login sker på email for både `Person` og `Merchant` (trimmet og case-insensitivt).
- Almindeligt login kræver password, og BCrypt-verifikation skal bestå.
- Passwordløst login tillades kun for en eksisterende `Person` uden password-hash, når ASP.NET Core kører i `Development`. Det afvises for merchants, konti med hash og alle andre miljøer.
- Vellykket login og registrering returnerer participant-type sammen med JWT og participant-ID.
- Vellykkede logins returnerer JWT med `sub` = `participantId`, `name` = navn, `jti` = nyt GUID. Token-levetid styres af `Jwt:ExpiresInMinutes` i konfigurationen (default **43200 min / 30 dage** i `appsettings.json`). `AuthController` returnerer en hardkodet `ExpiresAt = now + 480 min` i response-body — dette afspejler **ikke** den faktiske token-levetid (se Open Questions #2 i arkitektur-dokumentet).

### Google-login *(NYT)*

- `POST /api/auth/google-login` modtager et Google ID-token og validerer det via `GoogleJsonWebSignature.ValidateAsync()` (kræver `Google:ClientId` i konfigurationen).
- Hvis Google-subjekt allerede er knyttet til en `ParticipantExternalLogin` med `Provider = "Google"`, bruges den eksisterende `Participant`.
- Hvis e-mailen allerede er registreret hos en `Participant` med `PasswordHash`, kastes `ExternalLoginEmailConflictException` → HTTP 409. Ingen automatisk sammenslåning af konti.
- Hvis ingen matchende login-tilknytning eller konto findes, oprettes en ny `Participant` (type `Person`) og en ny `ParticipantExternalLogin`-post.
- Vellykkede Google-logins returnerer JWT på samme format som email-login.

### Vipps test-user mapping *(NYT)*

- `Participant.VippsTestUserId` er en nullable self-ref FK. Den peger på en anden `Participant` der repræsenterer brugeren i Vipps sandbox.
- `GET /api/participants/vipps-test-users` returnerer liste af alle testpersoner med mappings-status (`MappedByParticipantId`).
- `PATCH /api/participants/{id}/vipps-test-user` sætter `VippsTestUserId` for én deltager.
- Mapping er udelukkende til sandbox-brug og bruges ikke i produktionsflowet.

### Profilopdatering

- Navn må ikke være tomt ved opdatering.
- Kun `Name`, `Email` og `Phone` kan opdateres via `UpdateProfileAsync`.
- `PasswordHash`, `Type` og merchant-felter kan ikke ændres via profilopdatering.

### Venneforbindelser

- En `FriendRelation` er rettet (Initiator → Receiver), men behandles som tovejs i alle forespørgsler.
- En bruger må **ikke** tilføje sig selv som ven (`InitiatorId == ReceiverId` → `InvalidOperationException`).
- En relation kan ikke oprettes, hvis den allerede eksisterer (`RelationExistsAsync` tjekker begge retninger).
- Der er **ingen accept-flow** — venskabet er effektivt øjeblikkeligt ved `POST /api/friends`.

---

## Host-regler

- **Host** er altid den `Participant`, der oprettede ordren (`Order.CreatedByParticipantId`).
- Host tilføjes automatisk som `OrderParticipant` med status `Accepted` ved ordreoprettelse — host behøver ikke selv bestille.
- Kun host kan kalde:
  - `POST /api/orders/{id}/approve` (godkend og capture alle betalinger)
  - `POST /api/orders/{id}/cancel` (annuller ordren)
  - `POST /api/orders/{id}/complete` (legacy afslut)
  - `POST /api/orders/{id}/pay` (legacy betal)
- Controlleren udleder det aktuelle participant-ID fra det validerede JWT-claim `NameIdentifier`/`sub`; body-feltet `requestingParticipantId` bruges ikke til autorisation.
- Service-laget sammenligner JWT-identiteten med `Order.CreatedByParticipantId`.
- Manglende/ugyldigt identitets-claim giver 401. Fejlet host-tjek kaster `UnauthorizedAccessException`, som middleware mapper til et generisk 403-svar.

---

## Deltager-regler (OrderParticipant)

### Invitationer

- Deltagere angivet ved ordreoprettelse tilføjes med status `Invited`.
- Der genereres et unikt `ParticipantToken` (GUID, `ToString("N")`) pr. `OrderParticipant`.
- `ParticipantToken` har et unikt index i databasen.
- Deltagere kan **ikke** tilføjes til en ordre efter oprettelse (ingen endpoint for dette).

### Statuser

| Status | Hvornår tildelt |
|--------|-----------------|
| `Invited` | Ved ordreoprettelse for inviterede deltagere |
| `Accepted` | Automatisk for host ved ordreoprettelse |
| `OrderSubmitted` | Sat af `MerchantOrderService` når merchant-draft indsendes |
| `Paid` | Sat af `PaymentService` (legacy) eller synkroniseret i `GetOrderOverviewAsync` |
| `Declined` | Kun defineret i frontend-enum — **ingen backend-logik sætter denne status** |

---

## Ordre-regler

### Validering ved oprettelse

- Mindst ét af `Title` eller `Category` skal være udfyldt (begge må ikke være tomme/whitespace).
- Opretteren skal eksistere som `Participant` i databasen.
- Alle angivne deltager-IDs skal eksistere i databasen.
- Merchant (hvis angivet) skal eksistere i databasen.
- Et unikt `JoinToken` (GUID) genereres til ordren ved oprettelse.

### Merchant-tilknytning

- Én ordre kan maksimalt have én merchant (`Order.MerchantParticipantId` nullable FK).
- Hvis merchant er tilknyttet, genereres et personligt bestillingslink til **alle** `OrderParticipants` (inkl. host):  
  `{merchant.GroupOrderUrl ?? MerchantDemoUrl}?orderId={id}&merchantId={mid}&participantToken={token}`
- Disse links sendes som `Message`-records til hver deltager.
- Hvis ingen merchant er valgt, sendes en generel invitationstekst kun til **inviterede** (ikke host).

### Ordre-statusmaskine

```
Collecting
  └─► (deltagere indsender merchant-drafts)
        └─► OrderSubmitted pr. deltager
              └─► PayNSync opretter MobilePay/Vipps reservation pr. deltager
                    └─► Deltager swiper/godkender i MobilePay/Vipps
                          └─► ParticipantPayment = Reserved

Når alle ikke-merchant deltagere har ParticipantPayment.Status = Reserved:
  └─► ReadyToPay
        ├─► (host kalder /approve)
        │     └─► HostApproved
        │           └─► Capturing
        │                 ├─► Paid              (alle captured OK)
        │                 └─► PartiallyFailed   (≥1 capture fejlede)
        │                       └─► (kan retry via /approve igen)
        └─► Cancelled   (host kalder /cancel)

Collecting → Cancelled          (host kan altid annullere)
ReadyToPay → Cancelled          (host kan annullere)
HostApproved → Cancelled        (host kan annullere under capture)
Capturing → Cancelled           (host kan annullere)
PartiallyFailed → Cancelled     (host kan annullere)

Paid → (terminal — kan IKKE annulleres)

ReadyToPay → Completed          (legacy via /complete eller /pay)
```

Vigtigt:

- `OrderSubmitted` betyder kun, at deltagerens ordrelinjer er gemt.
- `OrderSubmitted` må **ikke** alene sætte hele ordren til `ReadyToPay`.
- `ReadyToPay` betyder, at alle relevante deltagerbetalinger er `Reserved`.
- En individuel MobilePay/Vipps reservation må aldrig sende/frigive ordren til merchant.

- Ordren kan **ikke** annulleres hvis status er `Paid`.
- Idempotent: allerede `Paid` returnerer success uden fejl fra `/approve`.
- Idempotent: allerede `Cancelled` returnerer success uden fejl fra `/cancel`.

---

## Merchant-regler


## Merchant Demo og MobilePay/Vipps-start

Merchant Demo starter ikke MobilePay/Vipps direkte. Merchant Demo starter PayNSyncs reservation-flow.

Regler:

- Merchant Demo må kun sende draft-ordre, totalbeløb, currency, `participantToken` og eventuelt testtelefonnummer til PayNSync API.
- PayNSync backend opretter Vipps/MobilePay payment/reservation.
- PayNSync backend returnerer `redirectUrl` til Merchant Demo.
- Merchant Demo redirecter deltageren til `redirectUrl`.
- Deltageren swiper/godkender i MobilePay/Vipps app/test flow.
- PayNSync modtager webhook og sætter den konkrete `ParticipantPayment` til `Reserved`.
- Merchant Demo må ikke indeholde Vipps/MobilePay secrets eller kalde Vipps API direkte.
- Merchant Demo må ikke capture betalinger.
- Merchant Demo må ikke selv afgøre om den samlede ordre er betalt.

Knappen på Merchant Demo bør hedde:

```text
Bekræft ordre og reservér med MobilePay
```

Forklaringstekst bør tydeligt sige:

```text
Du bliver sendt til MobilePay for at godkende en reservation.
Beløbet trækkes først, når alle i gruppen har bestilt, og værten godkender den samlede ordre.
```

Telefonnummer/testtelefonnummer:

- Kan bruges til at starte/oprette en Vipps/MobilePay payment i sandbox.
- Må ikke bruges som capture-nøgle.
- Capture skal altid ske via `ProviderPaymentId` / Vipps reference på en allerede godkendt reservation.


### Merchant-draft (bestilling)

- `POST /api/merchant-orders` er `[AllowAnonymous]` — kræver ingen JWT.
- `ParticipantToken` i requesten valideres mod `OrderParticipant.ParticipantToken` for den givne ordre. Ugyldigt token → `UnauthorizedAccessException`.
- En `Merchant`-type deltager kan **ikke** indsende en bestillingsdraft (kun `Person` kan).
- **Én draft pr. deltager pr. ordre** — gen-indsendelse sletter den forrige draft og opretter en ny.
- Alle ordrelinjer i en draft tildeles `ParticipantId` fra den fundne `OrderParticipant`.
- Draft-status sættes til `"Submitted"` (ikke `"Draft"`, som er default-værdien i entiteten).
- Betalingsreservation startes **automatisk** ved draft-indsendelse (`MerchantOrderService` kalder `ReserveParticipantPaymentAsync`).
- Merchant må ikke selv oprette MobilePay/Vipps-betalingen i v1. Merchant sender kun ordrelinjer og totalbeløb til PayNSync.
- En deltager skal efter ordre-draft sendes til MobilePay/Vipps-flowet og selv swipe/godkende reservationen. PayNSync må ikke senere betale på deltagerens vegne kun ud fra telefonnummer eller MobilePay-id.
- `AmountMinorUnits` beregnes som `(long)(draft.TotalAmount * 100)`.
- En gemt merchant-draft betyder ikke, at merchant må lave/frigive ordren. Draften er kun deltagerens ønskede ordre, indtil den samlede gruppeordre er captured og accepteret.

### PayNSync Merchant Integration Contract v1

- Merchant skal i v1 tilpasse sig PayNSyncs standard **Group Order Contract**.
- PayNSync understøtter ikke merchant-specifikke payload-mappinger i v1.
- Merchant skal kunne sende deltagerens draft-ordre til PayNSync og modtage én final group order fra PayNSync.
- PayNSync bør gemme normaliserede ordrelinjer og kan gemme merchantens originale JSON som `RawMerchantPayloadJson` til audit, debugging og fremtidige merchant adapters.
- Merchant-specific adapters kan komme senere, men må ikke komplicere v1-kerneflowet.

### Callback til merchant / final group order

- Merchant-callback må **kun** sendes som endelig ordreaccept efter samlet successful capture.
- Når alle betalinger er captured, sender `MerchantCallbackService` / `IMerchantOrderSender` én HTTP POST til `Merchant.GroupOrderUrl`.
- Payload skal være PayNSyncs standard `GroupOrderPaid` contract.
- Payload skal som minimum indeholde:
  - `eventType = "GroupOrderPaid"`
  - `paynsyncOrderId`
  - `merchantId`
  - `status = "Paid"`
  - `currency`
  - `totalAmount`
  - `paidAtUtc`
  - `participants[]` med deltagergrupperede ordrelinjer
  - `paymentStatus = "Captured"` pr. deltager
- Merchant må først lave/frigive ordren efter modtagelse af final group order med `status: "Paid"`.
- Callback-fejl stopper **ikke** flowet — betalingerne er allerede gennemført. Fejl skal dog logges og kunne håndteres af support/drift.
- Hvis `Merchant.GroupOrderUrl` er null/tom, springes callback over. Det bør kun være tilladt i dev/test eller ved merchants uden aktiv integration.
- PayNSync må ikke sende endelig merchant-callback ved `OrderSubmitted`, `ReservationStarted` eller enkelt-deltager `Reserved`.

---

## Betalingsregler

### ParticipantPayment (provider-backed)

- Én `ParticipantPayment` pr. deltager pr. ordre.
- `AmountMinorUnits` gemmes som `long` (øre/cents).
- Currency defaults til `"DKK"`.
- `RowVersion` (byte array) er konfigureret som EF Core optimistisk concurrency-token.

### Reserve-regler

- **Idempotens:** hvis en ikke-cancelled, ikke-failed, ikke-expired `ParticipantPayment` allerede eksisterer for samme deltager+ordre, returneres den eksisterende uden nyt provider-kald.
- Reservation startes **automatisk af `MerchantOrderService.InitOrderAsync`** umiddelbart efter draft er oprettet og `OrderParticipant.Status` sat til `OrderSubmitted`. Reservation kan også startes direkte via `POST /api/orders/{id}/reserve`.
- Reservationen er en **deltager-godkendt reservation**, ikke en endelig betaling. Deltageren skal swipe/godkende i MobilePay/Vipps.
- Et successful swipe/webhook må kun sætte den konkrete `ParticipantPayment` til `Reserved` og derefter tjekke, om hele ordren kan sættes til `ReadyToPay`.
- Reservationsflow:
  1. Opret `ParticipantPayment` (status `Created`)
  2. Sæt temp `ProviderPaymentId = "pending-{id}"`, status → `ReservationStarted`
  3. Kald `IPaymentProvider.ReserveAsync`
  4. Success: opdatér `ProviderPaymentId` med rigtigt provider-ID og returnér betalings-/redirect-information til deltageren
  5. Fake provider: sæt status → `Reserved` synkront; kald umiddelbart `CheckAndSetReadyToPayByReservedAsync` — sætter ordre til `ReadyToPay` hvis alle ikke-merchant deltagere nu er `Reserved`
  6. Rigtig provider: status forbliver `ReservationStarted` indtil webhook fra Vipps/MobilePay
  7. Webhook `AUTHORIZED`/`RESERVE`: status → `Reserved`; `VippsCallbackController` kalder derefter `CheckAndSetReadyToPayByReservedAsync` — sætter ordre til `ReadyToPay` hvis alle ikke-merchant deltagere nu er `Reserved`
  8. Fejl/exception: status → `ReservationFailed`

### Capture-regler

- Kun host kan godkende (`currentParticipantId` fra JWT skal matche `order.CreatedByParticipantId`).
- Capture må først ske, når deltagerne allerede har godkendt deres reservationer i MobilePay/Vipps.
- Ordren skal være i `ReadyToPay`, `HostApproved`, `Capturing` eller `PartiallyFailed` for at capture må starte.
- Mindst én `Reserved` betaling skal eksistere — ellers fejler kaldet.
- Rækkefølge:
  1. Alle `Reserved` → `CapturePending` (med `CaptureStartedAtUtc`)
  2. Ordre → `HostApproved`, derefter → `Capturing`
  3. For hver `CapturePending` betaling: kald `IPaymentProvider.CaptureAsync`
  4. Success → `Captured` (med `CapturedAtUtc`)
  5. Fejl/exception → `CaptureFailed` + ordre → `PartiallyFailed` + **stop loop**
- Allerede `Captured` betalinger springes over (idempotens).
- Alle captured → ordre → `Paid`.
- Først når ordren er `Paid`, må PayNSync sende endelig samlet ordreaccept til merchant.

### Cancel-regler

- Kun host kan annullere.
- `Paid` ordre kan **ikke** annulleres.
- Allerede `Captured`, `Cancelled` eller `Expired` betalinger springes over.
- Betalinger uden `ProviderPaymentId` sættes direkte til `Cancelled` (ingen provider-kald).
- Provider-cancel-fejl indsamles men stopper **ikke** loopet.
- Ordre sættes altid til `Cancelled` til sidst — selv hvis enkelt-cancel-kald fejlede.

### Legacy betalingsflow

- `POST /api/payments` opretter en `Payment`-record (ikke `ParticipantPayment`) med status `Completed` og sender en host-notifikation.
- `POST /api/orders/{id}/pay` kalder `ExternalPaymentService.ChargeAsync()` (stub — returnerer altid success) og sætter ordren til `Completed`.
- Legacy-flows eksisterer parallelt med det nye provider-flow.

---

## Betalingsstatus-maskine (ParticipantPaymentStatus)

Enforced af `ParticipantPaymentStateService.TransitionTo()` — ugyldige overgange kaster `InvalidOperationException`.

```
Created
  └─► ReservationStarted  (reserve startet)
  └─► Cancelled

ReservationStarted
  └─► Reserved            (provider bekræftede reservation)
  └─► ReservationFailed   (provider afviste)
  └─► Cancelled

Reserved
  └─► CapturePending      (host godkendte)
  └─► Cancelled

CapturePending
  └─► Captured            (capture success)
  └─► CaptureFailed       (capture fejlede)

CaptureFailed
  └─► CapturePending      (kan retry)

Captured
  └─► Refunded            (state defineret — ingen logik implementeret)

ReservationFailed  →  (terminal)
Cancelled          →  (terminal)
Expired            →  (terminal)
Refunded           →  (terminal)
```

Alle state-skift logger en `PaymentEventLog`-post med `OldStatus`, `NewStatus`, `EventType`, `CorrelationId` og `CreatedAtUtc`.

Alle state-metoder er **idempotente** — allerede i målstatus returnerer uden fejl.

---

## Webhook-regler

### Generiske webhooks (`PaymentsController`)

| Indgående `Status` (case-insensitiv) | Handling |
|--------------------------------------|----------|
| `"RESERVED"` eller `"AUTHORIZED"` | → `SetReservedAsync` |
| `"CANCELLED"` | → `SetCancelledAsync` |
| `"FAILED"` | → `SetReservationFailedAsync` |
| Andet | Returnerer `Accepted: false` med besked — ingen state-ændring |

Lookup sker på `ProviderPaymentId`. Ikke fundet → 404.

### Vipps-specifik callback (`VippsCallbackController`)

| Vipps `Name`-felt (case-insensitiv) | Handling |
|-------------------------------------|----------|
| `"AUTHORIZED"` eller `"RESERVE"` | → `SetReservedAsync` for den konkrete deltagerbetaling; derefter `CheckAndSetReadyToPayByReservedAsync` — sætter ordre til `ReadyToPay` hvis alle ikke-merchant deltagere nu er `Reserved` |
| `"CAPTURED"` | Logger og ignorerer — ingen state-ændring (capture sker via vores eget `/approve`-flow) |
| `"CANCELLED"` eller `"ABORTED"` | → `SetCancelledAsync` |
| `"TERMINATED"` eller `"EXPIRED"` | → `SetReservationFailedAsync` (sætter `ReservationFailed`, **ikke** `Expired`) |
| Andet | Logger og ignorerer |

Lookup sker på `ProviderPaymentId` (svarer til `reference` i Vipps-callback).  
Ikke fundet → returnerer **200 OK** (så Vipps ikke retrier).  
Webhook-signatur valideres **ikke**.

**Vigtig regel:** Vipps/MobilePay webhook må aldrig sende eller frigive den samlede ordre til merchant. Webhooken må kun opdatere betalingsstatus og eventuelt sætte ordren til `ReadyToPay`, hvis alle deltagerbetalinger er `Reserved`. Den endelige merchant-callback må kun ske efter host-godkendelse og successful capture.

---

## Notifikationsregler

Alle notifikationer gemmes som `Message`-records i databasen — **ingen push, ingen email, ingen real-time**.

| Hændelse | Modtager | Beskedindhold |
|----------|----------|---------------|
| Ordre oprettet med merchant | Alle `OrderParticipants` inkl. host | Bestillingslink med `ParticipantToken` |
| Ordre oprettet uden merchant | Alle **inviterede** (ikke host) | Generel invitationstekst |
| Alle deltagere har reserveret betaling (`ReadyToPay`) | Host | "✅ Alle har bestilt og reserveret betaling til '{titel}'. Du kan nu godkende den samlede ordre: {link}" |
| Deltager betalt (legacy `PaymentService`) | Host (kun hvis betaler ≠ host) | "✅ {navn} har betalt {beløb} kr." |

### Meddelelses-regler

- Besked må ikke være tom eller whitespace.
- Ordre og deltager skal eksistere ved manuel oprettelse.
- `IsRead` er `false` ved oprettelse.
- `MarkAllReadAsync` sætter alle ulæste beskeder for en deltager til `IsRead = true` i én operation.
- Ulæst-tæller returnerer `Count(m => !m.IsRead)` for en deltagers beskeder.

---

## Sikkerheds-regler

| Endpoint-gruppe | Adgangskrav |
|----------------|-------------|
| `POST /api/auth/login` + `register` + `register-merchant` | Anonymous |
| `POST /api/auth/google-login` | Anonymous — validerer Google ID-token, ingen JWT påkrævet *(NYT)* |
| Alle `OrdersController`-endpoints | JWT påkrævet (`[Authorize]` på klassen) |
| `POST /api/merchant-orders` (InitOrder) | Anonymous (`[AllowAnonymous]` på action) |
| `GET /api/merchant-orders/by-order/{id}` | JWT påkrævet (klasse-level `[Authorize]`) |
| `POST /api/payments/webhooks/*` | Anonymous |
| `POST /api/payments/vipps/callbacks/*` | Anonymous |
| `ParticipantsController` | Ingen auth-attribut — effektivt anonymous |
| `FriendsController` | Ingen auth-attribut — effektivt anonymous |
| `MessagesController` | Ingen auth-attribut — effektivt anonymous |
| `DirectoryController` | Ingen auth-attribut — effektivt anonymous |
| `DevController` | Kun registreret i `Development`; findes ikke som route i andre environments |

**Udviklerruter** er deny-by-default. Hele `DevController` fjernes fra MVC discovery uden for det præcise environment `Development`, så ruterne returnerer 404 og ikke vises i Swagger. Der tilføjes ikke et produktions-adminpassword som alternativ adgang.

**Host-ejerskab** håndhæves ved at udlede `currentParticipantId` fra det validerede JWT-claim `NameIdentifier`/`sub` i controlleren og sammenligne det med `order.CreatedByParticipantId` i service-laget. Legacy-feltet `requestingParticipantId` i request-body ignoreres ved autorisation.

---

## Beløb-regler

| Felt | Type | Enhed |
|------|------|-------|
| `ParticipantPayment.AmountMinorUnits` | `long` | Øre (1 DKK = 100) |
| `Payment.Amount` | `decimal(18,2)` | Kroner |
| `MerchantOrderDraft.SubtotalAmount` | `decimal(18,2)` | Kroner |
| `MerchantOrderDraft.TotalAmount` | `decimal(18,2)` | Kroner |
| `MerchantOrderLine.UnitPrice` | `decimal(18,2)` | Kroner |
| `MerchantOrderLine.LineTotal` | `decimal(18,2)` | Kroner |

- `MerchantOrderService` konverterer: `amountMinorUnits = (long)(draft.TotalAmount * 100)`.
- Currency er `"DKK"` overalt som default.

---

## Open Questions

1. **`Declined`-status** — Defineret i frontend-enum og `participantStatusLabel()`, men ingen backend-service eller endpoint sætter denne status. Er det en planlagt feature?
2. **`Refunded`-status** — Defineret i `ParticipantPaymentStatus`-enum og tilladte transitions (`Captured → Refunded`), men ingen service-metode implementerer refundering. Er det planlagt?
3. **Vipps `CAPTURED` callback ignoreres** — `VippsCallbackController` logger kun ved `CAPTURED` og laver ingen state-ændring. Afhænger dette af at capture altid startes fra vores eget flow, så Vipps' bekræftelse er overflødig?
4. **FriendRelation-unikhed** — `FriendRelationRepository.RelationExistsAsync` tjekker for eksisterende relation og kaster ved duplikat. Men der er intet unikt DB-constraint — hvad sker der ved race conditions?
5. **`MerchantOrderDraft.Status` defaultværdi vs. faktisk tildelt værdi** — Entiteten har `Status = "Draft"` som default, men `MerchantOrderService` sætter `"Submitted"`. Er `MerchantOrderDraft.Status` aktivt brugt nogen steder?
6. **`CheckAndSetReadyToPayAsync` er effektivt ubrugt i produktionsflowet** — Metoden tjekker `OrderParticipant.Status == "OrderSubmitted"` og er tilgængelig via `IOrderService`, men kaldes ingen steder i produktionskode. `ReadyToPay` sættes i stedet via `CheckAndSetReadyToPayByReservedAsync` (Reserved-baseret). Bør metoden fjernes eller erstattes?
