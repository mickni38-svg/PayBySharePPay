# Copilot Prompt: Refactor CreateOrder til wizard flow uden at ændre funktionalitet

## Rolle

Du er senior Angular-udvikler og UX-orienteret frontend-arkitekt.

Du skal refactor den eksisterende **CreateOrder / Opret ordre** side i PayBySharePay til et **4-trins wizard flow** baseret på de vedlagte screenshots.

Du må ikke ændre eksisterende forretningslogik, backend, API-kontrakter, datamodel eller oprettelsesfunktionalitet. Opgaven er en frontend/UX-refactor af den eksisterende CreateOrder-side.

---

## Meget vigtigt

Funktionaliteten må ikke laves om.

Det betyder:

- Samme data skal sendes til backend som i dag.
- Samme API endpoint skal bruges.
- Samme request model skal bruges.
- Samme valideringsregler skal bevares, medmindre der allerede findes tydelige fejl.
- Samme redirect/navigation efter oprettelse skal bevares.
- Samme måde at hente deltagere på skal bevares.
- Samme måde at hente/markere spisested/merchant på skal bevares.
- Samme måde at vælge kategori på skal bevares funktionelt, men UI skal ændres.
- Samme bundnavigation skal bevares.
- Farver, dark theme, logo og overordnet designstil skal bevares.

Du må kun ændre præsentation, komponentstruktur og lokal UI-state, så formularen bliver opdelt i wizard steps.

---

## Før du starter

Læs først eksisterende dokumentation:

```text
README.md
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

Find derefter den eksisterende CreateOrder/Opret ordre kode i Angular-projektet.

Søg efter relevante navne som:

```text
create-order
createorder
opret
order
orders
group payment
gruppebetaling
```

Undersøg især:

```text
src/Frontend.PayBySharePay/src/app
```

Find de faktiske filer før du ændrer noget.

---

## Vedlagte billeder

Brug de 4 vedlagte screenshots som designreference for det nye flow:

```text
opretordre1.png  -> Trin 1: Grundinfo
opretordre2.png  -> Trin 2: Spisested
opretordre3.png  -> Trin 3: Deltagere
opretordre4.png  -> Trin 4: Gennemse
```

Der findes også et samlet flow-billede:

```text
paybysharepay_app_setup_walkthrough.png
```

Brug billederne som visuel reference, men implementér med eksisterende Angular-kode og eksisterende CSS/designsystem.

---

## Ønsket wizard flow

CreateOrder skal ændres fra én lang formular til dette flow:

```text
Trin 1 af 4: Grundinfo
Trin 2 af 4: Spisested
Trin 3 af 4: Deltagere
Trin 4 af 4: Gennemse
```

Der skal være en step-indikator øverst på siden:

```text
1 Grundinfo
2 Spisested
3 Deltagere
4 Gennemse
```

Step-indikatoren skal vise:

- aktivt trin
- tidligere gennemførte trin
- kommende trin
- samme visuelle stil som screenshots

Der skal **ikke** være tilbage-pil øverst på siden.

Der skal heller ikke være en separat tilbage-knap i wizard-flowet, medmindre eksisterende navigation kræver det. Brugeren går primært frem med **Næste**.

---

## Trin 1: Grundinfo

Trin 1 skal indeholde:

- Titel
- Kategori
- Besked, valgfri
- Næste-knap

### Titel

Bevar eksisterende titel-felt og binding.

Eksempel:

```text
Titel
fx pizza aften
```

### Kategori

Kategoriområdet skal refactores.

Den gamle løsning viser for mange emoji-knapper direkte i formularen. Det fungerer ikke, når der kan være 100+ kategorier.

Ny løsning:

- Der skal være et almindeligt søgefelt.
- Søgefeltet må **ikke** være dropdown.
- Søgefeltet bruges kun til at filtrere/søge i kategorier.
- Der skal vises et lille antal foreslåede kategorier direkte under søgefeltet.
- Der skal være en række/knap: `Se alle kategorier`
- Der skal vises tekst som fx `100+ kategorier`.
- Brugeren kan kun vælge **én kategori ad gangen**.
- Når en kategori vælges, bliver selve kategori-chip/knap markeret som selected.
- Der skal **ikke** være et ekstra felt/chip ovenover, der viser valgt kategori.
- Den valgte kategori vises kun ved at den valgte kategori er markeret/selected i listen.
- Hvis brugeren vælger en ny kategori, fjernes selection fra den gamle.

Eksempel på kategori UI:

```text
Kategori (kun én kan vælges)

