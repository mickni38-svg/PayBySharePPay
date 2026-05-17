# Copilot Guide: Refactor “Opret ordre” til step-by-step flow

## Formål

Denne guide skal bruges af GitHub Copilot / Claude Sonnet 4.6 til at implementere en bedre UX på siden **Opret ordre** i PayBySharePay.

Den nuværende **Opret ordre** side er for lang, tung og visuelt rodet på mobil. Den skal ændres til et mere professionelt **trin-for-trin flow**, hvor brugeren indtaster lidt information ad gangen og går videre til næste trin.

**Vigtigt:** Farver, mørkt tema, logo, kort-design, navigation og generel visuel stil fungerer godt og må ikke redesignes. Opgaven handler primært om layout, flow og brugervenlighed på opret-siden.

---

## Før du starter

Læs først projektets dokumentation grundigt.

Start med:

```text
README.md
```

Læs derefter alle underliggende links fra README, især:

```text
docs/01-overblik.md
docs/02-arkitektur.md
docs/03-projektstruktur.md
docs/04-backend.md
docs/05-frontend.md
docs/06-api-endpoints.md
docs/07-database.md
docs/08-authentication-security.md
docs/09-konfiguration.md
docs/10-lokal-udvikling.md
docs/11-azure-deployment-prod.md
docs/12-test-og-kvalitet.md
docs/13-fejlfinding.md
docs/14-vigtige-kode-links.md
docs/15-screenshots.md
```

Når dokumentationen er læst, skal du finde den eksisterende Angular-kode for **Opret ordre / Opret gruppebetaling**.

Typiske steder at undersøge:

```text
src/Frontend.PayBySharePay
src/Frontend.PayBySharePay/src/app
src/Frontend.PayBySharePay/src/app/features
src/Frontend.PayBySharePay/src/app/pages
src/Frontend.PayBySharePay/src/app/components
src/Frontend.PayBySharePay/src/app/services
```

Du skal ikke gætte filnavne. Find de faktiske filer i repository.

---

## Visuel reference

Brug disse screenshots som reference for den nuværende app-stil.

### Forside

```text
forside.png
```

Forsiden har det rigtige visuelle niveau:

- mørkt tema
- tydeligt logo
- gode cards
- god spacing
- god bundnavigation
- professionel mobiloplevelse

### Beskeder

```text
beskeder.png
```

Besked-siden viser også et acceptabelt kort-layout, som passer til appens stil.

### Overblik

```text
overblik.png
```

Overblik-siden har godt mørkt layout, cards og statusvisning. Bevar denne retning.

### Nuværende Opret ordre

```text
opretordre.png
```

Dette screenshot viser problemet:

- siden er for lang
- der er for mange inputområder på én gang
- brugeren skal scrolle for meget
- “Opret gruppebetaling”-knappen ligger for langt nede
- siden føles mere som en formular end et moderne app-flow
- deltagerlisten fylder for meget på første skærm
- det er uklart hvor langt man er i processen

---

## Overordnet UX-mål

Ændr **Opret ordre** fra én lang formular til et simpelt wizard-flow.

Foreslået flow:

```text
Trin 1: Grundinfo
Trin 2: Spisested / merchant
Trin 3: Deltagere
Trin 4: Gennemse og opret
```

Brugeren skal kunne gå frem og tilbage mellem trin.

Der skal være en tydelig step-indikator øverst på siden, fx:

```text
1 af 4 · Grundinfo
```

eller:

```text
Grundinfo → Spisested → Deltagere → Gennemse
```

Den præcise visuelle udformning skal passe til den eksisterende app-stil.

---

## Designregler

Du må gerne ændre layoutet på Opret ordre-siden, men du må ikke ændre appens generelle design-identitet.

Bevar:

- mørk baggrund
- eksisterende farvepalette
- eksisterende logo/topområde
- eksisterende bundnavigation
- eksisterende typografi så vidt muligt
- eksisterende card-stil
- eksisterende border radius
- eksisterende ikoner/emoji-kategorier, hvis de allerede bruges
- eksisterende grøn/lilla/blå accent-stil
- eksisterende knap-stil, men gerne med bedre placering

Undgå:

- nyt farvetema
- store designændringer på andre sider
- ændringer i Forside, Overblik, Brugere eller Beskeder, medmindre det er absolut nødvendigt
- ændringer i backend/API, medmindre eksisterende frontend-flow kræver mindre tilpasning
- ændringer i datamodel eller database

---

## Funktionelle krav

### Krav 1: Opret ordre skal være opdelt i trin

