# PayBySharePay – Step-by-step guide til “Seneste aktivitet” på forsiden

## Formål

Denne guide beskriver, hvordan Copilot Claude Sonnet 4.6 skal implementere det nye statuskort på forsiden:

```text
Seneste aktivitet
Ingen nye aktiviteter
```

Kortet erstatter det tidligere kort:

```text
Du er opdateret
Ingen nye aktiviteter
```

Kortet skal ikke være en genvej til Overblik. Det skal være et selvstændigt aktivitetskort, der viser brugerens seneste relevante hændelser på tværs af ordre, betalinger, deltagere, beskeder og invitationer.

---

## Eksekveringsprompt til Copilot

Kopiér denne prompt ind i Visual Studio Copilot Agent / Claude Sonnet 4.6 sammen med denne `.md`-fil:

```text
Læs og implementér den uploadede fil:

PayBySharePay-Seneste-Aktivitet-Copilot-Guide.md

Brug eksisterende projektstruktur, database/API/services/viewmodels og UI-styling så meget som muligt.

Vigtigt:
- Data må ikke hardcodes.
- Brug eksisterende data fra backend, database, API, services eller mock/demo-data-lag hvis projektet allerede har det.
- Bevar eksisterende funktionalitet.
- Lav kun nye komponenter, DTO’er, viewmodels eller services hvis det er nødvendigt.
- Implementér ændringerne trin for trin.
- Byg projektet og ret compile errors.

Start med at analysere projektet og fortæl kort hvilke filer du forventer at ændre. Implementér derefter guiden.
```

---

## Baggrund

Forsiden har to øverste statuskort:

1. **X deltagere afventer**  
   Dette er et handlingskort. Det åbner en særskilt opfølgningsside på tværs af ordre, hvor værten kan se deltagere der mangler at gøre noget og sende påmindelser.

2. **Seneste aktivitet**  
   Dette er et aktivitetskort. Det viser om der er sket noget nyt siden brugeren sidst var aktiv.

Denne guide handler kun om **Seneste aktivitet**.

---

## Ønsket resultat på forsiden

På forsiden skal det grønne statuskort vise:

```text
Seneste aktivitet
Ingen nye aktiviteter
```

Når der findes nye aktiviteter, skal kortet dynamisk ændre sig, fx:

```text
2 nye aktiviteter
Selma har betalt · Jonas har valgt mad
```

eller:

```text
Seneste aktivitet
2 nye hændelser siden sidst
```

Vælg den løsning der passer bedst til projektets eksisterende UI-komponenter.

---

## Vigtig afgrænsning

Kortet må ikke bare sende brugeren til almindelig Overblik.

Kortet skal åbne en selvstændig visning for seneste aktivitet. Det kan implementeres som én af følgende afhængigt af projektets nuværende struktur:

- en separat side/rute
- en modal/dialog
- en bottom sheet
- en eksisterende besked-/aktivitetsside, hvis den allerede findes og passer funktionelt

Hvis projektet allerede har en aktivitets- eller notifikationsside, så genbrug den. Hvis ikke, opret en enkel og fokuseret side eller dialog.

---

# Funktionel specifikation

## 1. Forsidekort: “Seneste aktivitet”

### Standardvisning når der ikke er nye aktiviteter

Vis:

```text
Seneste aktivitet
Ingen nye aktiviteter
```

Kortet skal have samme visuelle stil som det nuværende grønne statuskort:

- grønt ikon
- mørkt kort
- afrundede hjørner
- højre-chevron
- samme spacing og font-stil som resten af forsiden

### Visning når der er nye aktiviteter

Hvis der findes ulæste/nye aktiviteter, skal kortet vise antal eller kort resumé.

Eksempel:

```text
2 nye aktiviteter
Selma har betalt · Jonas har valgt mad
```

Alternativt:

```text
Seneste aktivitet
2 nye hændelser siden sidst
```

Brug den tekstform der passer bedst til eksisterende layout og datamodel.

---

## 2. Klik på kortet

Når brugeren klikker på kortet, åbnes visningen:

```text
Seneste aktivitet
```

Denne visning skal vise brugerens seneste relevante hændelser på tværs af systemet.

Det skal ikke være en kopi af Overblik, og det skal ikke primært vise ordrelisten. Det skal vise aktivitets-hændelser.

---

## 3. Aktivitetstyper

Hvis datamodellen understøtter det, skal følgende typer aktivitet kunne vises:

### Betalinger

Eksempler:

```text
Selma har betalt sin andel til Pizzaaften
Jonas mangler stadig betaling til Biograftur
```

### Bestillinger

Eksempler:

```text
Jonas har valgt Burger, Cola og Ekstra mayo
Nikoline har valgt Pizza og Cola
```

### Ordreændringer

