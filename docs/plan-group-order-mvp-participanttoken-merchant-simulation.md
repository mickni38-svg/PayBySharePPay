# 🎯 Group Order MVP — ParticipantToken + Merchant Simulation

## Understanding
Implement the complete group-order MVP flow: add `ParticipantToken` per `OrderParticipant`, support per-participant merchant draft submission via token, `ReadyToPay` orchestration, `CompleteOrder` endpoint, update dummy merchant HTML, and expand seed with friends, merchants, and two group orders for participants 1 and 2.

## Assumptions
- Flush existing data before re-seeding
- `MerchantOrderDraft` gets nullable `ParticipantId` to allow one draft per participant per order
- Merchant is NOT added as `OrderParticipant` — existing `nonMerchantParticipants` filter handles this; seed data should be fixed accordingly
- `ParticipantToken` is a `Guid` generated per `OrderParticipant` at order creation
- `ReadyToPay` is triggered automatically when all non-merchant `OrderParticipant`s have status `"OrderSubmitted"`
- `CompleteOrder` is host-only (validate `createdByParticipantId`)
- Seed: participant id 1 = test1@dev.dk (Anders Nielsen), id 2 = test2@dev.dk (Mette Christensen) — but actual ids depend on seed order; seed will reference by email

## Key Files
- `src/DataStorage.PayBySharePay/Entities/OrderParticipant.cs` — add ParticipantToken
- `src/DataStorage.PayBySharePay/Entities/MerchantOrderDraft.cs` — add ParticipantId
- `src/DataStorage.PayBySharePay/Context/PayBySharePayDbContext.cs` — unique index on ParticipantToken
- `src/Service.PayBySharePay/Services/OrderService.cs` — token generation, URL format, CompleteOrderAsync, ReadyToPay check
- `src/Service.PayBySharePay/Interfaces/IOrderService.cs` — add CompleteOrderAsync
- `src/Service.PayBySharePay/Services/MerchantOrderService.cs` — token validation, status update, ReadyToPay trigger
- `src/Service.PayBySharePay/DTOs/` — new request/response DTOs
- `src/Api.PayBySharePay/DTOs/` — update InitMerchantOrderRequest, add CompleteOrderRequest
- `src/Api.PayBySharePay/Controllers/OrdersController.cs` — add CompleteOrder endpoint
- `src/Api.PayBySharePay/Controllers/MerchantOrdersController.cs` — pass participantToken
- `src/Frontend.MerchantDemo/index.html` — read & send participantToken
- `src/Tools.PayBySharePay/Program.cs` — expand seed

**Progress**: 100% [██████████]

**Last Updated**: 2026-05-16 14:57:45

## 📝 Plan Steps
- ✅ **Add `ParticipantToken` to `OrderParticipant` entity and `ParticipantId` to `MerchantOrderDraft` entity**
- ✅ **Update `DbContext` with unique index on `OrderParticipant.ParticipantToken` and FK for `MerchantOrderDraft.ParticipantId`**
- ✅ **Run EF Core migration**
- ✅ **Update `OrderService.CreateOrderAsync` — generate token per participant, fix URL format, add `CompleteOrderAsync` + `GetReadyToPayStatusAsync`**
- ✅ **Update `IOrderService` interface to expose new methods**
- ✅ **Update `MerchantOrderService.InitOrderAsync` — validate token, set draft `ParticipantId`, update `OrderParticipant.Status = "OrderSubmitted"`, trigger ReadyToPay check**
- ✅ **Update DTOs — add `ParticipantToken` to `InitMerchantOrderRequest`/`InitMerchantOrderDto`, add `CompleteOrderRequest`**
- ✅ **Update `MerchantOrdersController` and `OrdersController` — pass token, add `CompleteOrder` endpoint**
- ✅ **Update dummy merchant HTML to parse and send `participantToken`**
- ✅ **Expand seed in `Tools.PayBySharePay` — add friends, merchants, and two group orders for the first two participants; fix seed to NOT add merchant as OrderParticipant**
- ✅ **Flush database, run seed, verify build**

