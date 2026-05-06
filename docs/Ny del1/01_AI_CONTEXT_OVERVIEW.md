# SBYS AI Context — Overview

## Projekt
Projektet hedder **ShareBySharePay (SBYS)**.

SBYS er en platform til gruppebetaling af take-away, hvor flere personer kan bestille og betale hver sin del, mens spisestedet får én samlet ordre.

## Kerneidé
> Bestil sammen — betal hver for sig — uden at én person lægger ud.

## Primær MVP
MVP fokuserer på take-away-løsningen, ikke restaurant/POS-integration.

## Roller

### User
En almindelig privatperson i SBYS.

Kan:
- oprette profil
- finde andre brugere
- tilføje venner
- oprette gruppebetaling
- invitere venner
- se status på gruppebetalinger

### Merchant
Et spisested/take-away sted.

Vigtigt:
- Merchant er ikke en almindelig bruger
- Merchant har egen domain model og database-tabel
- Merchant kan vises i samme directory-liste som users
- Merchant kan have et website/group order URL
- Kun oprettede merchants må bruge SBYS gruppebetaling

### Host
Brugeren der opretter gruppebetalingen.

### Participant
En inviteret bruger i en gruppebetaling.

## Forretningsmodel
- Brugere betaler ikke
- Merchants/spisesteder betaler for at være oprettet i SBYS
- SBYS er B2B2C
- SBYS er et orchestration/status-lag, ikke en betalingsudbyder

## Vigtig afgrænsning
SBYS håndterer ikke selve PSP-betalingen i MVP.

Merchant bruger sit eget betalingssystem, fx:
- Stripe
- Nets
- QuickPay
- Adyen
- Pensopay
- andet

SBYS modtager status fra merchant:
- ordre oprettet
- deltager har reserveret/betalt
- alle har betalt
- ordre frigivet