[Søg eller vælg kategori]

[🍕 Pizza] [🍔 Burger] [🍣 Sushi] [🌮 Tacos] [🍺 Drikke]

[Se alle kategorier                              100+ kategorier >]
```

Når `Pizza` er valgt:

```text
[🍕 Pizza]  -> selected med grøn border/active style
```

Der må ikke vises:

```text
Valgt kategori: 🍕 Pizza
```

eller en ekstra selected-chip over forslagene.

### Se alle kategorier

Når brugeren trykker `Se alle kategorier`, skal der åbnes en mere skalerbar visning til mange kategorier.

Vælg den løsning der passer bedst til den eksisterende Angular-kode:

- modal/bottom sheet
- full screen panel
- ekspanderet liste
- separat overlay

Krav:

- Skal kunne håndtere 100+ kategorier.
- Skal have søgning/filter.
- Skal kun tillade single select.
- Når en kategori vælges, lukkes modal/panel gerne automatisk og kategorien vises som selected i kategoriområdet.
- Ingen multi-select.

Hvis projektet allerede har modal/bottom sheet komponenter, skal de genbruges. Ellers implementér en enkel løsning uden ny tung dependency.

### Besked

Bevar eksisterende felt og binding:

```text
Besked (valgfri)
Skriv en kort besked til deltagerne...
```

### Næste-knap

Når brugeren klikker **Næste**:

- valider kun trin 1
- hvis titel/kategori er påkrævet i eksisterende kode, skal det valideres her
- hvis validering fejler, vis fejl i samme stil som resten af appen
- hvis validering lykkes, gå til trin 2

---

## Trin 2: Spisested

Trin 2 skal indeholde:

- overskrift `Spisested`
- søgefelt til spisested
- liste over spisesteder/merchants
- selected state på valgt spisested
- Næste-knap

Søgefeltet skal bruges til at filtrere listen.

Eksempel:

```text
Vælg spisested

[Søg spisested...]

Pizzeria Roma ApS
Vesterbrogade 12, 1620 København V

Burger House
Nørrebrogade 45, 2200 København N

Sushi City
Amagerbrogade 77, 2300 København S
```

Valgt spisested markeres med grøn border/check/radio som på screenshot.

Funktionaliteten skal bevares:

- Brug eksisterende merchant/spisested data.
- Brug eksisterende selectedMerchant/merchantId logic.
- Hvis spisested er påkrævet i eksisterende oprettelse, skal brugeren ikke kunne gå videre uden valg.
- Hvis spisested ikke er påkrævet, må flowet tillade næste.

---

## Trin 3: Deltagere

Trin 3 skal indeholde:

- overskrift `Deltagere`
- søgefelt
- liste over deltagere
- selected state direkte i listen
- Næste-knap

Vigtig UX-regel:

Der skal **ikke** være en separat række/sektion med “valgte deltagere” eller selected chips øverst.

Man kan allerede se hvem der er valgt via grønne checkmarks direkte i deltagerlisten.

Derfor skal dette fjernes:

```text
3 deltagere valgt
[Anders Nielsen x] [Søren Jensen x] [Lene Hansen x]
```

Det må ikke vises.

Det ønskede UI er:

```text
[Søg navn...]

