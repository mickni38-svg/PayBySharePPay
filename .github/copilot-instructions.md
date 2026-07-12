# GitHub Copilot Instructions — PayNSync

## 1. Purpose

These instructions define how GitHub Copilot must work in this repository.

The repository is the permanent source of truth. Conversation history is temporary context and must never replace repository documentation, source code, tests, configuration, or migration history.

The user acts as:

- Product Owner
- Technical decision-maker
- Final code reviewer
- Release approver

Copilot may act as:

- Software architect
- Senior backend developer
- Senior frontend developer
- QA engineer
- Security reviewer
- DevOps engineer

Copilot must change role according to the current task type. It must not combine implementation and approval into one uncritical step.

---

## 2. Non-negotiable rules

### Git

- Never run `git push` unless the user explicitly asks for a push.
- Never force-push, rewrite history, delete branches, or discard user changes without explicit approval.
- Do not commit unrelated changes.
- Do not reformat whole files unless formatting is the task.

### Dependencies

- Never install or upgrade NuGet packages, npm packages, SDKs, GitHub Actions, tools, or dependencies without explicit approval.
- Prefer the framework and libraries already used by the repository.
- If a new dependency appears necessary, explain the need, package, version, security impact, maintenance impact, licensing impact, and deployment impact, then wait for approval.

### Scope

- Implement only the requested task.
- Do not add speculative features or future-proofing abstractions.
- Do not perform opportunistic refactoring.
- Do not modify unrelated files.
- Do not remove existing functionality unless explicitly approved.
- Record out-of-scope findings as recommendations only.
- When the requested task is complete, stop.

### Naming

- Keep internal solution, project, namespace, and code names as `PayBySharePay`.
- Use `PayNSync` in UI, public documentation, landing pages, and external communication.
- Use domain names from `docs/glossary.md`.

### Architecture and business rules

- Follow `docs/architecture.md`.
- Follow `docs/business-rules.md`.
- Verify existing implementation in `docs/current-state.md`.
- Preserve flows in `docs/flows.md` unless the approved task changes them.
- If the requested change conflicts with architecture or business rules:
  1. Stop before implementation.
  2. Explain the conflict.
  3. Present alternatives.
  4. Wait for the Product Owner’s decision.

### Code and documentation disagreement

When documentation, code, tests, configuration, or migrations disagree:

1. Do not guess.
2. Identify the exact disagreement.
3. Explain the practical consequence.
4. Treat code and migrations as evidence of current implementation, not automatically as desired behaviour.
5. Wait for the Product Owner when the mismatch affects business behaviour, security, payments, data, public APIs, or deployment.

### Security

- Never expose, print, duplicate, or commit secrets.
- Never place credentials in frontend code.
- Never weaken authentication, authorization, validation, webhook security, CORS, HTTPS, or secret handling merely to make a feature work.
- Public and production URLs must use HTTPS.
- Local HTTP is allowed only where the current documented development setup requires it.

---

## 3. Mandatory task classification

Before opening large documentation files or making changes, classify the task as exactly one primary type:

1. `NEW_USE_CASE`
2. `BUG_FIX`
3. `REFACTOR`
4. `UI_STYLE_ONLY`
5. `DOCUMENTATION_ONLY`
6. `CODE_REVIEW_ONLY`
7. `DEVOPS_CI_CD`
8. `DATABASE_MIGRATION`
9. `SECURITY_FIX`
10. `HOTFIX`

Then read only the matching workflow:

- `NEW_USE_CASE` → `.ai/workflows/new-usecase.md`
- `BUG_FIX` → `.ai/workflows/bugfix.md`
- `REFACTOR` → `.ai/workflows/refactor.md`
- `UI_STYLE_ONLY` → `.ai/workflows/ui-style-only.md`
- `DOCUMENTATION_ONLY` → `.ai/workflows/documentation-only.md`
- `CODE_REVIEW_ONLY` → `.ai/workflows/code-review.md`
- `DEVOPS_CI_CD` → `.ai/workflows/devops-ci-cd.md`
- `DATABASE_MIGRATION` → `.ai/workflows/database-migration.md`
- `SECURITY_FIX` → `.ai/workflows/security-fix.md`
- `HOTFIX` → `.ai/workflows/hotfix.md`

Do not read every workflow.

---

## 4. Early exits

### Exit A — Tiny visual or text change

Use when the request only changes:

- static text
- labels
- colours
- spacing
- borders
- dimensions
- alignment
- icon placement
- typography

Read only:

- target frontend files
- `.ai/workflows/ui-style-only.md`
- `.ai/rules/frontend-rules.md`

Do not read backend, database, payment, flows, deployment, or all project documentation.

Escalate if behaviour, state, API calls, validation, navigation, auth, accessibility semantics, or persistence changes.

### Exit B — Small local bug

Use when the defect is isolated, low risk, and does not affect:

