# PayNSync – Project Overview

> **Internal code name:** PayBySharePay  
> **Repository:** `PayBySharePPay` (branch: `main`)  
> **Runtime:** .NET 9 (backend), Angular 19 (frontend)  
> **Database:** SQL Server (EF Core)

---

## Hvad er PayNSync?

PayNSync er en **platform til gruppebetaling**. Den gør det muligt for en gruppe venner at bestille og betale individuelt til samme restaurant eller merchant – uden at nogen skal lægge ud for andre eller overføre penge efterfølgende.

Én person (Host/vært) opretter en gruppeordre, inviterer deltagere og vælger et spisested. Alle deltagere får et personligt link til spisestedets bestillingside, bestiller for sig selv, og betaler deres eget beløb via MobilePay/Vipps. Når alle har bestilt, godkender Host betalingen – og alle betalinger gennemføres i ét flow.

---

## Hvilket problem løser systemet?

Problemet opstår, når en gruppe mennesker bestiller mad eller drikke samlet, men ønsker at betale individuelt:

- Ingen skal lægge ud for andre og håbe på at få pengene igen
- Ingen manuel opdeling af regningen ("hvem havde hvad?")
- Ingen efterfølgende Mobilepay-overførsler
- Merchant og Host skal ikke håndtere kontanter eller individuelle kortbetalinger

PayNSync løser dette ved at lade hver deltager betale sit eget beløb direkte, koordineret som én samlet gruppeordre.

---

## Målgruppen

Koden afspejler to primære aktørtyper:

**Brugere (Person)**
- Grupper af venner, kollegaer eller familie der bestiller mad/drikke sammen
- Kræver app-login (email + password)
- Kan oprette ordrer (Host) eller deltage i andres ordrer (Participant)

**Merchants (Merchant)**
- Restauranter og spisestedet der ønsker at tilbyde gruppebetaling
- Registreres i systemet med CVR, firmanavn, betalingsreference og en `GroupOrderUrl` (link til egen bestillingsside)
- Modtager en HTTP callback når alle betalinger er gennemført

Der er desuden en demo-merchant (Pizzeria Roma) indbygget i løsningen til test og demonstration.

---

## Hovedfunktioner

### For brugere (Person)

| Funktion | Implementeret |
|----------|---------------|
| Opret konto og login (email + password) | ✅ |
| Login med Google (Google ID-token) | ✅ | *(NYT)* |
| Opret gruppeordre med titel, kategori og besked | ✅ |
| Vælg merchant (restaurant) til ordren | ✅ |
| Invitér venner som deltagere | ✅ |
| Modtag personligt bestillingslink via beskedindbakken | ✅ |
| Bestil via merchant's hjemmeside (med ParticipantToken) | ✅ |
| Se ordreoverblik med deltager- og betalingsstatus | ✅ |
| Host godkender og gennemfører alle betalinger | ✅ |
| Host annullerer ordre og reservationer | ✅ |
| Se afventende deltagere og send påmindelser | ✅ (reminder er frontend-placeholder) |
| Beskedindbakke med systemmeddelelser | ✅ |
| Venneliste og deltager-søgning | ✅ |

### For merchants

| Funktion | Implementeret |
|----------|---------------|
| Registrering som Merchant-deltager | ⚠️ Backend findes; nuværende frontend mangler påkrævet MSN og merchant-login |
| Modtag gruppebestillinger via anonym API | ✅ |
| Modtag HTTP callback ved afsluttet betaling | ✅ |
| Konfigurér egen bestillings-URL (`GroupOrderUrl`) | ✅ |

### Betalingsflow

| Funktion | Implementeret |
|----------|---------------|
| Reservation af betaling pr. deltager (MobilePay/Vipps) | ✅ |
| Capture (træk penge) af alle reservationer ved Host-godkendelse | ✅ |
| Annullering af reservationer | ✅ |
| Webhook-modtagelse fra Vipps MobilePay | ✅ |
| Fake betalingsudbyder til test og udvikling | ✅ |
| Audit trail (PaymentEventLog) for alle statusskift | ✅ |

---

## Besluttet betalingsmodel for PayNSync v1

PayNSync v1 bruger en hybridmodel:

> **Merchant ejer menu, kurv og ordrelinjer. PayNSync ejer gruppeflow, MobilePay/Vipps-reservation, betalingsstatus, capture og endelig ordreaccept.**

Merchant skal derfor **ikke** selv oprette eller capture MobilePay/Vipps-betalinger i v1. Merchant sender kun deltagerens ordrelinjer og totalbeløb til PayNSync sammen med `orderId` og `participantToken`. PayNSync opretter herefter betalingsreservationen hos Vipps/MobilePay.

En deltager skal altid selv godkende reservationen i sin MobilePay-app. PayNSync må ikke bruge deltagerens telefonnummer eller MobilePay-id til senere at betale på deltagerens vegne uden denne godkendelse.

Vigtigt:

