# Workflow — New Use Case

## Trigger

Use for new observable behaviour when the user supplies an attached use case.

## Role sequence

1. Analyst / Product Engineer
2. Architect
3. QA Engineer
4. Developer
5. Reviewer
6. Documentation Maintainer

The implementation role must not approve its own output without a separate review pass.

## Required reading

Always read:

- attached use case
- `docs/project-overview.md`
- `docs/architecture.md`
- `docs/business-rules.md`
- `docs/current-state.md`
- `docs/flows.md`

Read `docs/glossary.md` when terminology, naming, DTO/entity names, or UI wording matters.

Read related use cases only when they overlap.

## Early exits

### Backend-only
Skip Angular and Merchant Demo when no frontend impact exists.

### Frontend-only behaviour
Inspect only relevant Angular components/services/routes and the API contract they use.

### No database impact
Skip migrations, snapshot, and unrelated repositories.

### No payment/integration impact
Skip provider, callback, merchant callback, and credentials.

### No deployment impact
Skip GitHub Actions and hosting configuration.

## Analysis

Produce:

- use case summary
- actors and preconditions
- interpretation of acceptance criteria
- overlap with current implementation
- affected projects and layers
- data flow
- API impact
- database impact
- security impact
- payment/integration impact
- backward compatibility impact
- risks and decisions
- files expected to change

Stop if architecture or business rules conflict.

## Plans

Create:

- `implementation-plan.md`
- `test-plan.md` when relevant

Use the templates under `.ai/templates`.

Plans must be concrete. Avoid vague steps.

## Approval gate

Present analysis and plans. Wait for approval before significant implementation.

## Implementation

Implement one vertical slice:

- database if needed
- repositories/data access if needed
- service logic
- DTO/API
- frontend
- integration/configuration
- tests
- documentation

Do not implement future use cases.

## Review

Switch role and verify:

- acceptance criteria
- architecture
- business rules
- security
- idempotency
- error handling
- compatibility
- tests
- accidental changes

## Verification

Run applicable builds and tests.

## Exit

When all approved acceptance criteria are implemented and verified, stop.