Anders Nielsen      ✅
Søren Jensen        ✅
Lene Hansen         ✅
Camilla Andersen    ○
Thomas Larsen       ○
```

Der må gerne være en lille tekst som fx:

```text
Vælg deltagere
```

men ikke en ekstra selected-chip-liste.

Funktionaliteten skal bevares:

- Brug eksisterende deltagerliste.
- Brug eksisterende select/unselect logic.
- Brug eksisterende participant IDs i request.
- Hvis mindst én deltager kræves, valideres det før næste trin.
- Hvis nuværende bruger/vært automatisk tilføjes i eksisterende kode, må dette ikke ændres.

---

## Trin 4: Gennemse

Trin 4 skal indeholde en samlet opsummering.

Vis:

- Titel
- Kategori
- Besked
- Spisested
- Antal deltagere
- Liste over valgte deltagere
- Statusboks: `Klar til oprettelse`
- Primær knap: `Opret gruppebetaling`

Eksempel:

```text
Gennemse gruppebetaling

Titel             Fredagspizza med venner
Kategori          Pizza
Besked            Vi bestiller kl. 18
Spisested         Pizzeria Roma ApS
Deltagere         3 valgt

Valgte deltagere
Anders Nielsen
Søren Jensen
Lene Hansen

Klar til oprettelse
Alt ser godt ud. Du kan nu oprette din gruppebetaling.

