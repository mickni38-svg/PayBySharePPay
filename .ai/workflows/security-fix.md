# Workflow — Security Fix

## Trigger

Use for:

- authentication
- authorization
- JWT/OAuth
- secrets
- webhooks
- CORS
- public endpoints
- input validation
- ownership
- sensitive logging
- credential storage
- production exposure of dev endpoints

## Read

- affected code path
- architecture security section
- relevant business rules
- current-state security gaps
- `.ai/rules/security-rules.md`
- tests

## Analysis

State:

- threat
- asset
- attacker capability
- weakness
- mitigation
- compatibility impact
- configuration/deployment impact
- verification plan

## Rules

- do not weaken another control
- do not trust client-supplied identity when JWT identity exists
- do not expose internal exception detail
- do not add secrets to source control
- protect destructive dev endpoints from production
- preserve auditability

Security fixes require tests when practical.

Document residual risk and stop.
