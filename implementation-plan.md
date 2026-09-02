# Implementation plan — UC-03

## Forståelse

UC-03 gør den valgte merchant og valg af deltagere til wizardens første af tre trin. Merchant kommer fra UC-02 og kan ikke ændres i wizarden. Deltagere kommer dynamisk fra den eksisterende venneliste.

## Nuværende løsning

Wizarden har fire tekniske trin:

1. titel, emoji og besked;
2. merchant;
3. deltagere;
4. kontrol.

Når en merchant kommer fra forsiden, springes merchant-trinnet blot over. Værten filtreres ikke eksplicit, næste-knappen er ikke deaktiveret uden deltagere, og merchant-state valideres kun ved at kontrollere, at et ID findes i browser-state.

## Implementering

1. Ændr wizardens struktur til tre trin.
2. Gør trin 1 til **Vælg deltagere**.
3. Vis den forudvalgte merchant i et låst, kompakt kort med logo/navn og uden "Valgt", flueben, skift eller merchant-søgning.
4. Validér merchant-ID mod den aktuelle brugers dynamiske merchant-venner og brug data fra API-resultatet.
5. Indlæs kun personer fra den eksisterende venneliste.
6. Filtrér værten og den valgte merchant eksplicit, og dedupliker personer efter ID.
7. Bevar eksisterende valg ved genindlæsning og ved frem/tilbage-navigation.
8. Vis søgning, valgte markeringer og antal valgte.
9. Deaktivér **Næste**, indtil mindst én gyldig deltager er valgt.
10. Flyt de eksisterende titel/emoji/besked-felter urørte til trin 2 som midlertidig kompatibilitet med UC-04.
11. Flyt den eksisterende kontrolside urørt til trin 3 som midlertidig kompatibilitet med UC-05.
12. Bevar den eksisterende create-request og betalingsfunktionalitet.
13. Opdatér UC-03 og `docs/current-state.md` efter bestået verifikation.

## Forventede filer

- `src/Frontend.PayBySharePay/src/app/features/create-order/create-order.component.ts`
- `src/Frontend.PayBySharePay/src/app/features/create-order/create-order.component.html`
- `src/Frontend.PayBySharePay/src/app/features/create-order/create-order.component.scss`
- `src/Frontend.PayBySharePay/src/app/features/create-order/create-order.component.spec.ts`
- `docs/usecases/UC-03-wizard-step-1-participants.md`
- `docs/current-state.md`
- `implementation-plan.md`
- `test-plan.md`

## Påvirkning

- Frontend: ja.
- API: ingen kontraktændring.
- Database/migration: ingen.
- Betaling/Vipps: ingen.
- Authentication/authorization: ingen ændring; HostUserId læses fortsat fra den aktuelle session.
- Dependencies: ingen.
- Deployment: ingen workflowændring.

## Risici og afgrænsning

Fjernelsen af merchant-trinnet er tilsigtet af UC-02/03: en ny merchant vælges på forsiden. UC-04 og UC-05 implementeres ikke nu; deres eksisterende indhold flyttes kun til de korrekte trin, så det nuværende oprettelsesflow fortsat virker.
