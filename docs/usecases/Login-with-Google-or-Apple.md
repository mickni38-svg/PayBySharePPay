# Use Case: Authentication with Google or Apple Login

## Goal

Allow users to authenticate with PayNSync using either **Google Login** or **Apple Login** without requiring a traditional username/password. The solution must work for both the current web application and future native mobile apps.

---

# Actors

- User
- PayNSync Web Application
- PayNSync Backend API
- Google Identity Provider
- Apple Identity Provider

---

# Preconditions

- The user opens the PayNSync application.
- The user has either a Google account or an Apple ID.
- Google and Apple authentication have been configured.

---

# Main Flow

1. The user opens PayNSync.
2. The login page is displayed.
3. The user can choose one of the following options:
   - Continue with Google
   - Continue with Apple
4. The user authenticates with the selected provider.
5. The provider returns an identity token.
6. The frontend sends the token to the PayNSync Backend.
7. The backend validates the token.
8. The backend searches for an existing PayNSync user linked to the provider.
9. If the user exists:
   - The existing PayNSync account is loaded.
10. If the user does not exist:
    - A new PayNSync account is created automatically.
11. The backend issues a PayNSync session (JWT + Refresh Token or secure session cookie).
12. The frontend stores the session securely.
13. The user is redirected to the Home page.

---

# Alternative Flow – First Login

If no PayNSync account exists:

1. Create a new PayNSyncUser.
2. Store the selected Login Provider.
3. Continue with the normal login flow.

---

# Alternative Flow – Existing Account

If another PayNSync account already exists using the same email address:

- Do **not** automatically merge accounts.
- Require explicit verification before linking providers.

---

# Business Rules

## BR-001

A PayNSync user shall always have an internal unique identifier.

Example:

```
UserId = 42
```

Google or Apple identifiers must never be used as primary keys throughout the application.

---

## BR-002

Authentication providers are only used to verify identity.

They are **not** considered the application's user identity.

---

## BR-003

A user may later connect multiple authentication providers to the same PayNSync account.

Example:

- Google
- Apple

Both providers should authenticate the same PayNSync user.

---

## BR-004

The user should remain signed in on the same device until:

- Logout
- Refresh token expires
- Session is revoked

The user should not need to authenticate every time the application is opened.

---

## BR-005

The authentication architecture must support:

- Angular Web App (current)
- Native iOS App (future)
- Native Android App (future)

without changing the backend authentication model.

---

# Data Model

## PayNSyncUser

| Property | Description |
|----------|-------------|
| Id | Internal User Id |
| DisplayName | User display name |
| Email | Primary email |
| CreatedAtUtc | Creation timestamp |
| LastLoginAtUtc | Last successful login |

---

## UserLoginProvider

| Property | Description |
|----------|-------------|
| Id | Identifier |
| UserId | Reference to PayNSyncUser |
| Provider | Google / Apple |
| ProviderUserId | External provider identifier |
| Email | Email returned by provider |
| CreatedAtUtc | Linked date |

---

# Acceptance Criteria

- User can choose Google Login.
- User can choose Apple Login.
- First login automatically creates a PayNSync account.
- Existing users are recognized.
- Users remain signed in after browser refresh.
- Users remain signed in on future visits.
- The entire PayNSync domain uses the internal PayNSyncUserId.
- Authentication provider IDs are never used as business identifiers.
- The same backend authentication flow can later be reused by native mobile applications.

---

# Future Extensions

Possible future authentication providers:

- Microsoft
- Facebook
- GitHub
- Phone Number (OTP)
- Passkeys (FIDO2/WebAuthn)

These providers should plug into the same authentication architecture without requiring changes to the domain model.

---

# Out of Scope

The following are intentionally excluded from the MVP:

- Username/password authentication
- Email magic links
- MitID authentication
- Two-factor authentication
- Enterprise Single Sign-On (SAML/OIDC)
