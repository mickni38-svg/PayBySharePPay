# API Rules

- Preserve compatibility unless approved.
- Use existing route conventions.
- Apply authentication and authorization consistently.
- Use JWT identity for ownership when available.
- Use request/response DTOs.
- Validate required fields and invariants.
- Return correct status codes.
- Keep anonymous endpoints anonymous only when integration requires it.
- Do not expose internal exception detail.
- Keep webhooks idempotent.
- Webhooks must not release final merchant order before host approval/capture.
