# Checklist — Version 0.0.1

## Purpose

Version 0.0.1 delivers the product's complete identity, account security, organization, membership, and access-control foundation. It must be usable independently before sectors, shifts, checklists, billing, or other operational features are implemented.

## Release Outcome

After this release, a person can securely create or access an account, create or join organizations, manage organization membership within their authority, and use the same protected identity across Web, iOS, and Android.

The release establishes the security boundaries that every later feature must reuse:

- One global user identity may belong to multiple organizations.
- Every organization membership has its own role and status.
- Organization context is explicit and data never crosses organization boundaries.
- Roles are `Owner`, `Admin`, `Manager`, and `Operator`.
- Owner, Admin, and Manager require MFA; Operator may enable it voluntarily.

## Included Scope

### 1. Account Registration and Email Verification

- Create an account with email and password.
- Validate email uniqueness and password policy without exposing existing accounts.
- Send, resend, expire, supersede, and consume single-use email confirmation links.
- Prevent unconfirmed local accounts from accessing protected features.
- Record acceptance of the current legal terms when required.

### 2. Authentication

- Sign in with a confirmed email and password.
- Sign in or register with Google or Apple.
- Validate external identity responses and link them by provider and stable subject identifier.
- Never merge accounts from matching email addresses alone.
- Link or unlink Google and Apple after recent authentication.
- Prevent removal of the last usable authentication method.
- Sign out from the current session or all sessions.

### 3. Password and Email Management

- Change the password after recent authentication.
- Request password recovery without revealing whether an account exists.
- Send, expire, supersede, and consume single-use password-reset links.
- Revoke all active sessions after password recovery.
- Change the account email only after recent authentication and verification of the new address.
- Notify the previous address when the account email changes.
- Preserve MFA requirements throughout password and email recovery.

### 4. Multi-Factor Authentication

- Enroll a compatible TOTP authenticator application.
- Confirm enrollment before MFA becomes active.
- Require MFA during sign-in for protected roles, including after Google or Apple authentication.
- Generate single-use recovery codes, display them once, and replace the previous set when regenerated.
- Accept and invalidate individual recovery codes.
- Require recent authentication plus the current factor or a recovery code to remove or reset MFA.
- Require recent MFA confirmation for sensitive actions.
- Block users from retaining Owner, Admin, or Manager access without configured MFA.
- Do not use email or SMS as authentication factors.

### 5. Sessions and Account Protection

- Use protected Web sessions and rotating mobile refresh-token families.
- Show active sessions with client, device label, creation time, and recent activity.
- Revoke one session, every other session, or every session.
- Detect refresh-token reuse and revoke the affected session family.
- Apply failed-login throttling and temporary account lockout.
- Revoke sessions after password recovery, account suspension, confirmed compromise, or security-state changes that require it.
- Require reauthentication for password, email, MFA, external identity, ownership, and account-deletion operations.

### 6. Account Lifecycle and Privacy

- View and update the user's basic profile.
- Export the user's personal account data.
- Request account deletion with explicit confirmation and recent MFA when enabled.
- Prevent deletion while the user is the only active Owner of an organization.
- Revoke access immediately when deletion is confirmed.
- Preserve records that must remain for security, audit, or organization history while removing or anonymizing deletable personal data.

### 7. Organization Creation and Context

- Create an organization and make its creator the initial Owner.
- Maintain at least one active Owner at all times.
- View and update the organization's basic information, legal data, and time zone.
- Generate, view, and renew a unique organization access code.
- Treat the access code only as a join request mechanism, never as authentication or automatic authorization.
- List the organizations available to the current user.
- Switch organization and fully recompute membership, role, and permissions.

### 8. Invitations and Join Requests

- Invite a person by email with a predefined organization and role.
- Allow Owner to invite Admin, Manager, or Operator; allow Admin to invite Manager or Operator.
- Send, resend, revoke, expire, accept, or decline an invitation.
- Store only a hash of each single-use invitation token.
- Invalidate an older pending token when an invitation is resent.
- Require the authenticated account to match the invited email before acceptance.
- Allow an invited person to create an account or use an existing account.
- Reject reused, expired, revoked, or mismatched invitations without granting access.
- Allow a person to request membership with an organization access code.
- Keep join requests pending until Owner or Admin assigns and approves an allowed role.
- Prevent pending members from reading organization data.

### 9. Membership and Roles

