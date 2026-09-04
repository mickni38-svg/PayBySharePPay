# BUG-02 — Vipps/MobilePay webhook skal være produktionsklar og verificeret

**Status:** Åben  
**Prioritet:** 🔴 Høj  
**Opdaget:** 2026-09-04  
**Komponent:** Vipps/MobilePay ePayment / webhook callbacks / Payment API

## Problem

PayNSync har allerede callback-endpoints og sender en `webhookUrl` til Vipps/MobilePay ved oprettelse af betalinger, men webhook-flowet er ikke dokumenteret som fuldt verificeret i produktion.

I Vipps/MobilePay-portalen vises der aktuelt ingen registrerede webhooks. Dette er ikke i sig selv bevis på, at de per-payment callback-URL'er ikke virker, fordi PayNSync i dag sender en webhook-URL sammen med den enkelte betaling. Det skal derfor afklares og testes, om det eksisterende ePayment webhook-flow er korrekt for den valgte Vipps/MobilePay-integration, eller om PayNSync også skal registrere webhook via Vipps Webhooks API.

Den eksisterende dokumentation angiver desuden, at webhook-signaturvalidering ikke er implementeret.

## Nuværende flow

Ved oprettelse af en deltagerbetaling bygger backend callback-base:

`https://api.paynsync.dk/api/payments/vipps/callbacks`

Payment-provideren opretter derefter en callback-URL på formen:

`POST /api/payments/vipps/callbacks/{participantPaymentId}`

Webhook-eventet bruges til at opdatere betalingsstatus, fx Reserved, Captured eller Cancelled.

## Ønsket adfærd

PayNSync skal have ét entydigt og produktionsklart webhook-flow for Vipps/MobilePay.

Backend skal kunne modtage betalingshændelser uden at være afhængig af, om brugerens browser vender tilbage til PayNSync efter betaling.

Webhooken skal kunne kobles sikkert til den korrekte `ParticipantPayment` via Vipps-reference/provider payment id eller anden entydig reference.

Webhook-hændelser skal kunne behandles idempotent, så samme event kan modtages flere gange uden dobbelt statusændring eller dobbelt handling.

Webhookens autenticitet/signatur skal valideres efter den aktuelle Vipps/MobilePay-specifikation.

## Afklaring før implementering

Det skal først verificeres i den aktuelle Vipps/MobilePay ePayment-dokumentation om PayNSyncs eksisterende `webhookUrl` pr. betaling er den korrekte anbefalede mekanisme, eller om der også/alternativt skal oprettes en webhook via Webhooks API.

Vi må ikke ændre integrationen alene ud fra, at webhook-listen i portalen er tom.

## Acceptance Criteria

1. En rigtig Vipps/MobilePay testbetaling udløser et webhook-event til PayNSyncs produktions-/testendpoint.
2. Eventet kan entydigt kobles til den korrekte `ParticipantPayment`.
3. Status i databasen opdateres korrekt på baggrund af webhook-eventet.
4. Gentaget levering af samme event giver ikke dobbelt behandling.
5. Webhookens autenticitet valideres i henhold til Vipps/MobilePays aktuelle dokumentation.
6. Fejl i webhook-behandling logges med reference/event-id uden at logge hemmelige credentials eller unødvendige persondata.
7. Brugerens `returnUrl` er ikke nødvendig for at betalingsstatus bliver korrekt.
8. Der findes automatiske tests af webhook-flowet uden live kald til Vipps/MobilePay.
9. Dokumentationen beskriver, om løsningen bruger per-payment `webhookUrl`, Webhooks API eller begge dele, og hvorfor.
10. Flowet er verificeret med mindst én testbetaling i Vipps/MobilePay testmiljø.

## Ikke omfattet

Denne bug ændrer ikke den side, brugeren lander på efter betaling. Det håndteres separat i BUG-03.
