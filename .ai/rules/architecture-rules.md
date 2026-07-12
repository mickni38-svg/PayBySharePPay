# Architecture Rules

- Respect dependency direction in `docs/architecture.md`.
- Controllers handle HTTP and delegate.
- Services own business logic.
- Repositories own persistence.
- Infrastructure implements external providers.
- Frontend never holds Vipps credentials.
- Provider calls go through `IPaymentProvider`.
- Payment state transitions go through `ParticipantPaymentStateService`.
- Merchant draft submission must not release the final merchant order.
- Final merchant callback occurs only after required captures succeed.
- Do not create layer shortcuts.
- Do not put EF queries in controllers.
- Do not put HTTP response logic in services.
- Do not introduce generic repositories.
