# UC-18: Brug leveringsadresse på gruppeordren

## Status

✅ Implementeret.

## Formål

Sikre at den leveringsadresse, som gælder når værten opretter en gruppeordre, følger den konkrete ordre hele vejen til merchant, så køkken/bud ved hvor den færdige ordre skal afleveres.

UC-18 bygger videre på UC-17, hvor en personkonto kan gemme Adresse, Postnr., By og Land på profilen.

## Brugerhistorie

Som vært vil jeg have min gemte leveringsadresse knyttet til den gruppeordre, jeg opretter, så merchant kan bruge den korrekte afleveringsadresse, selv hvis jeg senere ændrer min profil.

## Funktionelt scope

### 1. Snapshot ved ordreoprettelse

Når en gruppeordre oprettes, kopieres følgende felter fra den oprettende persons profil til selve ordren:

- Adresse
- Postnr.
- By
- Land

Felterne gemmes som et snapshot på ordren og er uafhængige af senere ændringer på Participant-profilen.

### 2. Historisk korrekthed

Hvis værten ændrer sin profiladresse efter ordreoprettelsen, må en allerede oprettet ordres leveringsadresse ikke ændre sig.

Nye ordrer bruger den adresse, der er gemt på profilen på tidspunktet for den nye ordre.

### 3. Merchant callback

Når hele gruppebetalingen er captured og PayNSync sender `GroupOrderPaid` til merchant, indeholder callback-payloaden et `deliveryAddress`-objekt med ordre-snapshot'et:

```json
{
  "deliveryAddress": {
    "address": "Vestergade 12, 2. th.",
    "postalCode": "8000",
    "city": "Aarhus C",
    "country": "Danmark"
  }
}
```

Merchant skal dermed ikke hente den aktuelle brugerprofil for at finde afleveringsadressen.

### 4. Manglende adresse

UC-18 gør ikke leveringsadressen obligatorisk ved ordreoprettelse. Hvis værten ikke har udfyldt en leveringsadresse, kan ordren fortsat oprettes, og callbacken kan mangle `deliveryAddress`.

Et eventuelt krav om komplet adresse før en bestemt leveringsordre oprettes håndteres i en separat use case, så eksisterende gruppebetalingsflows ikke brydes.

## Datamodel

`Order` udvides med nullable snapshot-felter:

- `DeliveryAddress`
- `DeliveryPostalCode`
- `DeliveryCity`
- `DeliveryCountry`

Der tilføjes en EF Core migration til `Orders`-tabellen.

## Acceptkriterier

### AC1 — Adresse kopieres ved oprettelse

**Givet** en vært har en gemt leveringsadresse på sin profil  
**Når** værten opretter en gruppeordre  
**Så** gemmes adressen som snapshot på ordren.

### AC2 — Eksisterende ordre ændres ikke

**Givet** en ordre er oprettet med et adresse-snapshot  
**Når** værten efterfølgende ændrer sin profiladresse  
**Så** bevarer ordren den oprindelige leveringsadresse.

### AC3 — Ny ordre bruger ny profiladresse

**Givet** værten har ændret sin profiladresse  
**Når** værten opretter en ny ordre  
**Så** bruger den nye ordre den nye profiladresse.

### AC4 — Merchant modtager leveringsadressen

**Givet** ordren har et adresse-snapshot  
**Når** den samlede betaling er captured og `GroupOrderPaid` sendes  
**Så** indeholder callbacken `deliveryAddress` med Adresse, Postnr., By og Land fra ordren.

### AC5 — Callback bruger ikke live profildata

**Givet** værten har ændret profilen efter ordreoprettelsen  
**Når** merchant-callbacken sendes  
**Så** anvendes ordre-snapshot'et og ikke den aktuelle profiladresse.

### AC6 — Manglende adresse bryder ikke eksisterende flow

**Givet** en vært ikke har udfyldt leveringsadresse  
**Når** en gruppeordre oprettes og betales  
**Så** fortsætter det eksisterende betalingsflow uden fejl.

## Ikke i scope

- Redigering af leveringsadressen direkte på en eksisterende ordre.
- Flere leveringsadresser pr. bruger.
- GPS/geokodning eller validering mod ekstern adressetjeneste.
- Leveringspris eller afstandsberegning.
- Krav om komplet adresse før ordreoprettelse.

## Definition of Done

- Order har fire leveringsadresse-snapshotfelter.
- Database-migration er tilføjet.
- Adresse kopieres fra værten ved oprettelse af ordren.
- `GroupOrderPaid` indeholder snapshot-adressen, når den findes.
- Senere ændring af profilen påvirker ikke eksisterende ordrer.
- Eksisterende betalingsflow og callback-fejlhåndtering bevares.
- .NET build og tests er grønne.