Eksempler:

```text
Pizzaaften er klar til betaling
Biograftur mangler stadig 2 deltagere
Fredagsbar er betalt
```

### Deltagere

Eksempler:

```text
Olivia blev tilføjet til Biograftur
Nikoline har accepteret invitationen
```

### Beskeder/notifikationer

Eksempler:

```text
Ny besked i Pizzaaften
Påmindelse sendt til 3 deltagere
```

Implementér kun de aktivitetstyper, som giver mening ud fra eksisterende backend/data. Hvis projektet endnu ikke har alle typer, så lav strukturen så den nemt kan udvides senere.

---

## 4. Visning uden aktiviteter

Hvis der ikke findes nye aktiviteter, skal klik på kortet åbne en rolig statusvisning:

```text
Seneste aktivitet

Ingen nye aktiviteter siden sidst.

Alt ser fint ud.
```

Der kan eventuelt vises en lille oversigt:

```text
Ingen nye betalinger
Ingen nye beskeder
Ingen nye ordreændringer
Ingen nye invitationer
```

Dette skal give brugeren tryghed uden at skabe støj.

---

## 5. Visning med aktiviteter

Hvis der findes aktiviteter, vis dem i en liste sorteret nyeste først.

Eksempel:

```text
Seneste aktivitet

I dag
✓ Selma har betalt sin andel til Pizzaaften
✓ Jonas har valgt Burger, Cola og Ekstra mayo
✓ Biograftur mangler stadig 2 deltagere

I går
✓ Fredagsbar blev betalt
✓ Nikoline blev tilføjet til Pizzaaften
```

Hver aktivitet bør vise:

- ikon/type
- kort tekst
- relevant ordre/navn
- tidspunkt eller relativ tid hvis data findes
- læst/ulæst status hvis systemet understøtter det

---

## 6. Handlinger på aktivitetsvisningen

Hvis projektet understøtter det, skal der være en knap:

```text
Marker som læst
```

eller:

```text
Ryd nye aktiviteter
```

Dette skal kun tilføjes, hvis der findes eller let kan tilføjes en måde at markere aktiviteter/notifikationer som læst.

Hvis projektet ikke har læst/ulæst-status endnu, så implementér visningen uden denne handling og efterlad en tydelig TODO-kommentar eller et lille interface, som senere kan udvides.

---

## 7. Data må ikke hardcodes

Skærmbilleder og eksempeltekster er kun reference.

Aktiviteter skal hentes fra eksisterende datakilder, fx:

- eksisterende ordredata
- payment/payment status
- participant/order participant data
- order item/order line data
- messages/notifications hvis de findes
- audit/activity log hvis projektet har det

Hvis der ikke findes en samlet activity-feed-service, skal der oprettes et lille viewmodel/service-lag, der samler aktiviteter fra eksisterende data.

---

# Anbefalet teknisk design

## 1. Activity viewmodel

Opret eller genbrug en model/viewmodel som kan repræsentere en aktivitet.

Eksempel:

```csharp
public class ActivityItemViewModel
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? OrderId { get; set; }
    public string? OrderName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
```

Tilpas navne og typer til projektets eksisterende navngivning.

Hvis projektet er Angular/TypeScript, lav tilsvarende interface:

```typescript
export interface ActivityItemViewModel {
  id: string;
  type: string;
  title: string;
  description?: string;
  orderId?: string;
  orderName?: string;
  createdAt: string;
  isRead: boolean;
}
```

---

## 2. Activity service

Hvis der ikke findes en eksisterende service, opret en service som kan hente eller sammensætte aktivitetsdata.

Eksempler på service-metoder:

```typescript
getRecentActivities(): Observable<ActivityItemViewModel[]>
getUnreadActivityCount(): Observable<number>
markActivitiesAsRead(): Observable<void>
```

eller C#-variant afhængigt af projektets arkitektur.

---

## 3. Forside integration

Forsiden skal hente aktivitetsstatus dynamisk.

Den skal kunne vise:

### Ingen nye aktiviteter

```text
Seneste aktivitet
Ingen nye aktiviteter
```

### Nye aktiviteter

```text
2 nye aktiviteter
Selma har betalt · Jonas har valgt mad
```

eller:

```text
Seneste aktivitet
2 nye hændelser siden sidst
```

---

## 4. Navigation

Klik på kortet skal åbne `Seneste aktivitet`-visningen.

Anbefalede muligheder:

### Hvis projektet bruger routes

Opret fx:

```text
/activity
```

eller:

```text
/seneste-aktivitet
```

### Hvis projektet bruger modal/bottom sheet

Åbn en modal/bottom sheet direkte fra forsiden.

Vælg den løsning der passer bedst til eksisterende projektstruktur.

---

# Step-by-step implementering

