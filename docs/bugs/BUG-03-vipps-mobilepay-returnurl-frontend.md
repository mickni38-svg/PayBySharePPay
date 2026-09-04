# BUG-03 — Vipps/MobilePay returnUrl sender brugeren til API-domænet

**Status:** Åben  
**Prioritet:** 🔴 Høj  
**Opdaget:** 2026-09-04  
**Komponent:** MerchantOrderService / Vipps/MobilePay payment return / Angular frontend

## Problem

Efter en gennemført Vipps/MobilePay-betaling lander brugeren på `api.paynsync.dk`.

Den eksisterende kode bygger return URL ud fra API-base-URL'en:

`{ApiBaseUrl}/payment-return`

I produktion bliver det derfor:

`https://api.paynsync.dk/payment-return`

På iPhone/Safari resulterer dette aktuelt i en tom API-side og download/visning af `payment-return.txt`. Det giver en dårlig brugeroplevelse og efterlader brugeren uden for PayNSyncs mobilfrontend.

## Ønsket adfærd

Efter Vipps/MobilePay-flowet skal brugeren returneres til PayNSyncs Angular-frontend på `mobil.paynsync.dk`.

Foretrukken return URL:

`https://mobil.paynsync.dk/payment-return?reference={reference}`

Alternativt kan frontend efter behandling navigere videre til:

`https://mobil.paynsync.dk/home`

Return-siden skal vise en kort status, fx at betalingen behandles eller er registreret, og derefter give brugeren adgang til PayNSync igen.

## Vigtig forretningsregel

Browser-redirectet må ikke bruges som bevis på, at betalingen er gennemført.

Betalingsstatus skal komme fra backend/Vipps-webhook eller et eksplicit statusopslag. Return-siden må derfor gerne vise "Vi kontrollerer din betaling..." indtil backend har den endelige status.

## Foreslået teknisk ændring

`MerchantOrderService` skal ikke bygge `returnUrl` fra `ApiBaseUrl`.

Den skal i stedet bruge den allerede eksisterende frontend-konfiguration, fx:

`AppSettings:FrontendUrl = https://mobil.paynsync.dk`

og bygge:

`{FrontendUrl}/payment-return?reference=...`

Angular skal have en route/component til `/payment-return`.

Siden skal kunne hente eller polle den aktuelle betalingsstatus fra API'et med en ikke-følsom reference og derefter tilbyde eller automatisk navigere til `/home`.

## Acceptance Criteria

1. En Vipps/MobilePay-betaling i produktion/test får en `returnUrl` på `mobil.paynsync.dk`, ikke `api.paynsync.dk`.
2. Efter betaling åbnes PayNSyncs Angular-frontend i browseren.
3. Brugeren får ikke længere en `payment-return.txt` download.
4. `/payment-return` virker ved direkte reload og deep-link på Simply.com.
5. Return-siden kan håndtere mindst statusserne: afventer, reserveret/betalt, annulleret og fejl.
6. Return-siden markerer aldrig en betaling som gennemført alene fordi browseren blev redirected.
7. Når status er kendt, kan brugeren fortsætte til `/home`.
8. Reference/query-parametre må ikke indeholde credentials, access tokens eller andre hemmeligheder.
9. Frontend- og backendtests mocker Vipps/MobilePay og laver ingen live betalingskald.
10. Eksisterende Fake/Sandbox payment-flow må ikke brydes.

## Testscenarier

- Gennemført betaling → return til `mobil.paynsync.dk/payment-return` → status vises → videre til Home.
- Betaling annulleret i Vipps/MobilePay → return-side viser annulleret.
- Webhook er lidt forsinket → return-side viser afventer og opdaterer, når backend modtager status.
- Brugeren reloader return-siden → siden kan stadig hente korrekt status.
- Brugeren åbner return URL igen senere → ingen dobbelt betaling eller dobbelt statusændring.

## Relateret

BUG-02 beskriver det server-side webhook-flow, som skal være den autoritative kilde til betalingsstatus.
