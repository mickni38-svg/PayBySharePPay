# Workflow — DevOps / CI-CD

## Read

- affected workflow
- affected project/build files
- deployment section of `docs/architecture.md`
- `.ai/rules/devops-rules.md`
- `.ai/rules/security-rules.md`

Read application code only if required for build/runtime configuration.

## Early exit

Do not read use cases, glossary, merchant flow, or business rules for pipeline-only tasks.

## Safety

Never:

- expose secrets
- print secret values
- add real credentials
- silently enable automatic production deployment
- change hosting provider assumptions without approval
- remove `app_offline.htm` handling without understanding file locking

## Plan for non-trivial changes

Explain:

- triggers
- permissions
- secrets
- environments
- artifacts
- rollback
- downtime
- failure behaviour

## Verification

Validate YAML and referenced paths. Run underlying build commands when possible.

Do not claim deployment success unless the workflow ran successfully.
