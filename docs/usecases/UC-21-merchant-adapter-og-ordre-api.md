# UC-21: Merchant-adapter og ordre-API

## Status

Planlagt.

## Formål

Bevise at PayNSync kan mappe sin standardordre til et eksisterende merchant-format.

## Brugerhistorie

Som merchant vil jeg modtage PayNSync-ordren i det format, mit ordresystem allerede anvender, så jeg ikke skal implementere PayNSyncs interne format.

## Funktionelt scope

- Opret et simuleret merchant ordre-API.
- Definér et realistisk, Square-inspireret JSON-format.
- Implementér en isoleret PayNSync-adapter til formatet.
- Gem merchantens svar og eksterne ordrenummer.
- Adskil merchantens menu-URL fra adressen til ordrelevering.

## Acceptkriterier

- PayNSync kan mappe og sende én færdig ordre til merchant-API'et.
- Merchant-API'et modtager produkter, priser, tilvalg, host og levering korrekt.
- Samme PayNSync-ordre kan ikke oprette flere merchant-ordrer.

## Ikke i scope

- Produktionsintegration til Square eller en anden konkret POS-leverandør.
- Flere adapters i samme use case.