Implementér et step-flow/wizard på Opret ordre-siden.

Minimum trin:

1. **Grundinfo**
2. **Spisested**
3. **Deltagere**
4. **Gennemse og opret**

Brugeren skal kunne:

- trykke **Næste**
- trykke **Tilbage**
- se hvilket trin han/hun er på
- ikke oprette ordren før sidste trin
- få validering pr. trin

---

## Trin 1: Grundinfo

Dette trin skal indeholde:

- Titel
- Kategori
- Besked, valgfri

Eksempel på felter:

```text
Titel
fx pizza aften

Kategori
🍕 🍔 🍺 🌮 osv.

Besked, valgfri
Skriv en kort besked til deltagerne...
```

### Validering

Brugeren må ikke gå videre, før:

- titel er udfyldt
- kategori er valgt, hvis kategori er obligatorisk i eksisterende model

Hvis kategori ikke er obligatorisk i den eksisterende kode, skal den stadig gerne kunne vælges, men må ikke blokere flowet.

### UI

Felterne skal ligge i et card eller et tydeligt afsnit.

CTA nederst:

```text
Næste
```

---

## Trin 2: Spisested / merchant

Dette trin skal indeholde valg af spisested/merchant.

Hvis eksisterende kode allerede har logic for spisesteder, skal den genbruges.

Vis fx:

```text
Vælg spisested
Ingen spisesteder i din venneliste
```

Hvis der ikke findes spisesteder/merchants, skal brugeren have en pæn tom-tilstand.

Eksempel:

```text
Ingen spisesteder endnu
Du kan stadig fortsætte uden spisested, hvis løsningen tillader det.
```

eller, hvis merchant er obligatorisk:

```text
Du skal vælge et spisested for at fortsætte.
```

### Validering

Følg eksisterende forretningsregler:

- Hvis merchant/spisested er obligatorisk i den eksisterende oprettelse, så må brugeren ikke gå videre uden valg.
- Hvis merchant/spisested ikke er obligatorisk, så skal brugeren kunne fortsætte.

### CTA

```text
Tilbage
Næste
```

---

## Trin 3: Deltagere

Dette trin skal fokusere kun på deltagerne.

Indhold:

- Søgefelt
- Liste over deltagere
- Valgte deltagere
- Tydelig visning af antal valgte

Eksempel:

```text
Tilføj deltagere

Søg navn...

Valgte deltagere: 3
Anders Nielsen
Søren Jensen
Lene Hansen
```

### Vigtig UX-ændring

Deltagerlisten må gerne være scrollable inde i sit eget område, men hele siden skal ikke føles lang og uoverskuelig.

Brug eventuelt:

- kompakte deltager-rækker
- sticky knapområde nederst
- selected chips øverst
- “Vis valgte” sektion

### Validering

Følg eksisterende regler:

- Hvis mindst én deltager er påkrævet, må brugeren ikke gå videre uden deltager.
- Hvis brugeren selv automatisk tæller som deltager/vært i eksisterende kode, må denne logik ikke brydes.

### CTA

```text
Tilbage
Næste
```

---

## Trin 4: Gennemse og opret

Sidste trin skal give brugeren en samlet opsummering før oprettelse.

Vis:

- Titel
- Kategori
- Besked, hvis udfyldt
- Spisested/merchant
- Antal deltagere
- Deltagerliste
- Eventuelt vært/ejer
- Eventuelle advarsler

Eksempel:

```text
Gennemse gruppebetaling

Titel
Fredagspizza med venner

Kategori
🍕

Spisested
Pizzeria Roma ApS

Deltagere
4 deltagere

Besked
Vi bestiller kl. 18
```

CTA:

```text
Tilbage
Opret gruppebetaling
```

Knappen **Opret gruppebetaling** skal kun findes på sidste trin.

Efter oprettelse skal eksisterende navigation/redirect bevares, medmindre dokumentationen eller eksisterende kode siger noget andet.

---

## Navigation og knapper

Der skal være et knapområde nederst på Opret ordre-siden.

På mobil må knapperne gerne være sticky nederst over bundnavigationen, så brugeren ikke skal scrolle for at komme videre.

Eksempel:

```text
[ Tilbage ] [ Næste ]
```

På trin 1:

```text
[ Næste ]
```

På sidste trin:

```text
[ Tilbage ] [ Opret gruppebetaling ]
```

Bevar den eksisterende bundnavigation nederst i appen.

---

## State management

Brug eksisterende Angular-patterns i projektet.

Du skal undersøge om projektet bruger:

