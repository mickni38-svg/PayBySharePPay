# Business Rule Enforcement

Critical invariants:

- one merchant per order in v1
- one participant payment per participant per order
- draft submission is not final payment
- `ReadyToPay` requires relevant payments to be `Reserved`
- host approval captures existing reservations
- host approval does not create new payments
- capture uses provider reference, not phone number
- merchant final order is released only after required captures succeed
- paid orders cannot be cancelled
- transitions follow the state machine
- state changes remain idempotent
- merchant frontend contains no provider credentials

If code and rule differ, stop and surface the mismatch.