- Knappen hos merchant bør ikke hedde **"Betal"**, fordi beløbet ikke trækkes endeligt på merchant-siden.
- Knappen bør hedde fx **"Bekræft min ordre"** eller **"Gem ordre og reservér betaling"**.
- Efter klik sendes deltageren videre til MobilePay/Vipps for at godkende reservationen.
- Et MobilePay-swipe betyder kun **reservation godkendt**.
- Merchant må **ikke** modtage/frigive den samlede ordre, bare fordi én deltager har swipet.
- Merchant får først endelig ordreaccept, når alle deltagere har reserveret, host har godkendt, og PayNSync har captured alle betalinger.

---

## PayNSync Merchant Integration Contract v1


## Præcisering: Merchant Demo starter MobilePay-flowet via PayNSync

Merchant Demo skal **ikke** selv implementere Vipps/MobilePay API-kald direkte og må ikke indeholde API-nøgler, client secret, subscription key eller access token i frontend-kode.

Den korrekte rollefordeling er:

```text
Merchant Demo = UI, kurv og ordrelinjer
PayNSync API = opretter Vipps/MobilePay payment/reservation
Vipps/MobilePay = deltagerens swipe/godkendelse
PayNSync API = modtager webhook, opdaterer status og capturer senere
```

Flowet er:

1. Deltager vælger mad på Merchant Demo.
2. Deltager klikker fx **"Bekræft ordre og reservér med MobilePay"**.
3. Merchant Demo kalder `POST /api/merchant-orders` på PayNSync API med ordrelinjer, totalbeløb og `participantToken`.
4. PayNSync backend gemmer draft-ordren og opretter en Vipps/MobilePay reservation via ePayment API.
5. PayNSync returnerer en `redirectUrl` til Merchant Demo.
6. Merchant Demo redirecter browseren til `redirectUrl`.
7. Deltager swiper/godkender i MobilePay/Vipps test app.
8. Vipps/MobilePay sender webhook til PayNSync.
9. PayNSync markerer deltagerens `ParticipantPayment` som `Reserved`.
10. PayNSync venter på de øvrige deltagere.
11. Når alle er `Reserved`, kan host godkende, hvorefter PayNSync capturer alle betalinger én efter én.

Vigtigt:

- Selve swipet sker **altid** i MobilePay/Vipps app/test flow.
- Merchant Demo “aktiverer” kun flowet ved at bede PayNSync backend starte reservationen.
- PayNSync backend laver ikke swipe for brugeren.
- Telefonnummer kan bruges ved oprettelse/test af payment, men capture sker senere via `ProviderPaymentId` / Vipps reference.
- Merchant får først final group order efter successful capture af alle deltagerbetalinger.


PayNSync v1 definerer én standardiseret **Group Order API Contract**, som merchants skal tilpasse sig. PayNSync forsøger ikke i v1 at bygge en specialtilpasset JSON-payload til hver enkelt merchant eller POS-model.

Merchant skal i v1 kunne to ting:

1. **Sende deltagerens draft-ordre til PayNSync**  
   Merchant sender ordrelinjer, totalbeløb, currency, `orderId`, `participantToken` og eventuelt egen `merchantDraftId`. PayNSync gemmer både normaliserede ordrelinjer og kan gemme merchantens originale JSON som `RawMerchantPayloadJson`.

2. **Modtage én final group order fra PayNSync**  
   Når alle deltagerbetalinger er `Captured`, sender PayNSync én samlet `GroupOrderPaid` payload til merchantens `GroupOrderUrl`. Payloaden indeholder gruppeordre-id, totalbeløb, currency, betalingsstatus og deltagergrupperede ordrelinjer.

V1-princippet er derfor:

> **PayNSync definerer gruppeordre-formatet. Merchant mapper PayNSync-formatet til sit eget ordre-/POS-system.**

Merchant behøver ikke ændre hele sit normale ordreflow, men skal have et endpoint der kan modtage PayNSyncs final group order. Senere kan PayNSync udvides med merchant-specifikke adapters for større merchants, men det er ikke en del af v1.


## Overordnet vision (aflæst fra koden)

Koden afspejler en ambition om et fuldt integreret betalingsflow, hvor:

1. **Merchant-integration er førsteklasses** — merchants har deres egen URL, branding og modtager strukturerede callbacks. Systemet er designet til at fungere med rigtige restauranters egne hjemmesider, ikke kun demo-siden.
2. **PayNSync er betalingsorkestrator** — PayNSync opretter reservationer, holder status og capturer betalinger samlet efter host-godkendelse. Merchant ejer ikke MobilePay-flowet i v1.
3. **Betalingsudbydere er udskiftelige** — `IPaymentProvider`-abstraktionen og den konfigurerbare `Payments:Provider`-switch viser, at MobilePay/Vipps kan erstattes eller suppleres af andre udbydere.
4. **Deltageren ejer sin bestilling og sin betalingsgodkendelse** — hver deltager bestiller for sig selv hos merchant og skal selv swipe/godkende sin reservation i MobilePay. ParticipantToken-modellen sikrer, at bestillingen knyttes til den rigtige person i den rigtige ordre.
5. **Host har kontrol over frigivelse** — Host opretter, godkender og kan annullere. En individuel MobilePay-godkendelse frigiver aldrig merchant-ordren alene.
6. **Merchant får først endelig ordre efter samlet capture** — restauranten må først lave/frigive ordren, når alle deltagerbetalinger er captured og PayNSync har sendt `Paid` callback.
7. **Mobil-first** — frontenden er bygget som en responsiv SPA med mobil-first design, navigation via bottom-nav og kortbaseret layout.

