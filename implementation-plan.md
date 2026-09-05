# Implementation plan — UC-19

## Task

- Use case: `docs/usecases/UC-19-samlet-merchant-ordrekontrakt.md`
- Primary task type: `NEW_USE_CASE`
- Goal: Opret én permanent, samlet og betalt merchant-ordre efter fuld capture som grundlag for PayNSync Order Hub.
- Approval: Product Owner har bestemt, at UC-19 har forrang ved konflikt med eksisterende regler og kontrakter.

## Current state

- `GroupPaymentOrchestrationService` sætter allerede gruppeordren til `Paid` efter alle captures og bygger derefter en flygtig `PayNSyncFinalGroupOrderDto` til HTTP-callback.
- Den eksisterende callback-payload grupperer ordrelinjer pr. deltager og indeholder deltagernavne, participant-id'er og provider-betalingsreferencer.
- UC-19 kræver en kanonisk merchant-ordre uden deltageridentitet og betalingsreferencer.
- `Order` indeholder allerede snapshot af leveringsadressen. Hostens navn og telefon findes via `CreatedByParticipantId`/`OrderParticipants`.
- Der findes ingen permanent final `MerchantOrder`; kun deltagerens `MerchantOrderDraft` og tilhørende draft-linjer.

## Constraints

- Architecture: Orkestreringen kalder en service, som ejer opbygningen, mens et entity-specifikt repository ejer persistence.
- Business rules: Final merchant-ordre oprettes kun, når alle relevante betalinger er `Captured`; én gruppeordre giver højst én merchant-ordre.
- Security/privacy: Den permanente ordre og callbacken må ikke indeholde deltageridentitet, participant tokens, provider-referencer eller credentials.
- Compatibility: UC-19 har Product Owner-godkendt forrang. Den eksisterende callback-kontrakt ændres derfor til UC-19-formatet, selv om det er en breaking contract change.

## Impact

### Backend

- Ny `MerchantOrder`-entitet med PayNSync-ordrenummer, source order-id, merchant-id, host-snapshot, leveringssnapshot, total, valuta, betalingsstatus og tidsstempler.
- Ny `MerchantOrderItem`-entitet. Draft-linjer kopieres én-til-én uden deltagerrelation, så særskilte linjer ikke slås sammen.
- Nyt `IMerchantOrderRepository`/`MerchantOrderRepository`.
- Nyt `IMerchantOrderFinalizationService`/`MerchantOrderFinalizationService` med validering og idempotent oprettelse.
- `GroupPaymentOrchestrationService` finaliserer efter fuld capture og sender derefter den persistente standardordre som callback.
- Gentaget `/approve` på en allerede `Paid` ordre sikrer idempotent, at merchant-ordren findes uden nyt capture.

### DTO/API

- `PayNSyncFinalGroupOrderDto` ændres fra participant-gruppering til én flad samlet ordre med host, levering og linjer.
- Deltager-id, deltagernavn, payment status pr. deltager og provider payment-id fjernes fra callback-kontrakten.
- Ingen nye endpoints i UC-19.

### Frontend

- Ingen ændringer. Order Hub-visning tilhører UC-22.

### Database

- Nye tabeller `MerchantOrders` og `MerchantOrderItems`.
- Unikt index på `MerchantOrders.SourceOrderId` garanterer højst én permanent merchant-ordre pr. gruppeordre.
- Decimalfelter bruger precision `(18,2)`.
- Relationer bruger restriktiv delete-adfærd, så en betalt merchant-ordre ikke slettes med source order eller merchant.
- Ny EF Core-migration og opdateret model snapshot; ingen backfill.

### Payment/integration

- Ingen ændring af providerkald eller payment state machine.
- Finalisering kræver captured betaling for alle ordrepersoner, én valuta og overensstemmelse mellem kopierede linjer og captured beløb.
- Dataintegritetsfejl opretter ikke merchant-ordre og ruller ikke captured betalinger tilbage.
- HTTP callback sendes først efter vellykket persistent finalisering.

### Deployment

- Produktionsdatabasen skal migreres før den nye backend tages i brug.
- Ingen nye dependencies, secrets eller konfigurationsnøgler.

## Steps

1. Tilføj final merchant-order entities, relationer, constraints og migration.
2. Tilføj repository og finalization service med idempotent oprettelse og privacy-sikker mapping.
3. Integrér finalization service i successful capture og allerede-Paid retry-flowet.
4. Opdatér callback-kontrakten til UC-19-formatet.
5. Tilføj tests for timing, mapping, snapshots, total/valuta, idempotens og partial failure.
6. Kør .NET build/tests, gennemgå migration/snapshot og foretag separat review.
7. Opdatér UC-19 og relevante dokumentationsfiler efter grøn verification.

## Risks

| Risk | Impact | Mitigation |
|---|---|---|
| Callback-kontrakten ændres | Eksisterende merchant-modtager skal forstå det nye format | UC-19 har forrang; kontrakten dokumenteres og testes eksplicit |
| Capture lykkes, men permanent ordre kan ikke gemmes | Betalt ordre mangler i Order Hub | Idempotent finalisering ved completion og gentaget `/approve`; robust automatisk retry tilhører UC-24 |
| Samtidige finaliseringsforsøg | Dubletordre | Unikt database-index og idempotent lookup |
| Draft-linjer og captured beløb er inkonsistente | Forkert ordretotal | Valider summer/valuta og afvis finalisering uden at ændre betalingsstatus |
| Host ændrer profil efter finalisering | Historisk ordre ændrer kontaktdata | Kopiér navn, telefon og adresse til merchant-ordren som snapshot |

## Out of scope

- Order Hub Angular-app, ordrekø og merchant-statusflow (UC-22).
- Automatisk retry/outbox og manuel genudsendelse (UC-24).
- Merchant-specifik mapping/API (UC-21).
- E-mail og andre leveringskanaler (UC-23).
