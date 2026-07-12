# Workflow — Refactor

## Trigger

Use only for internal code improvement with no intended observable behaviour change.

## Read

- affected code
- relevant tests
- `docs/architecture.md`
- relevant business rule only when domain logic is touched

Do not read all use cases.

## Behaviour lock

Do not change:

- public API
- JSON contracts
- schema
- validation
- error/status codes
- payment behaviour
- state transitions
- UI behaviour
- deployment output

## Analysis

Explain:

- current problem
- proposed internal change
- benefit
- affected files
- regression risk
- evidence behaviour remains unchanged

Wait for approval for non-trivial refactors.

## Verification

Use existing tests as behaviour lock. Build and run affected tests.

## Exit

If observable behaviour must change, reclassify as use case or bug fix.
