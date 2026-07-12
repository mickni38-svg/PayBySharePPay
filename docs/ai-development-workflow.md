# AI Development Workflow

## Model

PayNSync uses one AI agent that switches role according to the task.

The Product Owner supplies the task and remains the final reviewer.

## New use case

1. Product Owner attaches a use case.
2. Agent analyses it against project documentation and code.
3. Agent creates implementation and test plans.
4. Product Owner reviews and approves.
5. Agent implements one vertical slice.
6. Agent builds and tests.
7. Agent performs a separate reviewer pass.
8. Agent updates affected documentation.
9. Product Owner reviews final code.

## Bug fix

Small bugs use reduced reading scope.

The agent reads only the defect description, affected code, nearest dependencies, and relevant tests. Broader documentation is opened only when the defect affects business behaviour, architecture, security, payment, data, or public contracts.

## Refactor

Refactoring is separate from feature work and must preserve observable behaviour.

## Single source of truth

- `/docs` stores project/domain knowledge.
- `.github/copilot-instructions.md` routes tasks.
- `/.ai/workflows` defines processes.
- `/.ai/rules` defines reusable constraints.
- Attached use cases define requested new behaviour.
- Source code, tests, configuration, and migrations show current implementation.

When they disagree, the disagreement must be surfaced rather than guessed away.
