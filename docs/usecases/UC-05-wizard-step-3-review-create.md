# FEATURE 05: Wizard trin 3 – kontrol og oprettelse

## Mål

Værten kontrollerer dynamiske oplysninger fra wizard-state og opretter gruppebetalingen gennem den eksisterende backendfunktionalitet.

## Instruks til Copilot Claude

Analysér først den eksisterende oprettelsesfunktion, requestkontrakt, backendvalidering, invitationer, idempotens og fejlhåndtering. Implementér kun denne feature og bevar det nuværende betalingsflow. Bevar arkitektur og kodestil, kør relevante tests, og afslut med ændrede filer samt testresultat.

## Forudsætning

`04-FEATURE-wizard-step-2-details.md` er implementeret, og wizard-state indeholder gyldig merchant, mindst én deltager og en gyldig titel.

## Designreference

Følg mockupfilen `375F42F8-535D-42F5-B23A-7E6378AA2718.jpeg`.

Merchant, deltagere, titel og besked i mockuppen er kun eksempler og må ikke hardcodes.

## Kontrolside

Vis data fra wizard-state:

- sideoverskriften **Kontrollér og opret**, hjælpeteksten og trinindikatoren 3 af 3 som i mockuppen;
- merchantens rigtige logo og navn;
- titel;
- besked eller **Ingen besked**, hvis den er tom;
- antal valgte deltagere;
- liste over valgte deltagere.

Der må ikke udføres nye opslag med hardcodede ID'er. Hvis den eksisterende løsning genindlæser eller validerer data gennem API'et, skal den eksisterende mekanisme genbruges.

## Redigering

- Redigering af titel eller besked navigerer til trin 2.
- Redigering af deltagere navigerer til trin 1.
- Alle øvrige wizard-data skal bevares.
- Merchant kan ikke redigeres i wizarden; en anden merchant kræver tilbagevenden til forsiden og et nyt flow.

## Oprettelse

Den primære knap hedder **Opret gruppebetaling** og skal bruge den allerede implementerede oprettelsesfunktion, service og backendkommando.

Klienten sender som minimum de eksisterende kontraktfelter svarende til:

| Felt | Indhold |
| --- | --- |
| `HostUserId` | Aktuel bruger fra session/auth-kontekst |
| `MerchantId` | Merchant valgt på forsiden |
| `Title` | Valideret titel |
| `Message` | Besked eller `null` |
| `Participants` | Valgte deltager-ID'er |

Backend har ansvaret for servergenererede værdier som status og oprettelsestidspunkt.

## Forretningsregler og backendvalidering

- Præcis én aktiv merchant.
- Værten har en gyldig relation til merchant-gruppen.
- Mindst én anden deltager end værten.
- Værten må ikke forekomme blandt deltagerne.
- Den valgte merchant må ikke forekomme blandt deltagerne.
- En deltager må kun forekomme én gang.
- Titel er påkrævet og højst 80 tegn.
- Besked er valgfri og højst 500 tegn.
- Merchant- og deltagerrelationer valideres igen ved oprettelse.
- Oprettelsen er atomisk sammen med de nødvendige invitationer/anmodninger.
- Dobbeltklik eller gentagne kald med samme idempotency key må ikke oprette dubletter.
- Eksisterende betalings-, reservation- og capture-flow må ikke ændres.

## Brugeroplevelse

- Brug samme sorte theme, smalle indholdsbredde og blå accent som de øvrige sider.
- Vis redigeringshandlinger ved de relevante sektioner som i mockuppen.
- Vis **Klar til oprettelse**, når state er gyldig.
- Deaktivér oprettelsesknappen under behandling.
- Ved succes navigeres til den eksisterende detaljeside for den nye gruppebetaling.
- Ved fejl bevares wizard-state, knappen aktiveres igen, og en forståelig fejl vises.

## Acceptkriterier

### AC1 – Dynamisk kontrolside

**Givet** gyldig wizard-state  
**Når** trin 3 åbnes  
**Så** vises korrekt merchant, titel, besked og deltagere uden hardcodede data.

### AC2 – Redigering

**Givet** kontrolsiden  
**Når** brugeren redigerer deltagere eller detaljer  
**Så** åbnes det relevante trin, og øvrige data bevares.

### AC3 – Opret én gang

**Givet** gyldige data  
**Når** brugeren trykker på **Opret gruppebetaling**  
**Så** sendes én kommando gennem den eksisterende oprettelsesfunktion  
**Og** dobbeltklik kan ikke oprette en dublet.

### AC4 – Backendvalidering

**Givet** en manipuleret request med vært eller merchant som deltager  
**Når** backend modtager requesten  
**Så** afvises den uden delvis oprettelse.

### AC5 – Succes og fejl

**Givet** en gyldig oprettelse  
**Så** oprettes invitationer og brugeren sendes til detaljesiden  
**Givet** en fejl  
**Så** bevares wizardens data, og en forståelig fejl vises.

## Test

- Korrekt visning af dynamisk merchant, titel, besked og deltagere.
- Tom besked viser **Ingen besked**.
- Redigeringsnavigation uden datatab.
- Dobbeltklik og gentaget idempotent API-kald.
- Backend afviser vært, merchant, dublet eller inaktiv relation som deltager.
- Atomisk rollback ved fejl under oprettelse af invitationer.
- Succesnavigation og fejltilstand.

## Ikke en del af denne feature

- Nyt betalings-, reservations- eller capture-flow.
- Historiske snapshots af merchant-logo.
- Nye venskabsanmodningsregler.
