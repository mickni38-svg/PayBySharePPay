# ShareBySharePay (SBYS) – Projektanalyse og dokumentation

## Dokumentformål
Dette dokument samler den samlede analyse, produktforståelse, problemformulering, arkitektur og anbefalede implementeringsretning for projektet **ShareBySharePay (SBYS)**.

Dokumentet er skrevet, så det kan læses af en udvikler eller AI-assistent som **Claude Sonnet 4.6 / GitHub Copilot**, og bruges som grundlag for videre implementering.

Fokus i denne version er en **web frontend**, som designes mobile-first og kan vises i mobil view. Backend antages stadig at være API-first med ASP.NET Core, EF Core og en relationel database.

---

# Agenda

1. Projektets overordnede idé
2. Problemformulering
3. Afgrænsning: take-away først
4. Centrale roller og begreber
5. Forretningsmodel
6. Brugerrejse – samlet flow
7. Merchant-flow – hvordan et spisested integreres
8. Gruppebetaling-flow step by step
9. Web frontend i mobil view
10. Datamodel og centrale entiteter
11. Integration mellem merchant og SBYS
12. Betalingsmodel: Model A
13. Sikkerhed: JWT, join tokens og HMAC
14. Funktionelle krav
15. MVP-afgrænsning
16. Status på eksisterende implementering
17. Anbefalet videre implementeringsplan
18. Pitch-manuskript
19. Konklusion

---

# 1. Projektets overordnede idé

**ShareBySharePay (SBYS)** er en platform til gruppebetaling ved take-away-bestillinger.

Formålet er at gøre det muligt for flere personer at bestille mad sammen, men betale hver deres del, uden at én person skal lægge hele beløbet ud.

SBYS fungerer ikke som en traditionel betalingsudbyder i MVP-versionen. I stedet fungerer SBYS som et **koordineringslag** mellem:

- brugere,
- venner/deltagere,
- spisesteder/merchants,
- merchantens eksisterende betalingssystem.

Kerneidéen er:

> Brugere bestiller sammen, betaler hver for sig, og spisestedet får først ordren frigivet, når alle deltagere har betalt eller reserveret deres del.

---

# 2. Problemformulering

Ved fælles take-away-bestillinger opstår der ofte friktion omkring betaling og koordinering. I dag er det typisk én person, der lægger hele beløbet ud og efterfølgende skal opkræve betaling fra de øvrige deltagere via MobilePay, bankoverførsel eller andre manuelle løsninger. Dette medfører risiko for fejl, manglende overblik, forsinkede betalinger og besværlig opfølgning.

Samtidig har take-away-spisesteder ikke en standardiseret måde at tilbyde gruppebetaling på i deres checkout-flow. Eksisterende checkout-løsninger er typisk designet til én kunde og én samlet betaling, ikke til en social gruppe hvor hver deltager betaler sin egen del.

Formålet med SBYS er at udvikle en platform, hvor registrerede spisesteder kan tilbyde gruppebetaling som en ekstra checkout-mulighed. En bruger kan oprette en gruppebetaling, invitere venner, og hver deltager kan via et personligt link bestille og betale sin del hos spisestedet. SBYS viser ordredata, deltagere og betalingsstatus, og ordren frigives først til produktion, når alle relevante deltagere har betalt eller reserveret deres del.

SBYS skal dermed reducere friktion for brugere, minimere risiko for udlæg og give spisesteder en ny, kontrolleret måde at modtage gruppeordrer på.

---

# 3. Afgrænsning: take-away først

Projektet starter med take-away-versionen.

Restaurant/POS-versionen, hvor en fysisk restaurant sender en kvittering fra kasseapparatet til en bordgruppe, er en senere udvidelse. Den version kræver sandsynligvis integration med POS-systemer, hvilket gør den mere kompleks.

Take-away-versionen vælges først, fordi:

- spisestedet allerede har et website eller checkout-flow,
- det er realistisk at tilføje en ekstra knap/link: “Gruppebetaling med SBYS”,
- der kræves ingen POS-integration,
- onboarding af merchants bliver lettere,
- MVP kan bygges hurtigere,
- værdien kan testes hurtigere i markedet.

---

# 4. Centrale roller og begreber

## User
En almindelig privat bruger af SBYS. Brugeren kan:

- oprette konto,
- tilføje venner,
- oprette gruppebetalinger,
- deltage i gruppeordrer,
- se status på bestillinger.

## Merchant
Et spisested eller take-away-sted, som er oprettet i SBYS.

