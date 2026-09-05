# UC-22: PayNSync Order Hub og Merchant Order App

## Status

Implementeret og CI-verificeret den 5. september 2026.

## Formål

Give merchants uden eget ordresystem PayNSyncs egen ordreplatform, så de kan modtage og behandle færdige PayNSync-ordrer uden et eksternt POS-system.

## Arkitekturafgrænsning

**PayNSync Order Hub** er backend/service-capability for merchant-ordrer. Den ejer den permanente merchant-ordrekø, merchant-adgang og ordrestatus.

**Merchant Order App** er den tabletvenlige Angular/PWA-klient oven på Order Hub. Den kan installeres på en iPad, men er ikke selve Order Hub.

GroupOrder/Group Payment må ikke være afhængig af Merchant Order App. Den færdige merchant-ordre oprettes permanent efter succesfuld capture og kan herefter læses af Order Hub.

## Brugerhistorie

Som merchant uden eget ordresystem vil jeg kunne åbne PayNSync Merchant Order App på en iPad og se og behandle mine betalte ordrer, så jeg ikke behøver anskaffe et separat POS-/ordresystem.

## Funktionelt scope

### Order Hub backend

- Merchant-login genbruges fra PayNSync.
- Order Hub-adgang kan aktiveres/deaktiveres på merchantkontoen.
- Den permanente `MerchantOrder` fra PayNSync er Order Hubs source of truth.
- Order Hub eksponerer kun ordrer for den autentificerede merchant.
- Aktive ordrer kan hentes igen efter genindlæsning eller forbindelsestab.
- Statusflow: `New → Accepted → Preparing → Ready → Completed`.
- Afsluttede ordrer kan hentes som enkel historik.
- Order Hub kræver ikke et eksternt merchant-callback.

### Merchant Order App

- Tabletvenlig ordrekø.
- Vis ordrelinjer og tilvalg.
- Vis host, leveringsadresse, bemærkning, betalingsstatus og total.
- Merchant kan acceptere og ændre ordrestatus.
- Merchant kan slå alarmlyd til/fra.
- Nye ordrer giver lyd, når alarmlyd er aktiveret.
- Klienten kan installeres som PWA via den eksisterende PayNSync PWA-konfiguration.

## Acceptkriterier

- En færdig PayNSync-ordre vises kun for den relevante autentificerede merchant.
- En merchant uden aktiveret Order Hub-adgang kan ikke læse eller ændre hub-ordrer.
- Order Hub bruger den permanente merchant-ordre og afhænger ikke af et eksternt merchant-callback.
- Merchant kan acceptere ordren og følge statusflowet frem til `Completed`.
- Ugyldige statusovergange afvises.
- Genåbning efter forbindelsestab viser alle ikke-afsluttede ordrer fra databasen.
- Afsluttede ordrer vises i historik.
- Alarmlyd kan aktiveres/deaktiveres af merchant i brugergrænsefladen.
- Merchant Order App er PWA-installérbar gennem den eksisterende Angular PWA-opsætning.

## Ikke i scope

- Komplet POS-, lager- eller regnskabssystem.
- Native iPad-app.
- Automatisk abonnement, fakturering eller betalingsopkrævning.
- Printerintegration.
- Real-time push/WebSocket; første version må hente nye ordrer periodisk.
- Produktionsintegration til tredjeparts-POS.