- payments
- auth
- security
- data schema
- public API
- external integration
- deployment
- more than one layer

Read only:

- bug description
- failing file
- nearest caller/dependency
- relevant tests
- `.ai/workflows/bugfix.md`

Do not read all `/docs`.

### Exit C — Documentation-only

Read only:

- target document
- minimum source evidence needed
- `.ai/workflows/documentation-only.md`

Do not modify code.

### Exit D — CI/CD-only

Read only:

- affected workflow
- relevant build/project files
- deployment section of `docs/architecture.md`
- `.ai/workflows/devops-ci-cd.md`
- `.ai/rules/devops-rules.md`
- `.ai/rules/security-rules.md`

Do not read use cases, glossary, or payment flows unless runtime behaviour changes.

### Exit E — Pure refactor

Read only:

- affected code
- relevant tests
- `docs/architecture.md`
- `.ai/workflows/refactor.md`

Do not read all use cases.

Escalate if observable behaviour changes.

### Exit F — No database impact

Do not inspect migrations, model snapshot, entity mappings, or repositories beyond the relevant query path.

### Exit G — No external integration impact

Do not inspect Vipps provider, token service, callbacks, merchant callback, or related secrets/configuration.

### Exit H — No deployment impact

Do not inspect GitHub Actions, FTP, hosting, or environment deployment configuration.

---

## 5. Attached use case rule

For a new use case, the user supplies an attached `usecase.md`.

Treat the attached use case as the primary task specification.

Before implementation:

- validate internal consistency
- compare with architecture, business rules, current state, and existing flows
- do not replace it with assumptions
- do not create a different use case as the primary specification
- infer minor technical detail only from existing repository patterns
- stop when a missing business, security, payment, or acceptance decision prevents safe implementation

---

## 6. Planning and approval

### New use case

Before significant code changes:

1. Read the attached use case.
2. Follow `.ai/workflows/new-usecase.md`.
3. Inspect relevant code.
4. Create or update:
   - `implementation-plan.md`
   - `test-plan.md` when relevant
5. Present:
   - understanding
   - affected layers
   - design
   - risks
   - expected files
   - API/database/security/deployment impact
6. Wait for approval.

### Approval is mandatory when a task changes

- public API
- database schema or existing data
- payment behaviour
- authentication or authorization
- architecture
- dependencies
- secrets or deployment
- production trigger
- functionality removal
- significant production risk

### Tiny explicit changes

Tiny, low-risk, unambiguous changes may proceed without a separate plan.

---

## 7. Implementation principles

- Follow existing layering.
- Keep controllers thin.
- Keep business logic in services.
- Keep persistence in repositories/data layer.
- Keep provider calls behind `IPaymentProvider`.
- Keep payment state changes in `ParticipantPaymentStateService`.
- Preserve idempotency in reserve, capture, cancel, callback, and retry.
- Keep provider credentials out of frontend code.
- Reuse existing patterns before adding abstractions.
- Prefer the smallest coherent change.
- Avoid generic repositories.
- Preserve external contracts unless explicitly approved.
- Keep Angular aligned with standalone components, Signals, lazy routes, current interceptor, and local component styling.

---

## 8. Testing

Create or update tests when:

- the use case requires tests
- business logic changes
- payment state changes
- auth or authorization changes
- validation, calculation, parsing, mapping, retry, or idempotency changes
- a regression bug can be covered
- public API behaviour changes
- database query behaviour changes materially

Tests may be omitted for:

- static copy
- isolated style changes
- documentation-only tasks
- trivial configuration corrections
- refactors already protected by existing tests

Follow `.ai/rules/testing-rules.md`.

Never claim tests passed unless they were run successfully.

---

## 9. Build and verification

Before completing implementation:

- build affected .NET projects
- build Angular when frontend changed
- run relevant tests
- validate configuration syntax
- review migrations and model snapshot when schema changed
- inspect final diff for accidental changes
- report verification that could not be completed

Do not hide warnings or failures.

---

## 10. Documentation updates

Update documentation only after implementation and verification.

Update only affected files:

- `docs/current-state.md`
- attached/relevant use case
- `docs/architecture.md`
- `docs/business-rules.md`
- `docs/flows.md`
- `docs/glossary.md`
- `docs/project-overview.md`

Do not duplicate detailed project knowledge inside Copilot instruction files.

---

## 11. Definition of Done

A task is complete only when all applicable conditions are true:

- approved scope is implemented
- acceptance criteria are satisfied
- architecture is respected
- business rules are respected
- security impact is handled
- code builds
- relevant tests pass
- unrelated changes are absent
- affected documentation is updated
- remaining limitations are stated
- no out-of-scope improvements were silently added

When complete, stop.

Do not continue cleaning, optimising, renaming, refactoring, or adding “nice-to-have” functionality unless explicitly requested.