Merchant er ikke en almindelig user, men kan vises i samme søge-/directory-liste som users. En bruger kan følge eller tilføje et spisested som favorit.

Merchant har tekniske integrationsfelter som website URL, group order URL, webhook URL, API key og signing secret.

## GroupPayment
En gruppebetaling oprettet i SBYS. Den indeholder:

- ejer/host,
- deltagere,
- valgt merchant,
- status,
- evt. tilknyttet ordre fra merchant.

## GroupPaymentMember
En deltager i gruppebetalingen.

Deltageren kan have status som:

- invited,
- accepted,
- order submitted,
- authorized,
- paid/captured,
- declined,
- expired.

## Merchant Order Draft
En ordrekladde hos merchant, som endnu ikke må sendes til produktion.

Ordren må først produceres, når SBYS har markeret gruppebetalingen som klar.

## Join Token
Et personligt token, som identificerer deltagerens invitation. Det bruges i URL’en til merchantens website.

Eksempel:

```text
https://pizzapalace.dk/order?sbysGroup=12345&sbysJoin=abc123
```

`sbysJoin` fortæller SBYS, hvilken deltager der bestiller.

---

# 5. Forretningsmodel

SBYS er en B2B2C-løsning.

## Privatbrugere
Brugere betaler ikke for at bruge appen/webappen.

Dette reducerer friktion og gør det lettere at få adoption.

## Merchants
Spisesteder betaler for at få adgang til SBYS og kunne tilbyde gruppebetaling.

Mulige prismodeller:

- fast månedligt abonnement,
- tier-baseret pris efter størrelse,
- fee pr. gennemført gruppebetaling,
- kombination af abonnement og transaktionsfee.

Eksempel:

- Small: lav pris for små take-away-steder,
- Medium: højere pris ved flere gruppebetalinger,
- Large: kæder og høj volumen.

Store restauranter/spisesteder bør betale mere end små, fordi de får mere værdi og højere volumen.

---

# 6. Brugerrejse – samlet flow

Det anbefalede flow er:

1. Brugeren logger ind på SBYS.
2. Brugeren finder et spisested i SBYS-directory.
3. Brugeren opretter en gruppebetaling hos dette spisested.
4. Brugeren inviterer venner fra sin venneliste.
5. Hver deltager modtager en notifikation i SBYS.
6. Notifikationen åbner merchantens website via et personligt SBYS-link.
7. Deltageren vælger mad fra menukortet på merchantens website.
8. Deltageren klikker “Betal som gruppe med SBYS”.
9. Merchant sender ordredata og betalingsstatus til SBYS.
10. SBYS viser deltagerens ordre og status i gruppebetalingen.
11. Når alle deltagere er authorized/paid, sender SBYS besked til merchant.
12. Merchant frigiver ordren og starter produktion.

---

# 7. Merchant-flow – hvordan et spisested integreres

Merchant skal først oprettes i SBYS.

Ved oprettelse gemmes blandt andet:

- navn,
- handle,
- website URL,
- group order URL,
- webhook URL,
- API key,
- signing secret,
- aktiv/inaktiv-status.

Merchant implementerer derefter en “Gruppebetaling med SBYS”-knap på sit website.

Når en deltager kommer ind via SBYS-linket, indeholder URL’en:

- groupPaymentId,
- joinToken.

Merchant behøver derfor ikke selv finde ud af, hvilken gruppebetalingen tilhører. Merchant skal blot videresende token og ordredata til SBYS ved checkout.

---

# 8. Gruppebetaling-flow step by step

## Step 1: Host vælger merchant
Host finder fx “Gasoline Grill” eller “Sticks & Sushi” i SBYS-directory.

## Step 2: Host opretter gruppebetaling
Host opretter en gruppebetaling og tilføjer sig selv samt to venner.

## Step 3: SBYS opretter invitationer
For hver deltager oprettes en personlig invitation med joinToken.

## Step 4: Deltager får notifikation
Eksempel:

> Du er inviteret til en gruppeordre hos Gasoline Grill. Tryk for at vælge din del.

## Step 5: Deltager åbner merchant website
SBYS åbner merchantens group order URL med query parameters:

```text
https://merchant.dk/order?sbysGroup=12345&sbysJoin=abc123
```

## Step 6: Deltager vælger mad
Deltageren vælger sin egen mad på merchantens website.

## Step 7: Deltager klikker “Betal som gruppe”
Merchant sender til SBYS:

- merchantId,
- groupPaymentId,
- joinToken,
- merchantOrderDraftId,
- ordrelinjer,
- totalbeløb,
- currency,
- payment status.

