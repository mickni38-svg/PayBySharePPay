# Implementation Plan — UC-21 Merchant-adapter og ordre-API

## Scope
Implementér én isoleret Square-inspireret merchant-adapter oven på UC-19's permanente final group order.

## Design
1. Bevar `GroupOrderUrl` som menu-/bestillings-URL.
2. Tilføj `MerchantOrderUrl` som separat destination for færdige merchant-ordrer.
3. Bevar UC-19's permanente `MerchantOrder` som idempotent source of truth.
4. Bevar strukturerede modifiers fra `RawMerchantPayloadJson` i den permanente merchant-ordre.
5. Map `PayNSyncFinalGroupOrderDto` til ét Square-inspireret request-format.
6. Send request via eksisterende merchant callback-boundary, men til `MerchantOrderUrl`.
7. Gem merchantens rå svar og eksterne ordrenummer på `MerchantOrder`.
8. Tilføj et Development-only simuleret merchant ordre-API med deterministisk idempotens.
9. Opdater dokumentation og UC-status efter verificering.

## Database impact
Ny migration med nullable felter:
- `Participant.MerchantOrderUrl`
- `MerchantOrder.ExternalOrderNumber`
- `MerchantOrder.ExternalResponseJson`
- `MerchantOrderItem.ModifiersJson`

Nullable felter bevarer bagudkompatibilitet for eksisterende rækker.

## Security
Det simulerede merchant API må kun registreres i Development. Ingen secrets eller betalingsreferencer sendes i adapter-payloaden.

## Out of scope
- Produktionsintegration til Square/OrderYOYO/andre konkrete POS-systemer.
- Flere adaptertyper.
- Order Hub UI.
- Retry/SLA ud over idempotens i denne use case.
