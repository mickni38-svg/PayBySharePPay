# PayBySharePay UI/UX Flow – Copilot Implementeringsguide v2

Denne guide skal bruges i **Visual Studio Copilot Agent med Claude Sonnet 4.6** til at implementere det nye UI/UX-flow for PayBySharePay.

Upload denne `.md`-fil sammen med det nyeste UI/UX flow-skærmbillede.

---

## Copilot eksekveringsprompt

Kopiér denne korte prompt ind i Copilot Agent:

```text
Læs denne fil igennem.

Brug også det uploadede UI/UX flow-skærmbillede som visuel reference.

Vigtigt:
- Skærmbilledet er kun en visuel reference.
- Data må ikke hardcodes ud fra skærmbilledet.
- Brug eksisterende database/API/services/viewmodels.
- Genbrug eksisterende funktionalitet så meget som muligt.
- Lav kun nye komponenter, properties, DTO’er eller services hvis det er nødvendigt.
- Bevar eksisterende funktionalitet.
- Byg projektet og ret compile errors.

Start med at analysere projektet og fortæl kort hvilke filer du forventer at ændre. Implementér derefter ændringerne trin for trin.
```

---

## Formål

PayBySharePay skal have et tydeligt mobilvenligt dashboard-flow:

1. **Forside** viser hurtig status og navigation.
2. **Overblik / Status** viser ordre opdelt efter brugerens rolle.
3. **Mine ordre (som vært)** viser ordre hvor brugeren er vært/host.
4. **Deltager i** viser ordre hvor brugeren er deltager, men ikke vært.
5. **Ordredetaljer** vises som accordion eller tilsvarende udvidet detaljevisning.
6. **Bestillinger** vises som ordrelinjer pr. deltager og kun når forretningsreglen tillader det.

Designet skal ligne det uploadede flow-skærmbillede, men data skal komme fra projektets eksisterende datalag.

---

## Vigtige principper

- Ingen UI-data må hardcodes ud fra skærmbilledet.
- Skærmbilledet er kun layout-, flow- og designreference.
- Brug eksisterende API, database, services, models og viewmodels.
- Hvis noget mangler i viewmodel-laget, må der tilføjes små, målrettede DTO’er eller viewmodels.
- Lav ikke stor refactor, hvis eksisterende funktionalitet kan genbruges.
- Bevar eksisterende navigation, routing og bundmenu så meget som muligt.
- Designet skal fungere i mobilvisning.
- Alle tekster skal være på dansk.

---

## Begreber og roller

### Vært / host

En vært er den bruger, der har oprettet ordren.

Værten kan:

- se sine egne ordre under fanen **Mine ordre (som vært)**
- se samlet ordrebeløb
- betale hele ordren til butikken
- se hvilke deltagere der har delbetalt
- se deltagernes bestillinger, når ordren er betalt/klar

### Deltager

En deltager er en bruger, der er tilknyttet en ordre, men ikke har oprettet den.

Deltageren kan:

- se ordre under fanen **Deltager i**
- se sin egen andel
- betale sin egen delbetaling
- åbne detaljer/overblik for ordren

---

## Overordnet flow

### Flow A – Fra forside til Mine ordre

1. Brugeren står på **Forside**.
2. Brugeren klikker på **Overblik**.
3. Appen åbner **Status / Overblik**.
4. Fanen **Mine ordre (som vært)** er valgt som standard, hvis det passer med eksisterende logik.
5. Brugeren ser ordre, hvor brugeren er vært.
6. Brugeren klikker på en ordre.
7. Ordren udvides som accordion og viser ordredetaljer.
8. Hvis ordren er betalt/klar, vises deltagernes bestillinger som ordrelinjer.

### Flow B – Fra forside til ordre med afventende betalinger

1. Brugeren står på **Forside**.
2. Øverste statuskort viser fx:

```text
3 ordre har afventende betalinger
På tværs af 3 ordre
```

3. Brugeren klikker på statuskortet.
4. Appen åbner en filtreret visning med ordre, der har afventende betalinger.
5. Brugeren kan åbne en ordre eller sende relevante påmindelser, hvis eksisterende funktionalitet understøtter det.

### Flow C – Deltager i

1. Brugeren står på **Status / Overblik**.
2. Brugeren klikker på fanen **Deltager i**.
3. Appen viser ordre, hvor brugeren deltager, men ikke er vært.
4. Brugeren ser sin egen andel og egen betalingsstatus.
5. Hvis brugeren mangler at betale, vises en handling som fx **Betal din andel**.
6. Hvis brugeren allerede har betalt, vises fx **Detaljer**.