- standalone components
- Angular services
- reactive forms
- template-driven forms
- signals
- RxJS
- route-based state
- local component state

Implementér wizard-state på den måde, der passer bedst til den eksisterende kode.

State skal som minimum holde:

```text
currentStep
title
category
message
selectedMerchant
selectedParticipants
validationErrors
isSubmitting
```

Brug eksisterende modeller/interfaces, hvis de findes.

---

## Data og API

Du skal genbruge eksisterende API-kald og services.

Find eksisterende metode til at oprette gruppebetaling/order.

Eksempler på ting du skal lede efter:

```text
OrderService
OrdersService
CreateOrder
CreateGroupPayment
PaymentService
MerchantService
UserService
ParticipantService
```

Du må ikke ændre API-kontrakten, medmindre det er strengt nødvendigt.

Hvis API’et allerede forventer ét samlet request object, så skal frontend stadig samle alle trin til ét request på sidste trin.

Eksempel:

```ts
const request = {
  title: form.title,
  category: form.category,
  message: form.message,
  merchantId: selectedMerchant?.id,
  participantIds: selectedParticipants.map(x => x.id)
};
```

Tilpas til den faktiske model i koden.

---

## Validering

Valider kun det aktuelle trin, når brugeren klikker **Næste**.

Eksempel:

- På trin 1 valideres titel/kategori.
- På trin 2 valideres merchant/spisested, hvis påkrævet.
- På trin 3 valideres deltagere.
- På trin 4 valideres hele requesten før submit.

Vis fejl på en rolig måde, der matcher appens stil.

Eksempel:

```text
Titel skal udfyldes
Vælg mindst én deltager
```

Undgå browser-alerts.

---

## Loading og fejl

Når brugeren klikker **Opret gruppebetaling**:

- disable knappen
- vis loading-tekst eller spinner
- undgå dobbelt-submit
- vis fejlbesked, hvis API-kald fejler
- bevar brugerens indtastede data ved fejl

Eksempel:

```text
Opretter...
```

Ved fejl:

```text
Ordren kunne ikke oprettes. Prøv igen.
```

Hvis backend returnerer en konkret fejlbesked, må den gerne vises i brugervenlig form.

---

## Responsivt design

Siden skal optimeres til mobil først.

Test minimum:

- smal mobilbredde som screenshots
- almindelig desktop-browser
- ingen vandret scroll
- bundnavigation må ikke dække vigtige knapper
- indhold skal kunne scrolles, men hvert trin skal føles kort og fokuseret

---

## Acceptkriterier

Opgaven er færdig, når følgende er opfyldt:

- Opret ordre-siden er ikke længere én lang formular.
- Opret ordre er opdelt i tydelige trin.
- Brugeren kan gå frem og tilbage.
- Der findes step-indikator.
- “Opret gruppebetaling” vises kun på sidste trin.
- Farver og overordnet design er bevaret.
- Forside, Overblik, Brugere og Beskeder er ikke redesignet.
- Eksisterende oprettelsesfunktion virker stadig.
- Eksisterende API-kontrakt er bevaret.
- Validering virker pr. trin.
- Loading-state virker ved submit.
- Fejl vises brugervenligt.
- Layout virker på mobil.
- Der er ikke introduceret TypeScript/build-fejl.
- Relevante tests er opdateret eller tilføjet, hvis projektet har test setup.
- Dokumentation er opdateret, hvis relevant.

---

## Implementeringsplan

### Step 1: Find eksisterende Opret ordre kode

Find component/page for Opret ordre.

Dokumentér kort i din slutrapport:

- hvilke filer du fandt
- hvilken component der styrer siden
- hvilke services den bruger
- hvilken API-metode der opretter ordren

---

### Step 2: Forstå eksisterende form og request model

Undersøg hvordan den eksisterende side bygger requestet.

Find:

- felter
- models/interfaces
- validation
- submit-metode
- redirect/navigation efter oprettelse
- API service method

Du må ikke ændre request model unødvendigt.

---

### Step 3: Lav wizard-state

Indfør state for trin.

Eksempel:

```ts
currentStep = 1;
readonly totalSteps = 4;
```

eller brug Angular signals, hvis projektet allerede bruger det.

Tilføj metoder:

```ts
goNext()
goBack()
canContinue()
validateCurrentStep()
submit()
```

Tilpas til eksisterende Angular-stil.

---

### Step 4: Split template i trin

Refactor HTML-template, så hvert trin kun viser relevant indhold.

Eksempel:

