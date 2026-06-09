# Business Rules

Regler udledt direkte fra kode (services, controllers, entities, state machine).  
Organiseret efter aktør og domæneområde.

---

## Deltagere (Participants)

### Typer og oprettelse

- En `Participant` er enten `Person` eller `Merchant` (`ParticipantType` enum — enkelt tabel, single-table inheritance).
- En **Person** kræver et navn (`Name` må ikke være tomt eller whitespace).
- En **Merchant** kræver et firmanavn (`CompanyName` må ikke være tomt eller whitespace).
- Password hashes med BCrypt ved oprettelse. Hvis password er tomt/null, gemmes `PasswordHash = null`.
- Email er ikke påkrævet ved oprettelse, men bruges som login-identifikator og til unikhedstjek ved registrering.

### Login

- Login sker på email — systemet finder den første `Person` med matchende email (case-insensitiv).
- Hvis `PasswordHash` er sat og et password er angivet, skal BCrypt-verifikation bestå.
- Hvis `PasswordHash` er null, tillades login **uden** password (legacy seed-brugere).
- Vellykkede logins returnerer JWT med `sub` = `participantId`, `name` = navn, `jti` = nyt GUID, udløb = konfigureret i `Jwt:ExpiresInMinutes` (default 480 min).

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
- Host-tjek sker i **service-laget** ved sammenligning: `requestingParticipantId == order.CreatedByParticipantId`. Tjekket er **ikke** baseret på JWT-claims.
- Hvis host-tjek fejler, kastes `UnauthorizedAccessException` (resulterer i HTTP 500 pga. manglende middleware-mapping — se Open Questions).

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
  └─► (alle ikke-merchant deltagere har status OrderSubmitted)
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

- Ordren kan **ikke** annulleres hvis status er `Paid`.
- Idempotent: allerede `Paid` returnerer success uden fejl fra `/approve`.
- Idempotent: allerede `Cancelled` returnerer success uden fejl fra `/cancel`.

---

## Merchant-regler

### Merchant-draft (bestilling)

- `POST /api/merchant-orders` er `[AllowAnonymous]` — kræver ingen JWT.
- `ParticipantToken` i requesten valideres mod `OrderParticipant.ParticipantToken` for den givne ordre. Ugyldigt token → `UnauthorizedAccessException`.
- En `Merchant`-type deltager kan **ikke** indsende en bestillingsdraft (kun `Person` kan).
- **Én draft pr. deltager pr. ordre** — gen-indsendelse sletter den forrige draft og opretter en ny.
- Alle ordrelinjer i en draft tildeles `ParticipantId` fra den fundne `OrderParticipant`.
- Draft-status sættes til `"Submitted"` (ikke `"Draft"`, som er default-værdien i entiteten).
- Betalingsreservation startes **automatisk** ved draft-indsendelse (`MerchantOrderService` kalder `ReserveParticipantPaymentAsync`).
- `AmountMinorUnits` beregnes som `(long)(draft.TotalAmount * 100)`.

### Callback til merchant

- Når alle betalinger er captured, sender `MerchantCallbackService` en HTTP POST til `Merchant.GroupOrderUrl`.
- Payload indeholder `orderId`, `merchantId`, `status: "Paid"` og liste af deltagerresultater.
- Callback-fejl stopper **ikke** flowet — betalingerne er allerede gennemført.
- Hvis `Merchant.GroupOrderUrl` er null/tom, springes callback over.

---

## Betalingsregler

### ParticipantPayment (provider-backed)

- Én `ParticipantPayment` pr. deltager pr. ordre.
- `AmountMinorUnits` gemmes som `long` (øre/cents).
- Currency defaults til `"DKK"`.
- `RowVersion` (byte array) er konfigureret som EF Core optimistisk concurrency-token.

### Reserve-regler

- **Idempotens:** hvis en ikke-cancelled, ikke-failed `ParticipantPayment` allerede eksisterer for samme deltager+ordre, returneres den eksisterende uden nyt provider-kald.
- Reservationsflow:
  1. Opret `ParticipantPayment` (status `Created`)
  2. Sæt temp `ProviderPaymentId = "pending-{id}"`, status → `ReservationStarted`
  3. Kald `IPaymentProvider.ReserveAsync`
  4. Success: opdatér `ProviderPaymentId` med rigtigt provider-ID
  5. Fake provider: sæt status → `Reserved` synkront
  6. Rigtig provider: status forbliver `ReservationStarted` indtil webhook
  7. Fejl/exception: status → `ReservationFailed`