---

## Skærm 1 – Forside

Forsiden skal vise:

- logo øverst
- statuskort for afventende betalinger
- statuskort for opdateret/aktivitet
- fire hovedkort:
  - Overblik
  - Opret
  - Brugere
  - Beskeder
- bundnavigation

### Statuskort 1 – Afventende betalinger

Den gamle tekst som primær tekst må ikke være:

```text
3 deltagere afventer
```

Den nye primære tekst skal være ordrebaseret:

```text
X ordre har afventende betalinger
```

Eksempel:

```text
3 ordre har afventende betalinger
På tværs af 3 ordre
```

Hvis der kun er én ordre:

```text
1 ordre har afventende betaling
På tværs af 1 ordre
```

Hvis der ikke er afventende betalinger:

```text
Du er opdateret
Ingen nye aktiviteter
```

### Beregning

Statuskortet skal beregnes dynamisk ud fra rigtige ordre-/betalingsdata.

Det skal tælle relevante ordre, hvor der mangler betaling eller handling.

Eksempler på relevante betingelser:

- ordre hvor en eller flere deltagere mangler at betale
- ordre hvor betaling til butik afventer
- ordre hvor deltagerhandlinger forhindrer færdiggørelse

Brug eksisterende statusfelter fra projektet, hvis de findes.

### Klik på statuskortet

Klik på kortet skal give merværdi. Det må ikke bare være en blind navigation til den almindelige Overblik-side.

Foretrukken løsning:

- åbne Overblik/Status med filter aktivt: **kun ordre med afventende betalinger**
- eller åbne en dedikeret visning: **Ordre med afventende betalinger**

Hvis projektets routing allerede understøtter query parameters, kan det fx være:

```text
/overview?filter=pending-payments
```

Brug eksisterende routing-konventioner.

---

## Skærm 2 – Status / Overblik

Siden skal have faner:

```text
Mine ordre (som vært)    Deltager i
```

### Vigtigt om navngivning

Fanen må ikke hedde **Mine grupper**, medmindre eksisterende kode kræver det internt.

I UI skal den hedde:

```text
Deltager i
```

Fanen for vært skal hedde:

```text
Mine ordre (som vært)
```

### Undertekst

Når **Mine ordre (som vært)** er valgt:

```text
Dine egne ordre (som vært)
```

Når **Deltager i** er valgt:

```text
Ordre hvor du deltager
```

---

## Fane: Mine ordre (som vært)

Denne fane viser kun ordre, hvor current user er vært/host.

### Hvert ordrekort skal vise

- ordretype/ikon
- ordrenavn, fx Pizzaaften, Biograftur, Fredagsbar
- samlet ordrebeløb
- statusbadge, fx:
  - Afventer betaling
  - Klar
  - Betalt
- deltagerikoner/initialer for deltagere på ordren
- progress-tekst, fx:

```text
1 af 3 har betalt
2 af 4 har betalt
```

- primær handling:
  - **Betal nu** hvis brugeren er vært og ordren afventer betaling
  - **Se detaljer** hvis ordren er klar/betalt

### Betal nu

**Betal nu** må kun vises når:

- current user er vært/host på ordren
- ordren afventer samlet betaling
- det giver forretningsmæssigt mening at betale hele ordren til butikken

Når værten klikker **Betal nu**, betaler værten hele den samlede ordre til butikken.

Deltagernes delbetalinger håndteres separat.

### Vigtigt om bestillinger

Hvis en ordre afventer betaling, må deltagerbestillinger ikke vises.

Vis i stedet en forklarende tekst:

```text
Bestilling vises først når ordren er betalt.
```

eller:

```text
Bestillinger vises først når ordren er betalt.
```

Brug ental/flertal korrekt, hvis det er nemt.

---

## Fane: Deltager i

Denne fane viser ordre, hvor current user deltager, men ikke er vært.

### Hvert ordrekort skal vise

- ordrenavn
- værtens navn, hvis data findes
- brugerens egen andel
- brugerens egen betalingsstatus
- deltagerikoner/initialer
- handling:
  - **Betal din andel** hvis brugeren mangler at betale
  - **Detaljer** hvis brugeren allerede har betalt eller kun skal se info

