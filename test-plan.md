# Test Plan — UC-22 Order Hub

## Backend
- Kun merchant kan bruge Order Hub.
- OrderHubEnabled kræves.
- Kø filtrerer på autentificeret MerchantParticipantId.
- Aktive og Completed adskilles korrekt.
- Gyldige statusovergange accepteres.
- Ugyldige statusovergange afvises.
- Merchant kan ikke opdatere en anden merchants ordre.
- Aktivering/deaktivering persisteres.

## Frontend
- Merchant route loader komponenten.
- Aktive ordrer og historik vises fra API.
- Statusændring opdaterer listen.
- Alarm-toggle persisteres lokalt.
- Ny ordre under polling udløser lyd kun når alarm er slået til.

## Verification
- dotnet build + tests
- Angular test + Simply build
- migration/snapshot review
