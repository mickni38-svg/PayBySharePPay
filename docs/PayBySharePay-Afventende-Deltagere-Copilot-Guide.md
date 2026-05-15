# PayBySharePay – Afventende deltagere på tværs af ordre

## Eksekveringsprompt til Visual Studio Copilot Agent / Claude Sonnet 4.6

Kopiér denne tekst ind i Copilot Agent, når denne `.md`-fil og flow-skærmbilledet er uploadet:

```text
Læs og implementér den uploadede fil:

PayBySharePay-Afventende-Deltagere-Copilot-Guide.md

Brug også det uploadede flow-skærmbillede som visuel reference.

Vigtigt:
- Skærmbilledet er kun en visuel reference.
- Data må ikke hardcodes ud fra skærmbilledet.
- Brug eksisterende database/API/services/viewmodels så meget som muligt.
- Løsningen har allerede meget funktionalitet, så lav ikke unødvendig refactor.
- Tilføj kun nye komponenter, properties, DTO’er eller services, hvis det er nødvendigt.
- Afventende ordre/deltagere må ikke vise beløb, hvis beløbet først findes efter betalt/bekræftet bestilling.
- Byg projektet og ret compile errors.

Implementér flowet trin for trin ud fra kravene i denne guide.
Start med at analysere projektet og fortæl kort hvilke filer du forventer at ændre.
```

---

## 1. Formål

Forsiden skal have et statuskort, som viser afventende deltagere på tværs af alle relevante ordre.

Kortet skal ikke bare være et link til den almindelige Overblik-side. Det skal åbne en dedikeret opfølgningsside, hvor brugeren hurtigt kan se, hvem der mangler at gøre noget, og sende påmindelser.

Flowet er:

```text
Forside
  → klik på "X deltagere afventer"
      → Deltagere der afventer
          → Send påmindelse til alle / enkelt ordre
          → Gå til konkret ordre
```

---

## 2. Vigtig forretningsregel om beløb

Afventende ordre eller afventende deltagere må ikke vise endeligt beløb, hvis beløbet først findes efter at bestillingen er betalt/bekræftet.

Det betyder:

```text
Afventende ordre
  → ingen deltagerbestilling vises
  → intet konkret deltagerbeløb vises, hvis beløbet ikke findes endnu
  → vis i stedet status: "Mangler at vælge bestilling", "Mangler betaling" eller lignende

Betalt/klar ordre
  → bestillinger må vises
  → ordrelinjer må vises
  → beløb må vises, hvis de findes i databasen
```

Hvis flow-skærmbilledet viser et beløb på en afventende deltager, skal det behandles som en visuel fejl/referencefejl. Implementeringen skal følge forretningsreglen ovenfor.

---

## 3. Forside – statuskort

På forsiden skal det øverste statuskort vise et dynamisk samlet antal deltagere, der afventer handling på tværs af ordre.

Eksempel:

```text
3 deltagere afventer
På tværs af 2 ordre
```

Hvis der ikke er nogen afventende deltagere, skal kortet skifte til positiv status:

```text
Du er opdateret
Ingen afventende handlinger
```

### Krav

- Antal deltagere skal beregnes fra rigtige ordredata.
- Antal ordre skal beregnes fra rigtige ordredata.
- Data må ikke hardcodes.
- Kortet skal kun tælle relevante afventende deltagere.
- Klik på kortet skal åbne en dedikeret side: `Deltagere der afventer`.
- Kortet må ikke bare navigere til almindelig Overblik.

### Hvad tæller som afventende?

En deltager tæller som afventende, hvis deltageren stadig mangler en nødvendig handling, fx:

```text
- mangler at vælge bestilling
- mangler at betale sin andel
- mangler at bekræfte deltagelse
- anden eksisterende status i systemet, som betyder at ordren ikke kan færdiggøres
```

Brug eksisterende statusfelter fra domænemodeller/API, hvis de findes.

---

## 4. Ny side – Deltagere der afventer

Når brugeren klikker på statuskortet på forsiden, skal appen åbne en dedikeret opfølgningsside.

Siden skal hedde:

```text
Deltagere der afventer
```

Undertitel:

```text
Liste over alle deltagere, der mangler at gøre noget
```

### Top summary

Øverst vises et summary-kort med fx:

```text
3 deltagere
2 ordre
```

Beløb må kun vises her, hvis det kan beregnes korrekt ud fra betalte/bekræftede ordredata. Hvis ordre stadig er afventende, og beløbet ikke er sikkert endnu, skal samlet beløb udelades.

