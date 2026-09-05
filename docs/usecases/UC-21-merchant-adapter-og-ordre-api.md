# UC-21: Merchant-adapter og ordre-API

## Status

Implementeret og verificeret via CI den 5. september 2026.

## Formål

Bevise at PayNSync som en senere PayNSync Pro-udvidelse kan mappe sin standardordre til et eksisterende merchant-format.

## Brugerhistorie

Som merchant vil jeg modtage PayNSync-ordren i det format, mit ordresystem allerede anvender, så jeg ikke skal implementere PayNSyncs interne format.

## Funktionelt scope

- Opret et simuleret merchant ordre-API.
- Definér et realistisk, Square-inspireret JSON-format.
- Implementér en isoleret PayNSync-adapter til formatet.
- Lad PayNSync eje mappingen, så merchant ikke skal implementere PayNSyncs standardpayload.
- Gem merchantens svar og eksterne ordrenummer.
- Adskil merchantens menu-URL fra adressen til ordrelevering.

## Acceptkriterier

- PayNSync kan mappe og sende én færdig ordre til merchant-API'et.
- Merchant-API'et modtager produkter, priser, tilvalg, host og levering korrekt.
- Samme PayNSync-ordre kan ikke oprette flere merchant-ordrer.

## Implementeringsnoter

- `Participant.GroupOrderUrl` er fortsat kundevendt menu-/bestillings-URL.
- `Participant.MerchantOrderUrl` er det separate endpoint til den færdige ordre.
- `SquareInspiredMerchantOrderAdapter` mapper den privacy-safe `PayNSyncFinalGroupOrderDto` til det simulerede merchant-format.
- `MerchantOrder.ExternalOrderNumber` og `ExternalResponseJson` gemmer merchantens svar.
- `MerchantOrderItem.ModifiersJson` bevarer strukturerede tilvalg fra merchantens draft.
- Det simulerede ordre-API er Development-only og idempotent pr. PayNSync ordre.
- Leveringsfejl ruller ikke captured betalinger tilbage.

## Ikke i scope

- Produktionsintegration til Square eller en anden konkret POS-leverandør.
- Flere adapters i samme use case.
- Den første Order Hub-pakke.