[Opret gruppebetaling]
```

Knappen `Opret gruppebetaling` må kun vises på trin 4.

Når brugeren trykker `Opret gruppebetaling`:

- brug eksisterende submit metode
- send samme request som før
- brug samme API-service som før
- bevar loading-state eller implementér hvis den mangler
- disable knappen under submit
- undgå dobbelt-submit
- vis fejlbesked hvis API-kald fejler
- bevar eksisterende navigation efter succes

---

## Navigation i wizard

Wizard-state skal som minimum have:

```ts
currentStep: number;
totalSteps: number = 4;
```

Der skal implementeres metoder svarende til:

```ts
goNext()
goToStep(step: number)
validateCurrentStep()
submit()
```

`goToStep` må gerne tillade navigation tilbage til tidligere trin via step-indikatoren, men må ikke tillade at springe frem til et trin, der kræver manglende data.

Eksempel:

- Fra trin 1 kan man ikke hoppe direkte til trin 4.
- Fra trin 3 kan man gerne trykke på trin 1 eller 2 og rette data.
- Fra trin 4 kan man gå tilbage til tidligere trin ved at trykke i step-indikatoren.

Der skal ikke være en tilbage-pil øverst.

Der skal ikke være en stor sekundær `Tilbage`-knap, medmindre det er nødvendigt for eksisterende accessibility eller eksisterende kode. Hvis du tilføjer tilbage-navigation, så brug step-indikatoren.

---

## Stylingkrav

Bevar eksisterende stil:

- mørk baggrund
- PayBySharePay logo
- neon-grøn active state
- samme bundnavigation
- samme card-stil
- samme input-stil
- samme rounded corners
- samme spacing-stil
- samme typografi så vidt muligt

Du må gerne forbedre spacing på CreateOrder-siden, men du må ikke redesigne hele appen.

Styling skal helst ligge i den relevante component stylesheet.

Undgå globale CSS-ændringer, medmindre projektets struktur allerede bruger globale styles til disse komponenter.

---

## Mobilkrav

Siden skal være mobile-first.

Test layout ved ca.:

```text
390px bredde
430px bredde
```

Krav:

- ingen vandret scroll
- bundnavigation må ikke dække knapper
- Næste/Opret-knappen skal være nem at nå
- hvert trin skal føles kort
- kategorier skal ikke fylde hele skærmen
- deltagerlisten må ikke have ekstra selected-chip-række
- kategori-listen skal kunne håndtere 100+ kategorier via søgning og “Se alle kategorier”

---

## Teknisk implementering

Brug eksisterende Angular-mønstre i projektet.

Undersøg om komponenten bruger:

- standalone components
- Reactive Forms
- Template-driven forms
- Signals
- RxJS
- services
- SCSS/CSS

Følg den eksisterende stil i kodebasen.

Du må gerne opdele CreateOrder i mindre child components, hvis det gør koden mere overskuelig, fx:

```text
create-order-stepper
create-order-basic-info-step
create-order-merchant-step
create-order-participants-step
create-order-review-step
category-picker
```

Men gør det kun hvis det passer til projektets struktur. En simpel refactor i samme component er også acceptabel, hvis det er mest sikkert.

---

## Kategori-data

Hvis kategorier i dag er hardcoded som emoji-array, må du gerne omstrukturere dem til en tydelig liste/array i frontend.

Eksempel:

```ts
categories = [
  { id: 'pizza', label: 'Pizza', emoji: '🍕' },
  { id: 'burger', label: 'Burger', emoji: '🍔' },
  { id: 'sushi', label: 'Sushi', emoji: '🍣' }
];
```

Krav:

- single select
- selected category gemmes i samme model/request som før
- eksisterende kategori-værdi i backend må ikke ændres uden grund
- hvis backend forventer emoji/string, så send samme format som før
- hvis backend forventer category id, så send samme id som før

Du må ikke ændre database eller backend for kategorier.

---

## Fejl og validering

Fejlbeskeder skal være inline og matche appens stil.

Undgå:

```ts
alert(...)
```

Brug i stedet eksisterende error handling, hvis den findes.

Eksempler:

```text
Titel skal udfyldes
Vælg en kategori
Vælg et spisested
Vælg mindst én deltager
```

Kun vis relevante fejl for det trin brugeren er på.

---

## Build og test

Efter implementering skal du køre relevante checks.

Frontend:

```powershell
cd src/Frontend.PayBySharePay
npm install
npm run build
```

Hvis der findes test setup:

```powershell
npm test
```

Backend skal ikke ændres, men hvis du har rørt shared contracts eller API-integration, så kør:

```powershell
dotnet build
dotnet test
```

---

## Acceptkriterier

Opgaven er færdig når:

- CreateOrder er ændret til 4-trins wizard flow.
- Funktionaliteten er den samme som før.
- Backend/API/datamodel er ikke ændret.
- Trin 1 indeholder titel, kategori og besked.
- Kategori understøtter 100+ kategorier via søgning og `Se alle kategorier`.
- Kategori er single-select.
- Valgt kategori vises kun ved selected state på kategorien, ikke i separat valgt-felt.
- Kategori-søgefeltet er ikke en dropdown.
- Trin 2 indeholder valg af spisested.
- Trin 3 indeholder valg af deltagere.
- Trin 3 har ikke en separat “valgte deltagere” chip-liste.
- Trin 4 viser review og `Opret gruppebetaling`.
- `Opret gruppebetaling` findes kun på trin 4.
- Der er ingen tilbage-pil øverst.
- Step-indikator viser aktivt og gennemførte trin.
- Bundnavigation er bevaret.
- Farver og designstil er bevaret.
- Mobilvisning fungerer.
- Build gennemføres uden fejl.
- Eksisterende ordreoprettelse virker stadig.

---

## Slutrapport

Når du er færdig, skal du skrive en kort rapport med:

1. Hvilke filer der blev ændret
2. Hvordan wizard-flowet er implementeret
3. Hvordan kategori single-select virker
4. Hvordan `Se alle kategorier` virker
5. Hvordan deltagertrinnet er forenklet
6. Om API-kontrakten er uændret
7. Hvordan du har testet løsningen
8. Eventuelle kendte begrænsninger

---

## Start nu

Start med at finde den eksisterende CreateOrder/Opret ordre component.

Lav derefter refactoren til wizard flowet baseret på screenshots og kravene herover.

Husk: Dette er en UI/UX-refactor. Funktionaliteten må ikke ændres.