## Step 8: Betaling reserveres eller gennemføres
Merchant håndterer betaling via eget betalingssystem.

SBYS får status tilbage, fx:

- AUTHORIZED,
- PAID,
- FAILED,
- CANCELLED.

## Step 9: SBYS viser status
SBYS viser alle deltagere, deres ordre og betalingsstatus.

## Step 10: Alle har betalt
Når alle deltagere er klar, sender SBYS webhook til merchant:

```text
SBYS.GROUPCHECKOUT.ALL_AUTHORIZED
```

## Step 11: Merchant frigiver ordren
Merchant capturer evt. betalingerne og starter madproduktion.

---

# 9. Web frontend i mobil view

Selvom projektet tidligere har haft MAUI-app-fokus, skal denne dokumentation tage udgangspunkt i en **web frontend**, som fungerer godt i mobilvisning.

Det betyder:

- frontend bør være responsive/mobile-first,
- layout skal fungere i smal bredde,
- navigation bør ligne en mobilapp,
- dybe links bør være HTTPS-baserede,
- notifikationer kan i første omgang vises som in-app pending invitations,
- push-notifikationer kan implementeres senere.

## Anbefalet frontendstruktur

### Pages/views

1. Login/Register
2. Home/Dashboard
3. Find Users & Merchants
4. Merchant Detail
5. Create Group Payment
6. Group Payment Overview
7. Pending Invitations
8. Participant Order Status
9. Merchant onboarding/admin view

## Mobile-first UX-principper

- én primær handling pr. skærm,
- store touch targets,
- card-baseret layout,
- status-badges,
- klar deltagerliste,
- tydelig “mangler betaling”-status,
- tydelig “klar til produktion”-status.

---

# 10. Datamodel og centrale entiteter

## UserEntity
Repræsenterer almindelige brugere.

Typiske felter:

- Id,
- DisplayName,
- Handle.

## MerchantEntity
Repræsenterer spisestedet.

Typiske felter:

- Id,
- DisplayName,
- Handle,
- Description,
- LogoUrl,
- WebsiteUrl,
- GroupOrderUrl,
- ContactEmail,
- ContactPhone,
- AddressLine1,
- City,
- PostalCode,
- CountryCode,
- IsActive,
- CreatedAtUtc.

## MerchantIntegrationEntity
Tekniske integrationsdata.

Typiske felter:

- MerchantId,
- WebhookUrl,
- DefaultReturnUrl,
- AllowedOrigin,
- ApiKeyHash,
- SigningSecretHash,
- IsEnabled,
- UpdatedAtUtc.

## DirectoryConnectionEntity
Relation mellem en user og enten en anden user eller en merchant.

Typiske felter:

- Id,
- OwnerUserId,
- TargetType,
- TargetId,
- IsActive,
- CreatedAtUtc.

Dette gør det muligt at vise users og merchants i samme directory og lade brugeren tilføje/følge begge typer.

## GroupPaymentEntity
Selve gruppebetalingen.

Typiske felter:

- Id,
- CreatedByUserId,
- MerchantId,
- Title,
- Message,
- IconKey,
- Status,
- CreatedAtUtc,
- ExpiresAtUtc,
- IsActive.

## GroupPaymentMemberEntity
Deltager i gruppebetalingen.

Typiske felter:

- Id,
- GroupPaymentId,
- UserId,
- InvitationStatus,
- PaymentStatus,
- JoinTokenHash,
- CreatedAtUtc,
- AcceptedAtUtc,
- AuthorizedAtUtc,
- CapturedAtUtc.

## MerchantParticipantOrderEntity
Anbefalet ny entity til at gemme hver deltageres ordre.

Typiske felter:

- Id,
- GroupPaymentId,
- GroupPaymentMemberId,
- MerchantId,
- MerchantOrderDraftId,
- MerchantParticipantOrderId,
- Currency,
- SubtotalAmount,
- TotalAmount,
- Status,
- MerchantPaymentRef,
- CreatedAtUtc,
- UpdatedAtUtc.

## MerchantParticipantOrderLineEntity
Ordrelinjer for deltagerens bestilling.

Typiske felter:

- Id,
- ParticipantOrderId,
- LineId,
- Name,
- Quantity,
- UnitPrice,
- LineTotal.

---

# 11. Integration mellem merchant og SBYS

Merchant skal implementere en lille integration.

## Når deltager klikker “Betal som gruppe”
Merchant sender et init/status-kald til SBYS.

Payload bør indeholde:

