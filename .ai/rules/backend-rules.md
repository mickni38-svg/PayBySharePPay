# Backend Rules

- Use the repository's current .NET version.
- Keep controllers thin.
- Keep DTOs separate from EF entities.
- Validate external input at boundaries.
- Preserve async and nullable patterns.
- Do not expose internal details.
- Preserve provider references and idempotency keys.
- Consider concurrency for payment/order updates.
- Use structured logging without secrets.
- Avoid unnecessary `SaveChangesAsync` calls.
