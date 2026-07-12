# Database Rules

- Use EF Core and SQL Server patterns already present.
- Use entity-specific repositories.
- Preserve key and relationship semantics.
- Define delete behaviour deliberately.
- Add a migration for schema changes.
- Do not edit applied migrations.
- Handle existing rows when adding non-null fields.
- Use decimal precision for kroner.
- Use `long` minor units for provider payments.
- Preserve `RowVersion`.
- Avoid N+1 queries.
- Add unique constraints when concurrency requires guaranteed uniqueness.
- Do not extend plaintext credential storage without approval.