- List organization members, pending invitations, and pending join requests.
- Approve or reject membership requests.
- Change a member between the roles permitted by the acting user.
- Suspend, reactivate, or remove a member without deleting historical identity references.
- Allow an active member to leave an organization unless they are its only active Owner.
- Transfer organization ownership after recent MFA confirmation by the current Owner and acceptance by the new Owner.
- Require the new Owner to have MFA before transfer completion.
- Allow only Owner to grant or remove Admin access.
- Prevent Admin from modifying Owner or another Admin's privileged status.
- Invalidate affected organization access immediately after suspension, removal, departure, or role reduction.

Manager and Operator exist in this release as valid membership roles. Their sector and operational capabilities begin only in later versions.

### 10. Authorization and Tenant Isolation

- Authenticate every protected request and resolve its active organization explicitly.
- Authorize each operation from current membership status, role, ownership, and recent-MFA requirements.
- Deny access by default when context or permission is missing.
- Never trust role, organization, ownership, or entitlement claims supplied by a client.
- Prevent identifiers from being used to read or modify another organization's data.
- Apply the same authorization rules to API endpoints, background work, and file or administrative operations.
- Return safe errors that do not disclose protected account or organization information.

### 11. Security Audit and Notifications

- Record append-only audit events for sign-in, relevant failures, recovery, password and email changes, MFA changes, session revocation, external identity changes, organization creation, invitations, join decisions, role changes, suspension, removal, departure, and ownership transfer.
- Record actor, organization context, action, target, timestamp, result, and safe request metadata.
- Never place passwords, tokens, recovery codes, secret values, or sensitive provider payloads in audit records or telemetry.
- Send transactional email for confirmation, recovery, invitations, email changes, and security-sensitive events.
- Provide in-app security notifications where an authenticated user must see the event.
- Keep email delivery asynchronous, retryable, idempotent, and provider-neutral.

### 12. Platform Coverage

- ASP.NET Core provides the identity authority, authorization, organization rules, and protected APIs.
- Web supports the complete account and organization flow.
- iOS and Android support authentication, MFA, session persistence, organization selection, invitations opened from links, and the account flows applicable to mobile.
- Google and Apple sign-in use the native platform entry points and the backend remains the final identity authority.
- Deep links return confirmation, recovery, invitation, and external authentication flows to an approved application destination.
- The clients never store passwords or usable recovery tokens and use platform-appropriate protected session storage.

### 13. Operational Readiness

- Development runs the complete flow locally while using Development secrets and real SMTP delivery.
- Production deploys through the defined Cloud Run and Google Cloud architecture.
- Development and Production use isolated credentials, identity clients, redirect destinations, and email configuration.
- Database migrations cover all identity, session, organization, membership, invitation, audit, and email-delivery data.
- Logs, metrics, traces, health checks, and security alerts cover the complete authentication path.
- Runbooks cover account compromise, MFA loss, email-delivery failure, leaked credentials, and Owner recovery without bypassing authorization rules.

## Required Validation

- Unit tests cover identity policy, token state, roles, membership transitions, invitation state, and ownership invariants.
- Authorization tests exercise every role, inactive membership, missing MFA, and cross-organization access attempts.
- Persistence tests verify uniqueness, concurrency, expiration, token consumption, and organization isolation against PostgreSQL.
- External adapter tests cover SMTP and Google and Apple identity validation through fakes or provider sandboxes.
- Security tests cover enumeration resistance, brute-force controls, replay, token reuse, session revocation, unsafe redirects, CSRF, and privilege escalation.
- End-to-end tests cover registration, email confirmation, login, MFA, recovery, organization creation, invitation acceptance, membership administration, organization switching, and account deletion constraints.
- Web, iOS, and Android complete the applicable release flows before approval.

## Explicitly Out of Scope

- Sectors and organization charts.
- Shifts and operational assignments.
- Checklists, tasks, schedules, execution, review, and escalation.
- Operational files, photos, videos, and offline synchronization.
- Dashboards and operational reports.
- Billing, subscription checkout, and paid-access enforcement.
- Production customer onboarding and customer data.

Later versions must build on this release's identity, organization, authorization, session, audit, and notification boundaries instead of creating parallel mechanisms.

## Definition of Done

Version 0.0.1 is complete only when:

- Every included flow works through its supported client and the backend.
- All privileged flows enforce role, organization context, recent authentication, and MFA where required.
- Cross-organization access and privilege escalation tests pass.
- Security tokens are single-use where applicable, expire correctly, and are never persisted in usable plaintext.
- Account, session, invitation, membership, and ownership state transitions are auditable and recover safely from retries.
- No later-domain entity such as sector, shift, checklist, or subscription is required for the release to operate.
- The release can be deployed, observed, and operated in Development and Production using the documented architecture.
