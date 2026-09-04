# Problemformulering: Merchant-ordreplatform til PayNSync

## Baggrund

PayNSync samler en gruppebetaling, hvor hver deltager vælger egne varer og godkender sin egen betaling. Pengene går direkte til merchantens Vipps MobilePay-aftale. PayNSync modtager ikke pengene og opkræver ikke betalingsgebyret.

Den nuværende Merchant Demo viser en enkel menu og kan sende deltagernes ordrekladder til PayNSync. Den viser ikke realistisk, hvordan en merchant først modtager den færdige ordre, når alle betalinger er captured.

## Problem

Mange mindre takeaway-steder har ikke egne udviklere eller kendskab til API-integration. Deres hjemmeside og ordremodtagelse drives ofte af en ekstern leverandør, eller ordrer modtages på mail, tablet eller køkkenprinter.

PayNSync kan derfor ikke basere produktet på, at hver merchant selv udvikler et nyt endpoint og mapper PayNSyncs JSON-format. Samtidig skal individuelle deltagerkladder forblive i PayNSync og må ikke frigives til køkkenet før den samlede betaling er gennemført.

## Mål

PayNSync skal kunne tilbyde flere enkle måder at modtage en færdig gruppeordre på:

- Integration med merchantens eksisterende hjemmeside via én PayNSync-knap.
- Mapping fra PayNSyncs standardordre til merchantens eksisterende ordreformat.
- En PayNSync Order Hub-webapp til iPad for merchants uden eget ordresystem.
- E-mail som selvstændig kanal eller fallback.

Merchant skal opleve den færdige gruppeordre som én normal, allerede betalt takeaway-ordre.

## Første produkt

Første salgsklare produkt er **PayNSync Order Hub** til en vejledende pris på 599 kr. pr. måned ekskl. moms. Produktet er målrettet mindre takeaway-steder, som vil modtage færdigbetalte gruppeordrer på en iPad uden integration til et eksisterende POS-system.

PayNSync tager ikke kommission af ordren. Abonnementet faktureres manuelt, og adgang til Order Hub aktiveres manuelt på merchantkontoen. Automatisk abonnementsbetaling er ikke en del af første version.

## Centrale forretningsregler

- PayNSync orkestrerer betalingerne, men modtager ikke kundernes penge.
- Merchant afregnes direkte gennem sin egen Vipps MobilePay-aftale.
- Individuelle ordrekladder sendes ikke til merchantens køkken eller ordrekø.
- Merchant modtager først én samlet ordre, når alle nødvendige betalinger er captured.
- Den færdige ordre indeholder hostens navn, telefonnummer og leveringsadresse som snapshot fra ordren.
- Deltagernes identitet og betalingsreferencer deles ikke med merchant.
- PayNSync ejer mappingen til merchantens eksisterende JSON-format.
- En mislykket ordrelevering må ikke forsvinde eller medføre en dobbeltordre.

Order Hub er første prioritet. En forbedret takeaway-demo, e-maillevering og integration til eksterne POS-systemer er efterfølgende produktudvidelser.

## Afgrænsning

Første løsning er et realistisk testmiljø og ikke et fuldt kasse- eller regnskabssystem. Den skal bevise hele flowet fra merchantens menu, over gruppebetalingen, til én betalt ordre i merchantens ordrekø.

Den første eksterne mapping kan baseres på et offentligt dokumenteret POS-format, eksempelvis et Square-inspireret ordreformat. Direkte produktionsintegration til en bestemt POS-leverandør besluttes senere.

## Succeskriterium

Løsningen er succesfuld, når flere deltagere kan bestille hver for sig, hosten kan gennemføre betalingen, og merchant derefter modtager præcis én samlet og betalt ordre via den valgte leveringskanal.