- merchantId,
- groupPaymentId,
- joinToken,
- merchantOrderDraftId,
- merchantParticipantOrderId,
- currency,
- items,
- subtotalAmount,
- totalAmount,
- payment status,
- signature.

## Statusser

Participant payment status:

- DRAFT,
- SUBMITTED,
- AUTHORIZED,
- CAPTURED,
- FAILED,
- CANCELLED,
- EXPIRED.

Group payment status:

- CREATED,
- COLLECTING,
- ALL_AUTHORIZED,
- READY_TO_CAPTURE,
- COMPLETED,
- CANCELLED,
- EXPIRED.

---

# 12. Betalingsmodel: Model A

Projektet vælger **Model A**:

> Merchant bruger sit eget betalingssystem. SBYS håndterer kun koordinering, status og frigivelse.

Fordele:

- SBYS skal ikke være betalingsudbyder,
- lavere compliance-risiko,
- merchant beholder sit eksisterende PSP-setup,
- SBYS bliver PSP-agnostisk,
- lettere MVP.

SBYS skal ikke gemme merchantens Stripe/Nets/QuickPay credentials.

SBYS skal kun kende:

- merchantId,
- API key,
- signing secret,
- webhook URL,
- group order URL.

---

# 13. Sikkerhed: JWT, join tokens og HMAC

## JWT
JWT bør bruges til almindelige brugeres login.

Web frontend bør typisk bruge enten:

- HttpOnly cookies,
- eller access token i memory + refresh token-strategi.

Til en web frontend er HttpOnly secure cookies ofte sikrere end localStorage.

## Join tokens
`groupPaymentId` må ikke alene give adgang til en gruppebetaling.

Der skal bruges et join token.

Eksempel:

```text
https://merchant.dk/order?sbysGroup=12345&sbysJoin=random-token
```

Join token bør være:

- unikt,
- svært at gætte,
- tidsbegrænset,
- bundet til en bestemt deltager,
- gemt som hash i databasen.

## SHA / hashing
SHA er ikke kryptering. Det er hashing.

Man bør ikke “kryptere groupPaymentId” med SHA. I stedet bør man bruge random join tokens eller signerede tokens.

## HMAC
Merchant-serverkald til SBYS og SBYS-webhooks til merchant bør signeres med HMAC-SHA256.

Forslag til headers:

```text
X-SBYS-MerchantId: {merchantId}
X-SBYS-Timestamp: {unixSeconds}
X-SBYS-Signature: {hmac}
```

Signature bør beregnes over:

```text
timestamp + "." + rawBody
```

---

# 14. Funktionelle krav

## Bruger og login

- Brugere skal kunne oprette konto.
- Brugere skal kunne logge ind.
- Brugere skal kunne se egne gruppebetalinger.

## Directory

- Brugere skal kunne søge efter andre brugere.
- Brugere skal kunne søge efter merchants/spisesteder.
- Users og merchants skal kunne vises i samme liste.
- Merchants skal tydeligt vises som spisesteder.

## Venneliste/favoritter

- Brugere skal kunne tilføje andre brugere som venner.
- Brugere skal kunne følge eller favorisere merchants.
- Brugere skal ikke kunne tilføje sig selv.

## Gruppebetaling

- Brugere skal kunne oprette en gruppebetaling.
- Brugere skal kunne vælge merchant.
- Brugere skal kunne invitere venner.
- Deltagere skal modtage invitation.
- Deltagere skal kunne acceptere eller afvise.

## Merchant-order

- Merchant skal kunne sende fulde ordredata til SBYS.
- SBYS skal kunne gemme ordrelinjer.
- SBYS skal kunne vise alle deltageres ordrer.
- SBYS skal vise totalbeløb pr. deltager og samlet total.

## Betalingsstatus

- Merchant skal sende status pr. deltager.
- SBYS skal vise status pr. deltager.
- SBYS skal markere gruppen som klar, når alle er authorized/paid.

## Frigivelse af ordre

- Merchant må ikke producere mad før alle har betalt/reserveret.
- SBYS skal sende webhook til merchant, når gruppen er klar.

---

# 15. MVP-afgrænsning

MVP bør ikke indeholde:

- POS-integration,
- SBYS som betalingsudbyder,
- avanceret refund automation,
- real-time push via SignalR/WebSockets,
- komplet merchant-dashboard,
- mobile native app som eneste frontend,
- restaurant-bordbetaling.

MVP bør indeholde:

