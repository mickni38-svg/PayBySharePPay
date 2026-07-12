# Testing Rules

Priorities:

1. business rules
2. payment transitions
3. idempotency
4. authorization
5. validation
6. mapping/calculation
7. error handling
8. integration boundaries

Rules:

- use existing xUnit patterns
- reuse existing fakes
- do not add EF InMemory
- do not add mocking packages without approval
- name tests by behaviour
- include success/failure/boundaries where relevant
- keep tests deterministic
- never call real Vipps in unit tests
- never claim tests passed unless run
