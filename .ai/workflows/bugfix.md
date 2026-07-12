# Workflow — Bug Fix

## Trigger

Use when existing behaviour is wrong compared with an approved requirement, use case, rule, contract, or previously working behaviour.

## Fast path

Use when all are true:

- local defect
- visible root cause
- no schema change
- no API contract change
- no auth/security/payment impact
- no architecture change
- no dependency change
- one small code path

Steps:

1. Read bug description.
2. Inspect failing code and nearest dependencies.
3. Inspect relevant tests.
4. State root cause and minimal fix.
5. Implement.
6. Build.
7. Run focused tests if available.
8. Stop.

Do not read all docs.
Do not create implementation/test plans.

## Standard path

Read only:

- bug description
- attached use case if supplied
- relevant current-state section
- relevant business rule
- relevant flow
- architecture for affected layers only

## Escalate when affecting

- authentication
- authorization
- payments
- webhook handling
- merchant final callback
- data integrity
- schema
- public API
- production deployment

## Root cause

Identify:

- failing condition
- why code allows it
- other affected callers
- regression risk
- whether tests or docs are also wrong

## Testing

Add a regression test when practical for business logic, security, payment, validation, mapping, or calculation.

## Exit

Fix only the defect. Do not refactor surrounding code unless necessary.