### Capture-regler

- Kun host kan godkende (`requestingParticipantId == order.CreatedByParticipantId`).
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
| `"AUTHORIZED"` eller `"RESERVE"` | → `SetReservedAsync` |
| `"CAPTURED"` | Ingen state-ændring (capture sker via vores eget flow) |
| `"CANCELLED"` eller `"ABORTED"` | → `SetCancelledAsync` |
| `"TERMINATED"` eller `"EXPIRED"` | → `SetReservationFailedAsync` |
| Andet | Logger og ignorerer |

Lookup sker på `ProviderPaymentId` (svarer til `reference` i Vipps-callback).  
Ikke fundet → returnerer **200 OK** (så Vipps ikke retrier).  
Webhook-signatur valideres **ikke**.

---

## Notifikationsregler

Alle notifikationer gemmes som `Message`-records i databasen — **ingen push, ingen email, ingen real-time**.

| Hændelse | Modtager | Beskedindhold |
|----------|----------|---------------|
| Ordre oprettet med merchant | Alle `OrderParticipants` inkl. host | Bestillingslink med `ParticipantToken` |
| Ordre oprettet uden merchant | Alle **inviterede** (ikke host) | Generel invitationstekst |
| Alle deltagere har bestilt (`ReadyToPay`) | Host | "Alle har bestilt. Du kan nu gennemføre betalingen: {link}" |
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
| `POST /api/auth/login` + `register` | Anonymous |
| Alle `OrdersController`-endpoints | JWT påkrævet (`[Authorize]` på klassen) |
| `POST /api/merchant-orders` (InitOrder) | Anonymous (`[AllowAnonymous]` på action) |
| `GET /api/merchant-orders/by-order/{id}` | JWT påkrævet (klasse-level `[Authorize]`) |
| `POST /api/payments/webhooks/*` | Anonymous |
| `POST /api/payments/vipps/callbacks/*` | Anonymous |
| `ParticipantsController` | Ingen auth-attribut — effektivt anonymous |
| `FriendsController` | Ingen auth-attribut — effektivt anonymous |
| `MessagesController` | Ingen auth-attribut — effektivt anonymous |
| `DirectoryController` | Ingen auth-attribut — effektivt anonymous |
| `DevController` | Ingen auth-attribut — effektivt anonymous |

**Host-ejerskab** håndhæves i service-laget ved at sammenligne `requestingParticipantId` med `order.CreatedByParticipantId`. Det er klienten, der sender `requestingParticipantId` i request-body — det valideres **ikke** mod JWT-claimet `sub`.

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

1. **Host-tjek returnerer HTTP 500** — `UnauthorizedAccessException` er ikke mappet i `ExceptionHandlingMiddleware`. En ikke-host der kalder `/approve` eller `/cancel` får HTTP 500 i stedet for 403. Er dette bevidst?
2. **`requestingParticipantId` vs. JWT `sub`** — Host-tjek baserer sig på hvad klienten sender i body. En ondsindet klient kan sende en anden brugers ID. Er dette en sikkerhedsrisiko der skal adresseres?
3. **`DevController` uden auth** — `DELETE /api/dev/reset` sletter alle ordrer og er tilgængeligt uden authentication i produktion. Bevidst valg?
4. **`Declined`-status** — Defineret i frontend-enum og `participantStatusLabel()`, men ingen backend-service eller endpoint sætter denne status. Er det en planlagt feature?
5. **`Refunded`-status** — Defineret i `ParticipantPaymentStatus`-enum og tilladte transitions (`Captured → Refunded`), men ingen service-metode implementerer refundering. Er det planlagt?
6. **Vipps `CAPTURED` callback ignoreres** — `VippsCallbackController` logger kun ved `CAPTURED` og laver ingen state-ændring. Afhænger dette af at capture altid startes fra vores eget flow, så Vipps' bekræftelse er overflødig?
7. **FriendRelation-unikhed** — `FriendRelationRepository.RelationExistsAsync` tjekker for eksisterende relation og kaster ved duplikat. Men der er intet unikt DB-constraint — hvad sker der ved race conditions?
8. **`MerchantOrderDraft.Status` defaultværdi vs. faktisk tildelt værdi** — Entiteten har `Status = "Draft"` som default, men `MerchantOrderService` sætter `"Submitted"`. `CheckAndSetReadyToPayAsync` tjekker `OrderParticipant.Status == "OrderSubmitted"`, ikke `MerchantOrderDraft.Status`. Er draft-status aktivt brugt?