- web frontend i mobil view,
- brugere,
- venner,
- merchants,
- directory,
- opret gruppebetaling,
- invitationsflow,
- merchant link-flow,
- ordredata fra merchant,
- betalingsstatus fra merchant,
- samlet status i SBYS.

---

# 16. Status på eksisterende implementering

Der er allerede arbejdet med:

- user-liste,
- find personer,
- tilføj venner,
- opret gruppebetaling,
- tilføj personer fra vennelisten,
- overview-side,
- merchant entities,
- merchant integration entities,
- directory endpoint,
- directory DTO,
- visning af merchants sammen med users,
- seed af merchants:
  - Sticks & Sushi,
  - Gasoline Grill.

Der arbejdes fortsat på at adskille users og merchants korrekt, så merchants ikke bliver fake users, men stadig kan vises i samme UI.

---

# 17. Anbefalet videre implementeringsplan

## Fase 1: Directory og merchant selection

- færdiggør directory-listen,
- vis users og merchants i samme mobile-first web UI,
- tilføj merchant detail page,
- gør det muligt at vælge merchant ved oprettelse af gruppebetaling.

## Fase 2: GroupPayment + merchant binding

- tilføj MerchantId på GroupPayment,
- tilføj statusfelter,
- tilføj ExpiresAtUtc,
- opret invitationer med joinToken.

## Fase 3: Invitation flow

- vis pending invitations i web frontend,
- lav “åbn merchant website”-knap,
- generér URL med groupPaymentId og joinToken.

## Fase 4: Fake merchant demo

Lav en simpel fake pizza-webside, der kan:

- modtage sbysGroup og sbysJoin,
- vise et simpelt menukort,
- oprette en ordre,
- sende ordredata til SBYS,
- simulere AUTHORIZED/PAID.

Dette er den bedste måde at demonstrere hele flowet på uden rigtig merchant-integration.

## Fase 5: Merchant integration API

- endpoint til at modtage participant order,
- endpoint til payment status,
- HMAC validation,
- webhook fra SBYS til merchant.

## Fase 6: Auth/security

- JWT login,
- HttpOnly cookie eller token-strategi,
- joinToken hashing,
- HMAC for merchant calls.

---

# 18. Pitch-manuskript

## Åbning

Hvor mange gange har én person lagt ud for mad til hele gruppen og bagefter skulle jagte betalinger?

Det er en situation mange kender: én bestiller, én betaler, og resten skal huske at betale tilbage.

## Problem

Når man bestiller take-away sammen i dag, er betalingsflowet stadig designet til én person. Det skaber friktion, fejl og et unødvendigt ansvar for den person, der lægger ud.

Samtidig mangler spisesteder en simpel måde at tilbyde gruppebetaling uden at ændre hele deres betalingssystem.

## Løsning

ShareBySharePay gør gruppebetaling til en del af checkout-flowet.

Brugeren opretter en gruppebetaling, inviterer venner, og hver deltager bestiller og betaler sin egen del hos spisestedet.

SBYS holder styr på, hvem der har bestilt og betalt, og først når alle er klar, frigives ordren til spisestedet.

## Værdi for brugeren

Ingen skal lægge ud.
Ingen skal rykke venner.
Alle kan se status.

## Værdi for spisestedet

Spisestedet får større gruppeordrer, færre afbrudte køb og en bedre kundeoplevelse uden at skulle skifte betalingsudbyder.

## Forretningsmodel

Brugere anvender SBYS gratis.
Spisesteder betaler for adgang til SBYS som en B2B-service.

## Afslutning

SBYS er ikke endnu en betalingsapp. Det er et koordineringslag for gruppebetaling, der forbinder sociale relationer med eksisterende checkout-flow.

Kort sagt:

> Bestil sammen. Betal hver for sig. Ingen lægger ud.

---

# 19. Konklusion

SBYS løser et konkret og genkendeligt problem i take-away-markedet: gruppebestilling og individuel betaling uden udlæg.

Den valgte MVP-retning er realistisk, fordi den starter med take-away-websites frem for POS-integration. Ved at lade merchants beholde deres eksisterende betalingssystem reduceres teknisk kompleksitet og compliance-risiko.

Den anbefalede arkitektur er API-first med en mobile-first web frontend. Dette gør det muligt senere at tilføje native mobilapp, merchant dashboard, restaurant/POS-version og mere avancerede betalingsflows uden at ændre kernearkitekturen.

SBYS bør derfor bygges som en fleksibel platform, hvor users, merchants, gruppebetalinger, invitationer, ordredata og betalingsstatus er klart adskilte domæner.