Anbefalet visning:

```text
3
Deltagere

2
Ordre
```

Hvis beløb er tilgængeligt for relevante betalinger:

```text
1.050,00 kr.
Samlet kendt beløb
```

Hvis beløb ikke er tilgængeligt:

```text
Beløb beregnes først når bestillinger er betalt
```

---

## 5. Gruppering efter ordre

De afventende deltagere skal grupperes efter ordre.

Eksempel:

```text
Biograftur
2 afventer

Nikoline Annenberg
Mangler at vælge bestilling

Selma Markussen
Mangler at vælge bestilling

[Se ordre]
```

```text
Pizzaaften
1 afventer

Olivia Anneberg
Mangler at vælge bestilling

[Se ordre]
```

### Krav til ordrekort på siden

Hvert ordrekort skal vise:

```text
- ordrenavn
- statusbadge, fx "2 afventer"
- liste over afventende deltagere
- deltagerinitialer/avatar
- hvad hver deltager mangler at gøre
- handling: Se ordre
```

Hvert ordrekort må ikke vise deltagerbeløb, hvis ordren er afventende og beløb først kendes efter betaling/bekræftet bestilling.

---

## 6. Send påmindelse

Siden skal have en primær handling nederst:

```text
Send påmindelse til alle afventende
```

Når brugeren klikker på knappen, åbnes en bekræftelsesvisning eller dialog.

### Send påmindelse – alle

Dialogen/siden skal vise:

```text
Send påmindelse

3 deltagere vil modtage en påmindelse
På tværs af 2 ordre

Modtagere:
- Nikoline Annenberg · Biograftur
- Selma Markussen · Biograftur
- Olivia Anneberg · Pizzaaften

Besked:
Hej 👋
Vi mangler stadig, at du færdiggør din handling i PayBySharePay.
Så vi kan gøre ordren klar.
Tak! 🙂

[Send påmindelse]
[Annuller]
```

### Krav

- Modtagere skal komme fra database/API/services.
- Beskeden kan gerne være en standardbesked, men bør ligge ét sted i koden, ikke kopieres rundt.
- Brug eksisterende besked-/notification-service, hvis den findes.
- Hvis der ikke findes en beskedservice endnu, implementér UI og en tydelig placeholder-metode/service, der senere kan kobles på backend.
- Der skal ikke sendes rigtige beskeder fra frontend alene, hvis backend-flow ikke findes.

---

## 7. Gå til ordre

Hvert ordrekort skal have handlingen:

```text
Se ordre
```

Når brugeren klikker på `Se ordre`, skal appen åbne den relevante ordre i eksisterende ordre-/overblik-flow.

Det må gerne navigere til den normale ordrevisning, men kun som sekundær handling fra opfølgningssiden.

Forsidens statuskort må stadig ikke bare være et direkte link til Overblik.

---

## 8. Relation til almindelig Overblik

Den almindelige bundnavigation `Overblik` skal fortsat fungere som før, men med det nye UI/UX-flow:

```text
Overblik
  → Status
      → Mine ordre (som vært)
      → Deltager i
```

Denne side viser bredere overblik over ordre.

Den nye side `Deltagere der afventer` er en fokuseret opfølgningsside.

### Forskellen

```text
Overblik
= Se alle relevante ordre og detaljer

Deltagere der afventer
= Se kun de deltagere, der blokerer for at ordre kan afsluttes
= Send påmindelser hurtigt
= Gå direkte til relevant ordre
```

---

## 9. Data og modeller

Brug eksisterende modeller, services og API-kald, hvis de findes.

Copilot skal undersøge projektet for relevante begreber som:

```text
Order
GroupOrder
Participant
Payment
OrderStatus
PaymentStatus
OrderItem
OrderLine
User
Host
Merchant
```

### Nødvendig viewmodel

Hvis der ikke allerede findes en passende viewmodel, opret en lille viewmodel/DTO til UI’et.

Eksempel:

```csharp
public sealed class PendingParticipantsSummaryViewModel
{
    public int PendingParticipantCount { get; init; }
    public int AffectedOrderCount { get; init; }
    public IReadOnlyList<PendingOrderViewModel> Orders { get; init; } = Array.Empty<PendingOrderViewModel>();
}

public sealed class PendingOrderViewModel
{
    public Guid OrderId { get; init; }
    public string OrderName { get; init; } = string.Empty;
    public int PendingCount { get; init; }
    public string? MerchantName { get; init; }
    public IReadOnlyList<PendingParticipantViewModel> PendingParticipants { get; init; } = Array.Empty<PendingParticipantViewModel>();
}

public sealed class PendingParticipantViewModel
{
    public Guid ParticipantId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Initials { get; init; } = string.Empty;
    public string PendingReason { get; init; } = string.Empty;
}
```