### Deltagere må ikke betale hele ordren

Hvis current user ikke er vært, må der ikke vises **Betal nu** for hele ordren.

Ikke-værter må kun betale deres egen andel.

---

## Ordredetaljer som accordion

Når brugeren klikker på en ordre, skal ordren åbnes som accordion eller tilsvarende udvidet detaljevisning.

Det kan ske på samme side, hvis det passer med eksisterende UI.

### Accordion skal vise

- ordrenummer
- oprettet dato/tid
- sted / butik / merchant
- betalingsmetode, hvis data findes
- bestillinger, hvis ordren er betalt/klar

Eksempel:

```text
Ordredetaljer
Ordrenr.        #FB-2024-0521
Oprettet        21. maj 2024, 18.30
Sted            The Social Bar
Betalingsmetode MobilePay
```

---

## Bestillinger

Bestillinger skal vises som ordrelinjer pr. deltager.

De må ikke vises som kommasepareret tekst.

### Korrekt visning

```text
Michael Nielsen (dig)
- Burger x1
- Cola x1
- Ekstra mayo x1

Nikoline
- Pizza x1
- Cola x1

Selma
- Pasta x1
- Cola x1
- Ekstra mayo x1

Jonas
- Salat x1
- Vand x1
- Dip x1
```

### Forkert visning

```text
Burger x1, Cola x1, Ekstra mayo x1
```

### Visning afhænger af ordrestatus

Bestillinger må kun vises når ordren er betalt/klar efter projektets forretningsregel.

Hvis ordren stadig afventer betaling, vis ikke deltagerbestillinger.

---

## Deltagerikoner / initialer

Ordrekort skal vise små ikoner/initialer for deltagere tilknyttet ordren.

Eksempel:

```text
MN  NA  SM  OA
```

### Regler

- Initialer genereres fra deltagerens navn, hvis der ikke findes avatar.
- Hvis der er mange deltagere, vis fx de første 4 og derefter `+2`.
- Farver kan genbruges fra eksisterende design/style tokens.
- Deltagerikoner skal være visuelle hints, ikke primære datakilder.

---

## Hurtige handlinger

Hvis projektet allerede har plads til hurtige handlinger på Status/Overblik-siden, kan disse bevares eller justeres:

- Se kvitteringer
- Åbn seneste gruppe/ordre
- Send påmindelser

Lav kun ændringer her, hvis det understøtter flowet og ikke kræver unødvendig ny funktionalitet.

---

## Data og viewmodels

Copilot skal undersøge eksisterende datamodel før implementering.

Find eksisterende modeller/services for:

- Order / GroupOrder / OrderSummary
- Participant / User / Member
- Payment / PaymentStatus
- OrderItem / OrderLine / ProductLine
- Merchant / Shop / Place
- current user / authentication context

### Der må tilføjes viewmodel-felter hvis nødvendigt

Eksempel på relevante viewmodel-felter:

```csharp
bool IsHost
bool IsParticipantOnly
decimal TotalAmount
decimal? CurrentUserShareAmount
int ParticipantCount
int PaidParticipantCount
int PendingParticipantCount
bool CanPayTotalOrder
bool CanPayOwnShare
bool CanShowOrderLines
IReadOnlyList<ParticipantAvatarViewModel> Participants
IReadOnlyList<ParticipantOrderLinesViewModel> ParticipantOrderLines
```

Brug projektets eksisterende navngivning og arkitektur.

Hvis projektet er Angular/TypeScript, brug tilsvarende interfaces/types.

---

## Forretningsregler

### Vært

- Vært ser egne ordre i **Mine ordre (som vært)**.
- Vært kan betale hele ordren til butikken.
- Vært ser **Betal nu**, når ordren afventer betaling.
- Vært ser deltagerstatus og samlet ordrebeløb.

### Deltager

- Deltager ser ordre i **Deltager i**.
- Deltager betaler kun sin egen andel.
- Deltager må ikke se **Betal nu** for hele ordren.

### Bestillinger

- Bestillinger vises kun når ordren er betalt/klar.
- Bestillinger vises som ordrelinjer pr. deltager.
- Afventende ordre viser ikke bestillingslisten.

### Statuskort på forsiden

- Skal være dynamisk.
- Skal være ordrebaseret.
- Skal bruge teksten `X ordre har afventende betalinger`.
- Må ikke bruge `X deltagere afventer` som primær tekst på forsiden.

