# Kravspecifikation – PayBySharePay

## Formål

PayBySharePay er en løsning til fælles bestillinger, delt betaling og statusoverblik mellem privatpersoner og virksomheder/merchants.

Systemet skal gøre det muligt at:

- oprette og administrere ordrer
- tilføje deltagere til en ordre
- registrere beskeder og betalingsstatus
- vise oversigter og status
- arbejde med både private deltagere og merchants

## Projekter

- `Frontend.PayBySharePay`
- `Service.PayBySharePay`
- `Api.PayBySharePay`
- `DataStorage.PayBySharePay`

## Deltager-typer

En deltager kan være:

- `Person`
- `Merchant`

En merchant er også en deltager, men har ekstra oplysninger relateret til firma og betaling.

## Merchant-oplysninger

En merchant skal kunne have disse ekstra felter:

- firmanavn
- CVR/registreringsnummer
- momsnummer
- kontaktperson
- kontakt-email
- kontakttelefon
- virksomhedsadresse
- betalingsreference
- payout-/kontooplysninger
- eventuel betalingsudbyder

## Funktionelle krav

### 1. Forside
Systemet skal have en mobilvenlig forside med handlingskort og navigation til:
- overblik
- opret
- find deltagere
- beskeder

### 2. Find deltagere
Systemet skal kunne:
- søge i deltagere
- vise personer og merchants i samme resultatliste
- markere deltager-type visuelt
- tilføje ven
- vælge deltagere til ordre

### 3. Opret ordre
Systemet skal kunne:
- oprette ordre med titel/kategori
- tilføje besked
- vælge deltagere
- gemme ordren med status

### 4. Ordreoverblik
Systemet skal kunne:
- vise deltagere
- vise deltagerstatus
- vise betalingsstatus
- vise besked og valgt ordretype
- have handling til betaling

### 5. Betaling
Systemet skal kunne:
- registrere betaling
- gemme betalingsstatus pr. deltager
- håndtere merchant-relaterede betalingsoplysninger

### 6. Beskeder
Systemet skal kunne:
- hente beskeder
- oprette beskeder
- knytte beskeder til ordrer

## Ikke-funktionelle krav

- Frontend skal være Angular + TypeScript
- Backend skal være .NET 9 Web API
- EF Core og SQL Server skal bruges
- UI skal være mobil-først
- Layout skal passe til iPhone 14
- API må ikke indeholde forretningslogik
- Service-laget skal indeholde forretningslogik
- Repository-laget må kun håndtere dataadgang

## Domænekrav

Systemet skal som minimum understøtte disse entiteter:

- Participant
- FriendRelation
- Order
- OrderParticipant
- Payment
- Message

`Participant` skal kunne rumme både person og merchant.

## Valideringsregler

- En person skal have navn
- En merchant skal have firmanavn
- En merchant skal have relevante betalingsoplysninger, hvis de kræves i flowet
- En bruger må ikke tilføje sig selv som ven
- En ordre skal have titel eller kategori
- En betaling skal have gyldigt beløb større end 0
- En besked må ikke være tom

## API-krav

API'et skal som minimum understøtte:
- søg deltagere
- opret person
- opret merchant
- opret venrelation
- opret og hent ordrer
- hent ordreoverblik
- registrer betaling
- hent og opret beskeder

## Definition of done

Løsningen er færdig når:
- backend og frontend kan bygges
- SQL Server kan bruges via EF Core
- API er koblet til service- og repository-lag
- centrale flows virker
- UI matcher designreglerne
- både person og merchant understøttes som deltagere
- dokumentation er opdateret