```html
@if (currentStep === 1) {
  <!-- Grundinfo -->
}

@if (currentStep === 2) {
  <!-- Spisested -->
}

@if (currentStep === 3) {
  <!-- Deltagere -->
}

@if (currentStep === 4) {
  <!-- Gennemse og opret -->
}
```

Brug den Angular syntax, som projektet allerede bruger.

Hvis projektet bruger `*ngIf`, så fortsæt med `*ngIf`.

---

### Step 5: Tilføj step-indikator

Tilføj en step-indikator øverst under logo/back-knap.

Eksempel:

```text
Trin 1 af 4
Grundinfo
```

Vis gerne en simpel progress bar eller små step dots, hvis det passer til eksisterende design.

---

### Step 6: Forbedr deltager-trinnet

Deltager-trinnet skal være mere kompakt end i dag.

Krav:

- søgning skal stadig fungere
- valg/fravalg skal stadig fungere
- valgte deltagere skal være tydelige
- lange deltagerlister må ikke gøre hele flowet grimt

---

### Step 7: Lav review-trin

Tilføj samlet opsummering.

Brug eksisterende data.

Undgå at brugeren opretter uden at have set det samlede valg.

---

### Step 8: Submit på sidste trin

Flyt submit/opret-knappen til sidste trin.

Eksisterende submit-logik skal genbruges.

Sørg for:

- loading-state
- disabled state
- fejlvisning
- ingen dobbelt-submit

---

### Step 9: Styling

Tilpas kun styles for Opret ordre-siden.

Målet er ikke nyt design, men bedre flow.

Brug eksisterende CSS-klasser og mønstre, hvor det er muligt.

Hvis der tilføjes nye styles, så hold dem lokalt til componenten, medmindre projektets struktur kræver global styling.

---

### Step 10: Test manuelt

Kør appen lokalt.

Test:

1. Åbn Opret ordre.
2. Udfyld titel og kategori.
3. Klik Næste.
4. Vælg spisested eller fortsæt efter eksisterende regler.
5. Klik Næste.
6. Vælg deltagere.
7. Klik Næste.
8. Gennemse data.
9. Klik Opret gruppebetaling.
10. Bekræft at ordren oprettes korrekt.
11. Bekræft at efterfølgende side/redirect stadig virker.
12. Bekræft at Overblik viser ordren korrekt.
13. Bekræft at Beskeder/notifikationer ikke er brudt.

---

### Step 11: Build og test

Kør relevante kommandoer.

Typisk:

```powershell
cd src/Frontend.PayBySharePay
npm install
npm run build
```

Hvis projektet har tests:

```powershell
npm test
```

For backend, hvis der er ændringer eller integrationen skal verificeres:

```powershell
dotnet build
dotnet test
```

---

### Step 12: Slutrapport

Når du er færdig, skal du skrive en kort rapport med:

- hvilke filer der blev ændret
- hvordan flowet nu er opdelt
- hvordan validering virker
- om API-kontrakten blev bevaret
- om der blev tilføjet tests
- hvordan løsningen blev testet
- eventuelle kendte begrænsninger

---

## Ting du ikke må gøre

Du må ikke:

- ændre farvetema
- redesigne hele appen
- ændre logo
- fjerne bundnavigation
- ændre backend unødvendigt
- ændre database
- ændre API-kontrakter uden meget god grund
- fjerne eksisterende funktionalitet
- ødelægge eksisterende ordreflow
- navigere til en anden side end eksisterende kode gør efter oprettelse, medmindre det allerede er dokumenteret som ønsket adfærd
- introducere ny tung dependency uden grund

---

## Eksempel på ønsket brugeroplevelse

```text
Bruger trykker Opret i bundnavigation

App viser:
Trin 1 af 4 - Grundinfo
Titel, kategori, besked
[Næste]

Bruger trykker Næste

App viser:
Trin 2 af 4 - Spisested
Vælg spisested
[Tilbage] [Næste]

Bruger trykker Næste

App viser:
Trin 3 af 4 - Deltagere
Søg og vælg deltagere
[Tilbage] [Næste]

Bruger trykker Næste

App viser:
Trin 4 af 4 - Gennemse og opret
Opsummering
[Tilbage] [Opret gruppebetaling]
```

---

## Prioritet

Dette er en UX-refactor af høj prioritet.

Det vigtigste er:

1. Kortere og mere overskuelige trin
2. Bevar eksisterende design
3. Bevar eksisterende funktionalitet
4. Mobil-first layout
5. Ingen backend/database-ændringer, medmindre absolut nødvendigt
