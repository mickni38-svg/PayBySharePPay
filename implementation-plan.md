# Implementation plan — UC-04

## Forståelse

UC-04 gør wizardens trin 2 til siden **Detaljer**. Værten angiver en påkrævet titel og en valgfri besked. Merchant og deltagere kommer fra den eksisterende UC-03 wizard-state, og oplysningerne skal bevares ved frem/tilbage-navigation.

## Nuværende løsning

Wizarden har allerede tre trin efter UC-03, men trin 2 består stadig af de gamle felter titel, obligatorisk emoji og en besked begrænset til 200 tegn. Titel og besked ligger uden for den samlede wizard-state, der mangler tegntællere og dynamisk opsummeringskort, og valideringen kræver fortsat emoji.

API- og databasemodellen accepterer allerede en valgfri besked uden en 200-tegnsbegrænsning. `Category` er valgfri i create-kontrakten, så emoji kan fjernes fra trin 2 uden API-, database- eller migrationsændringer.

## Implementering

1. Udvid den eksisterende `CreateOrderWizardState` med titel og besked; opret ikke en parallel state-løsning.
2. Gør trin 2 til **Detaljer** med hjælpetekst og eksisterende trinindikator 2 af 3.
3. Erstat de gamle felter med:
   - **Titel**, placeholder **Fx Pizzaaften**, påkrævet, maks. 80 tegn og synlig tegntæller.
   - **Besked**, placeholder **Skriv en besked til deltagerne...**, valgfri, maks. 500 tegn og synlig tegntæller.
4. Fjern emoji som obligatorisk felt og valideringskrav. Den eksisterende valgfrie `category`-egenskab sendes fortsat kompatibelt som `undefined`.
5. Trim kun titel ved validering og før den gemmes i wizard-state. Bevar beskedens præcise tekst, linjeskift, danske tegn og emoji.
6. Deaktivér **Næste**, når titlen efter trim er tom eller over 80 tegn, når beskeden er over 500 tegn, eller når UC-03-state mangler gyldig merchant/deltager.
7. Vis et kompakt, dynamisk opsummeringskort med merchantens validerede navn/logo og aktuelt deltagerantal.
8. Bevar titel, besked, merchant og deltagere ved navigation tilbage til trin 1 og frem til trin 2 igen.
9. Beskyt trin 2 mod ugyldig wizard-state via den eksisterende komponentnavigation: tilbage til trin 1, eller forsiden hvis merchant-state ikke længere er gyldig.
10. Lad trin 3 og den endelige oprettelse være funktionelt uændret; UC-05 implementeres ikke.
11. Opdatér UC-04 og `docs/current-state.md` efter bestået verifikation.

## Forventede filer

- `src/Frontend.PayBySharePay/src/app/features/create-order/create-order.component.ts`
- `src/Frontend.PayBySharePay/src/app/features/create-order/create-order.component.html`
- `src/Frontend.PayBySharePay/src/app/features/create-order/create-order.component.scss`
- `src/Frontend.PayBySharePay/src/app/features/create-order/create-order.component.spec.ts`
- `docs/usecases/UC-04-wizard-step-2-details.md`
- `docs/current-state.md`
- `implementation-plan.md`
- `test-plan.md`

## Påvirkning

- Frontend: ja.
- API: ingen kontraktændring.
- Database/migration: ingen.
- Betaling/Vipps: ingen.
- Authentication/authorization: ingen.
- Dependencies: ingen.
- Deployment: ingen workflowændring.

## Risici og afgrænsning

Trin 3 viser indtil UC-05 den eksisterende kontrolside. Emoji fjernes fra trin 2, fordi UC-04 ikke indeholder kategorivalg, og backend-kontrakten allerede gør `category` valgfri. Create-request, invitationer og betalingsflow ændres ikke.