Tilpas navne og typer til projektets eksisterende stil.

Hvis projektet er Angular/TypeScript, opret tilsvarende interfaces:

```typescript
export interface PendingParticipantsSummary {
  pendingParticipantCount: number;
  affectedOrderCount: number;
  orders: PendingOrder[];
}

export interface PendingOrder {
  orderId: string;
  orderName: string;
  pendingCount: number;
  merchantName?: string;
  pendingParticipants: PendingParticipant[];
}

export interface PendingParticipant {
  participantId: string;
  displayName: string;
  initials: string;
  pendingReason: string;
}
```

---

## 10. Beløb må ikke hardcodes

Beløb må ikke hardcodes fra skærmbilledet.

Regel:

```text
Hvis ordre/deltager stadig er afventende, og beløbet først findes efter betalt/bekræftet bestilling, skal UI ikke vise beløb.
```

Vis i stedet:

```text
Mangler at vælge bestilling
Mangler betaling
Afventer betaling
Beløb beregnes først når bestillingen er betalt
```

Hvis en ordre er betalt/klar, må ordrelinjer og beløb vises, hvis data findes.

---

## 11. UI-designkrav

Designet skal følge det eksisterende PayBySharePay dark/neon design:

```text
- mørk baggrund
- afrundede kort
- neon lilla/grøn/blå/orange kanter
- tydelige statusbadges
- deltagerinitialer i cirkler
- stor tydelig primær handling
- bundnavigation bevares
```

Siden skal fungere godt i mobilvisning.

---

## 12. Navigation

Implementér navigationen sådan:

```text
Forside statuskort
  → PendingParticipantsPage / DeltagereDerAfventerPage

DeltagereDerAfventerPage
  → SendReminderPage/Dialog
  → OrderDetails/Overblik for konkret ordre
```

Brug projektets eksisterende routing/nav-service.

Undgå at lave parallel navigation, hvis projektet allerede har en etableret måde at navigere på.

---

## 13. Tom tilstand

Hvis der ikke findes afventende deltagere, skal siden vise en positiv tom tilstand:

```text
Du er opdateret
Ingen deltagere afventer handling lige nu.
```

Forsidens kort skal i samme situation vise:

```text
Du er opdateret
Ingen afventende handlinger
```

---

## 14. Acceptkriterier

Implementeringen er færdig når:

- Appen bygger uden fejl.
- Forsiden viser dynamisk antal afventende deltagere og berørte ordre.
- Forsidens statuskort åbner en dedikeret opfølgningsside, ikke bare almindelig Overblik.
- Siden `Deltagere der afventer` viser deltagere grupperet efter ordre.
- Afventende ordre/deltagere viser ikke beløb, hvis beløbet først er tilgængeligt efter betaling/bekræftet bestilling.
- Brugeren kan sende påmindelse til alle afventende deltagere.
- Brugeren kan se modtagere inden påmindelsen sendes.
- Brugeren kan gå til konkret ordre via `Se ordre`.
- Overblik i bundmenuen fungerer fortsat som den almindelige ordre/status-side.
- Der bruges rigtige data fra database/API/services.
- Der er ingen hardcoded UI-data fra skærmbilledet.
- Eksisterende funktionalitet er ikke ødelagt.

---

## 15. Implementeringstrin til Copilot

1. Find eksisterende forside/home component/page.
2. Find eksisterende ordre-, deltager- og betalingsmodeller.
3. Find eksisterende API/services til ordredata.
4. Find eksisterende navigation/routing.
5. Implementér dynamisk summary til forsiden.
6. Tilføj/ret statuskort på forsiden.
7. Opret ny side/component for `Deltagere der afventer`.
8. Opret viewmodel/interface til pending participants, hvis nødvendigt.
9. Implementér gruppering efter ordre.
10. Implementér tom tilstand.
11. Implementér send-påmindelse dialog/side.
12. Implementér `Se ordre` navigation.
13. Sørg for at beløb kun vises når det er tilladt efter forretningsreglen.
14. Genbrug eksisterende styling.
15. Byg projektet.
16. Ret compile errors.
17. Opdater eller tilføj relevante tests, hvis projektet har teststruktur.
18. Afslut med en kort opsummering af ændrede filer.

