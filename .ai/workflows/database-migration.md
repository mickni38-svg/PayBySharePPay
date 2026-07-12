# Workflow — Database Migration

## Read

- attached use case/bug
- affected entity
- DbContext/model configuration
- repository/service callers
- related migrations
- model snapshot
- relevant architecture/rules
- `.ai/rules/database-rules.md`

## Mandatory plan

State:

- schema change
- data migration requirement
- backward compatibility
- null/default strategy
- index/constraint impact
- rollback risk
- deployment order
- compatibility window

Wait for approval.

## Rules

- do not edit applied migrations
- create a new migration
- review generated migration
- update snapshot
- do not delete/transform data without approval
- consider existing rows
- preserve monetary precision and minor-unit semantics

## Verification

Build, review migration/snapshot, run relevant tests, document deployment dependency.
