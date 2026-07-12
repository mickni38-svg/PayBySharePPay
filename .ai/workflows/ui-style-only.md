# Workflow — UI / Style Only

## Trigger

Use only for presentation changes:

- colour
- spacing
- border
- typography
- dimensions
- alignment
- icon placement
- responsive layout
- static labels

## Read

- target Angular component files
- shared styles actually used
- `.ai/rules/frontend-rules.md`

## Do not read

- backend services
- repositories
- migrations
- payment provider
- callbacks
- deployment workflows
- all domain documentation

## Escalate when changing

- API calls
- state
- validation
- navigation
- auth
- permissions
- persistence
- payment behaviour
- meaningful accessibility semantics

## Implementation

- preserve current patterns
- reuse existing variables/tokens
- keep mobile-first behaviour
- avoid brittle fixed positioning
- maintain accessible contrast, labels, and tap targets

## Verification

Build Angular and report any visual verification still needed.
