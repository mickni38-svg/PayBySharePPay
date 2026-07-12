# PayNSync AI Development System

This folder contains workflows, reusable rules, templates, and short prompts for using one Copilot agent in multiple roles.

## Main entry point

The repository-wide entry point is:

```text
.github/copilot-instructions.md
```

It classifies the task and routes Copilot to one workflow.

## Structure

```text
.ai/
├── README.md
├── workflows/
├── rules/
├── templates/
└── prompts/
```

## Source of truth

- `/docs` contains project and domain knowledge.
- `/.ai` defines how the agent works.
- Attached use cases define requested new behaviour.
- Source code, tests, configuration, and migrations show current implementation.

The agent must not read every file for every task. Early exits reduce unnecessary context.

## Recommended workflow

1. Attach a use case or bug description.
2. Use a short prompt from `.ai/prompts`.
3. Let Copilot classify the task.
4. Review analysis/plan.
5. Approve implementation when required.
6. Review code and documentation.