---

## Deployed Environments

| Komponent | URL | Hosting |
|-----------|-----|---------|
| API | `https://api.paynsync.dk` | Simply.com (Windows hosting, IIS in-process) |
| Frontend (Angular SPA) | `https://mobil.paynsync.dk` | Simply.com |
| Landing page | `https://paynsync.dk` | Simply.com |
| Merchant Demo (Pizzeria Roma) | `https://merchant.paynsync.dk` | Simply.com |

---

## Projects in Solution

> Bemærk: Kun .NET-projekter er registreret i `PayBySharePay.sln`. Frontend-projekterne er **ikke** en del af Visual Studio-løsningsfilen — de er selvstændige mapper ved siden af `.sln`.

| Projekt | Type | Rolle |
|---------|------|---------|
| `Api.PayBySharePay` | ASP.NET Core Web API (.NET 9) | HTTP-indgang — controllers, auth, middleware |
| `Service.PayBySharePay` | Class library | Forretningslogik, orkestrering, DTOs, interfaces |
| `DataStorage.PayBySharePay` | Class library | EF Core entities, DbContext, repositories, migrationer |
| `Infrastructure.Payments.PayBySharePay` | Class library | Betalingsudbyder-implementeringer (Fake + Vipps MobilePay) |
| `Tools.PayBySharePay` | Console app | Udviklerværktøj / seed-scripts |
| `Tests.PayBySharePay` | xUnit test project | Enhedstests (orkestrering, state machine, fake provider) |
| `Frontend.PayBySharePay` | Angular SPA *(ikke i .sln)* | Brugervendt mobil-first webapp |
| `Frontend.MerchantDemo` | Statisk HTML/JS *(ikke i .sln)* | Simuleret merchant-bestillingsside (Pizzeria Roma) |

---

## Key External Dependencies

- **Vipps MobilePay ePayment API** — `https://apitest.vipps.no/epayment/v1/payments` (sandbox)
- **SQL Server** — lokal SQLEXPRESS (dev), Simply.com MSSQL (prod via `SIMPLY_DB_CONNECTION_STRING` secret)
- **Simply.com** — Windows hosting (FTP-deploy via GitHub Actions `deploy-simply.yml`)

---

## Tech Stack Summary

**Backend:** ASP.NET Core 9, Entity Framework Core, JWT Bearer auth, Swagger/OpenAPI  
**Frontend:** Angular 19 (standalone components, signals, lazy-loaded routes, mobil-first)  
**Payments:** Vipps MobilePay ePayment API (OAuth2 client credentials)  
**Auth:** JWT HS256. `JwtTokenService` læser `Jwt:ExpiresInMinutes` fra config (default fallback `480`, `appsettings.json` sætter `43200` = 30 dage). Token-levetiden er dermed **43200 min (30 dage)**. `AuthController` returnerer dog `ExpiresAt = now + 480 min` hardkodet i response-body — dette er en fejl (se Open Questions #3). Google login implementeret via `ExternalAuthService` + `Google.Apis.Auth`; `POST /api/auth/google-login` validerer Google ID-token og returnerer JWT. *(NYT: Google login)*
**Tests:** xUnit, FluentAssertions, in-memory fakes (ingen EF InMemory)

---

## Open Questions

1. **Merchant-kontooplevelse** — UC-15 fastlægger, at merchants skal kunne registrere sig og logge ind i kontocenteret. Et egentligt merchant-dashboard, ordrebehandling og POS-administration er fortsat uafklaret/uden for UC-15.
2. **Vision for deltagerbetaling** — Koden returnerer en `redirectUrl` fra reserve-kaldet (til MobilePay-app). Det er uklart om slutbrugeren skal omdirigeres automatisk, eller om dette kun bruges i testscenarier.
3. **JWT `ExpiresAt` i response-body** — `JwtTokenService` udsteder et token med `Jwt:ExpiresInMinutes = 43200` (30 dage). `AuthController` returnerer dog `ExpiresAt = DateTime.UtcNow.AddMinutes(480)` hardkodet i alle tre auth-svar (login, register, register-merchant). Klienten tror dermed at sessionen udløber om 8 timer, selvom tokenet er gyldigt i 30 dage.
4. **Skalering til andre merchanttyper** — Systemet er designet til restaurant-bestillinger (ordrelinjer, menupunkter). Det er ikke tydeligt fra koden, om visionen omfatter andre merchanttyper (fx events, transport, tjenester).