## Step 1: Find eksisterende forside

Find komponent/page for forsiden.

Typiske navne kan være:

```text
home
frontpage
forside
dashboard
start
```

Identificér det eksisterende grønne statuskort med teksten:

```text
Du er opdateret
Ingen nye aktiviteter
```

---

## Step 2: Omdøb kortets tekst

Skift primær tekst til:

```text
Seneste aktivitet
```

Skift sekundær tekst til:

```text
Ingen nye aktiviteter
```

Dette er fallback-visningen, hvis der ikke findes nye aktiviteter.

---

## Step 3: Gør kortet dynamisk

Kortet må ikke være statisk.

Tilføj binding til activity-status, fx:

- antal ulæste aktiviteter
- seneste aktivitetstitel
- fallback hvis ingen aktiviteter

Eksempel-logik:

```text
if unreadActivityCount == 0:
    title = "Seneste aktivitet"
    subtitle = "Ingen nye aktiviteter"
else:
    title = unreadActivityCount + " nye aktiviteter"
    subtitle = short summary of latest activity
```

---

## Step 4: Find eksisterende datakilder

Undersøg om projektet allerede har:

- notification service
- message service
- activity log
- payment status
- order history
- participant status

Genbrug eksisterende data frem for at oprette parallel logik.

---

## Step 5: Opret activity-feed hvis det mangler

Hvis projektet ikke har en samlet aktivitetsservice, opret en lille service/viewmodel, som samler relevante hændelser fra eksisterende data.

Start simpelt:

- betaling gennemført
- deltager har valgt bestilling
- ordrestatus ændret
- ny besked
- ny invitation

Implementér kun det, der er realistisk med eksisterende data.

---

## Step 6: Opret visning “Seneste aktivitet”

Opret eller genbrug en visning med titel:

```text
Seneste aktivitet
```

Visningen skal have to states:

### Empty state

```text
Ingen nye aktiviteter siden sidst.
Alt ser fint ud.
```

### Activity list

Liste med seneste aktiviteter sorteret nyeste først.

---

## Step 7: Tilføj klik-handler

Når brugeren klikker på kortet, skal appen åbne aktivitetsvisningen.

Vigtigt:

- Klik må ikke bare gå til Overblik.
- Klik må ikke åbne “Deltagere der afventer”.
- Klik skal åbne aktivitetsvisningen.

---

## Step 8: Markér som læst hvis muligt

Hvis systemet understøtter læst/ulæst status, tilføj:

```text
Marker som læst
```

Når brugeren markerer som læst, skal forsiden opdatere tilbage til:

```text
Seneste aktivitet
Ingen nye aktiviteter
```

Hvis systemet ikke understøtter det, så undlad handlingen i første version.

---

## Step 9: Loading og fejltilstand

Tilføj simple states:

### Loading

```text
Henter aktivitet...
```

### Fejl

```text
Kunne ikke hente seneste aktivitet
```

Bevar appens eksisterende stil.

---

## Step 10: Test flowet

Test følgende scenarier:

1. Ingen aktiviteter
   - Forsiden viser “Seneste aktivitet / Ingen nye aktiviteter”
   - Klik åbner empty state

2. Der findes nye aktiviteter
   - Forsiden viser antal eller kort resumé
   - Klik åbner aktivitetsliste

3. Aktiviteter markeres som læst
   - Forsidekortet opdateres

4. Backend/API fejler
   - UI viser en pæn fejltilstand uden at crashe

5. Eksisterende navigation virker stadig
   - Overblik virker stadig
   - Opret virker stadig
   - Brugere virker stadig
   - Beskeder virker stadig

---

# Acceptkriterier

Implementeringen er færdig når:

- Forsiden viser kortet “Seneste aktivitet”.
- Kortet viser “Ingen nye aktiviteter”, når der ikke findes nye hændelser.
- Kortet viser dynamisk tekst, når der findes nye aktiviteter.
- Klik på kortet åbner en selvstændig “Seneste aktivitet”-visning.
- Kortet navigerer ikke bare til Overblik.
- Aktivitetsvisningen viser enten empty state eller en liste med seneste aktiviteter.
- Data kommer fra eksisterende database/API/services/viewmodels eller et nyt service-lag baseret på eksisterende data.
- Der er ingen hardcoded skærmbillede-data i produktionskoden.
- UI matcher appens mørke/neon-inspirerede stil.
- Bundnavigationen virker fortsat.
- Appen bygger uden fejl.

---

# Kort opsummering til Copilot

Implementér et rigtigt aktivitetskort på forsiden:

```text
Seneste aktivitet
Ingen nye aktiviteter
```

Kortet skal vise dynamisk status for nye hændelser og åbne en aktivitetsvisning, ikke Overblik.

