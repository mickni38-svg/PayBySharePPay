# Security Rules

- Never commit or log secrets.
- Never log access tokens, JWTs, passwords, or payment credentials.
- Keep provider integration server-side.
- Derive authorization from authenticated claims.
- Protect destructive endpoints from production.
- Treat anonymous webhooks as untrusted input.
- Preserve HTTPS.
- Keep CORS narrow.
- Do not expose internal exception detail.
- Validate external identity-token audience.
- Do not auto-merge external-login accounts without an approved secure flow.
- Existing plaintext merchant credentials are technical debt, not a pattern to extend.