---

## Designkrav

Designet skal følge det uploadede flow-skærmbillede:

- dark mode
- neon-accent borders
- afrundede kort
- tydelige statusbadges
- blå primære knapper
- grøn status for klar/betalt
- orange/gul status for afventer betaling
- lilla/cyan deltagerikoner
- mobilvenligt layout

Genbrug eksisterende CSS/SCSS/design tokens hvis de findes.

Undgå pixel-perfect hardcoding. Designet skal være responsivt.

---

## Implementeringstrin til Copilot

### Trin 1 – Analyse

Find eksisterende filer for:

- Forside/Home component/page
- Overblik/Status component/page
- Order cards
- Order details
- Navigation/routing
- Services/API integration
- Viewmodels/types/models

Start med at fortælle kort hvilke filer der forventes ændret.

### Trin 2 – Forside

- Erstat gammelt primært statuskort med ordrebaseret statuskort.
- Beregn antal ordre med afventende betalinger dynamisk.
- Vis teksten `X ordre har afventende betalinger`.
- Klik på kortet skal åbne filtreret ordrevisning eller Overblik med filter.

### Trin 3 – Overblik/Status faner

- Omdøb/implementér faner:
  - `Mine ordre (som vært)`
  - `Deltager i`
- Sørg for at fanerne filtrerer data korrekt ud fra current user.

### Trin 4 – Mine ordre (som vært)

- Vis kun ordre hvor current user er vært.
- Tilføj deltagerikoner.
- Tilføj progress-tekst `X af Y har betalt`.
- Vis `Betal nu` kun når vært kan betale samlet ordre.
- Vis `Se detaljer` for relevante færdige/betalte ordre.

### Trin 5 – Deltager i

- Vis ordre hvor current user deltager, men ikke er vært.
- Vis brugerens egen andel.
- Vis `Betal din andel` når relevant.
- Vis aldrig `Betal nu` for hele ordren for deltagere.

### Trin 6 – Accordion / detaljer

- Implementér eller genbrug accordion for ordredetaljer.
- Vis ordrenummer, oprettet dato, sted og betalingsmetode.
- Vis bestillinger kun når `CanShowOrderLines == true` eller tilsvarende eksisterende regel.

### Trin 7 – Bestillingslinjer

- Vis ordrelinjer pr. deltager.
- Undgå kommasepareret tekst.
- Brug eksisterende order line data.
- Tilføj viewmodel mapping, hvis nødvendigt.

### Trin 8 – Build og test

- Byg projektet.
- Ret compile errors.
- Tilføj/opdater tests hvis projektet har teststruktur.
- Kontroller at eksisterende funktionalitet stadig virker.

---

## Acceptkriterier

- Appen bygger uden fejl.
- Forsiden viser `X ordre har afventende betalinger` dynamisk.
- Forsiden bruger ikke `X deltagere afventer` som primær status.
- Klik på statuskort åbner en relevant filtreret visning eller Overblik med filter.
- Overblik/Status har fanerne:
  - `Mine ordre (som vært)`
  - `Deltager i`
- `Mine ordre (som vært)` viser kun ordre hvor current user er vært.
- `Deltager i` viser kun ordre hvor current user deltager, men ikke er vært.
- `Betal nu` vises kun for vært/host på egne ordre.
- Ikke-værter kan kun betale egen andel eller se detaljer.
- Ordrekort viser deltagerikoner/initialer.
- Ordrekort viser samlet ordrebeløb for vært.
- Ordrekort viser egen andel for deltager.
- Ordredetaljer vises via accordion eller tilsvarende detaljevisning.
- Bestillinger vises som ordrelinjer pr. deltager.
- Bestillinger vises kun når ordren er betalt/klar efter forretningsreglen.
- Afventende ordre viser forklaring i stedet for bestillinger.
- Bundnavigationen virker fortsat.
- Ingen skærmbillede-data er hardcoded.
- Eksisterende funktionalitet er ikke ødelagt.

---

## Noter til Copilot

Hvis eksisterende kode bruger andre navne end denne guide, så brug projektets navne internt, men sørg for at UI-teksterne følger guiden.

Eksempel:

- Intern model kan hedde `GroupOrder`, men UI kan stadig vise `Ordre`.
- Intern side kan hedde `GroupsPage`, men fanen skal i UI hedde `Deltager i`.

Prioritér små, sikre ændringer frem for stor refactor.
