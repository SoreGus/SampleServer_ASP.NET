# Checklist — Version 0.0.1 Implementation

## Purpose

This document is the executable implementation checklist for Version 0.0.1. It converts the scope defined in `Version-0.0.1.md` into ordered, verifiable work across the backend, database, shared core, Web, iOS, Android, infrastructure, security, testing, CI/CD, and operations.

An item is complete only when its implementation, applicable automated tests, documentation, and observability are complete. Checking an item based only on a partial client or a happy-path demonstration is not sufficient.

## Scope Guard

- [ ] Confirm `Version-0.0.1.md`, `UseCases.md`, `Data.md`, and `ARCHITECTURE.md` are the approved sources for the release.
- [ ] Record any discovered contradiction in the relevant source document before implementing it.
- [ ] Keep sectors, organization charts, shifts, checklists, operational files, offline synchronization, billing, and customer onboarding outside this release.
- [ ] Do not create temporary identity, authorization, organization, notification, or audit mechanisms that later features would need to replace.
- [ ] Keep all user-facing presentation names separate from the system role names `Owner`, `Admin`, `Manager`, and `Operator`.

## 1. Delivery Foundation

### 1.1 Repository and Component Boundaries

- [ ] Create or confirm independent projects for the ASP.NET Core backend, React Web application, Rust Core, iOS application, Android application, infrastructure, and tests.
- [ ] Enforce the documented dependency direction between Domain, Application, Infrastructure, API, clients, and platform adapters.
- [ ] Keep ASP.NET Core as the only identity authority and source of organization authorization decisions.
- [ ] Keep shared domain contracts independent of EF Core, HTTP, UI frameworks, device APIs, and Google Cloud SDKs.
- [ ] Pin supported SDK, compiler, package-manager, Terraform provider, and external dependency versions.
- [ ] Enable reproducible dependency restoration with committed lock and integrity files where supported.
- [ ] Establish component-scoped versioning and tag conventions for backend, Web, Rust Core, iOS, and Android.

### 1.2 Shared Contracts

- [ ] Define stable identifiers for users, organizations, memberships, invitations, sessions, external identities, and audit events.
- [ ] Define system enums and state transitions for roles, account status, membership status, invitation status, session status, and email-delivery status.
- [ ] Define validation rules once at the appropriate domain or application boundary.
- [ ] Define API DTOs independently from persistence entities and client-local models.
- [ ] Define machine-readable error codes for validation, authentication, MFA, authorization, conflict, expiration, throttling, and unavailable dependencies.
- [ ] Define UTC as the technical timestamp standard and organization time zone as presentation context.
- [ ] Publish and validate the initial OpenAPI contract before client integration begins.

## 2. Development Environment

### 2.1 Local Infrastructure

- [ ] Document supported macOS development prerequisites for .NET, PostgreSQL, Rust, Node.js, Xcode, Android tooling, and Terraform.
- [ ] Run the backend, PostgreSQL, Web application, background workers, file storage, telemetry, and auxiliary development services locally without Docker.
- [ ] Create the local PostgreSQL database and a least-privilege application user.
- [ ] Provide deterministic database creation and migration instructions.
- [ ] Provide commands for starting each component independently and for running the complete local system.
- [ ] Keep development files in a dedicated local storage directory excluded from source control.
- [ ] Keep generated build output, local databases, local files, IDE state, and secret material excluded from source control.

### 2.2 Development Configuration and Secrets

- [ ] Put non-sensitive Development settings in the standard .NET Development configuration files.
- [ ] Create a dedicated Google Cloud Development project containing secrets only and no runtime infrastructure.
- [ ] Create separate Development credentials for SMTP, Google identity, Apple identity where required, and other protected integrations.
- [ ] Implement a one-time Development bootstrap that reads only authorized Development secrets from Google Secret Manager.
- [ ] Store retrieved values in the local .NET user-secrets store, never in `.env` files or committed settings.
- [ ] Make repeated bootstrap runs idempotent and update only changed or newly introduced secrets.
- [ ] Document explicit bootstrap and secret-refresh commands.
- [ ] Verify a developer cannot read Production secrets or access Production runtime resources.
- [ ] Verify secret values never appear in command output, logs, telemetry, build artifacts, or shell history.

### 2.3 Local Email and Links

- [ ] Configure real SMTP delivery in Development with Development-only credentials and sender identity.
- [ ] Restrict Development recipients and volume to prevent accidental external messaging.
- [ ] Configure Development callback and deep-link destinations for confirmation, recovery, invitation, Google, and Apple flows.
- [ ] Verify every email link can complete its flow in the local Web application and the applicable mobile application.

## 3. Production Infrastructure

### 3.1 Google Cloud Foundation

- [ ] Create the dedicated Google Cloud Production project and attach the intended billing account.
- [ ] Select and document the Production region and data-location assumptions.
- [ ] Enable only the Google Cloud APIs required by Version 0.0.1.
- [ ] Provision the Terraform remote-state bucket separately with versioning, restricted access, and recovery controls.
- [ ] Define Terraform modules and Production composition for IAM, networking, Cloud Run, Cloud SQL, Cloud Storage where required, Artifact Registry, Secret Manager, Tasks, Scheduler, and observability.
- [ ] Keep Terraform state free of secret values.
- [ ] Apply labels for environment, component, owner, and cost attribution to supported resources.
- [ ] Configure Google Cloud budget alerts before deploying Production workloads.
- [ ] Document that budget alerts do not impose a hard spending cap.

### 3.2 Identity and Access Management

- [ ] Create a distinct runtime service account for each workload that requires a different permission set.
- [ ] Grant each runtime identity only the database, secret, storage, task, and telemetry permissions it requires.
- [ ] Create a migration-job identity separate from the API runtime identity.
- [ ] Configure GitHub Actions authentication through Workload Identity Federation and short-lived credentials.
- [ ] Grant CI permission to build, publish, create revisions, run the migration job, and shift traffic without granting secret-value access.
- [ ] Require owner approval for Production deployment.
- [ ] Keep billing administration, IAM administration, Production secret access, account recovery, and break-glass access restricted to the owner.
- [ ] Prohibit long-lived Google Cloud service-account keys.
- [ ] Test denied access for developers, CI, and unrelated runtime identities.

### 3.3 Production Data and Compute

- [ ] Provision the initial Cloud SQL for PostgreSQL instance using the approved low-cost capacity.
- [ ] Create separate database identities for application runtime and schema migration.
- [ ] Restrict database connectivity to authorized workloads and administrative recovery paths.
- [ ] Configure connection-pool and Cloud Run scaling limits within Cloud SQL capacity.
- [ ] Keep automated backups and point-in-time recovery disabled until the First Customer gate defined by the architecture.
- [ ] Provision Artifact Registry and an image-retention cleanup policy.
- [ ] Provision Cloud Run services for the API and Web frontend.
- [ ] Configure zero minimum instances and a safe maximum-instance limit initially.
- [ ] Provision only the Cloud Tasks queues, Cloud Scheduler jobs, and Cloud Run Jobs required by Version 0.0.1.
- [ ] Keep internal task and job endpoints private and protected by service identity.
- [ ] Provision any required private Cloud Storage bucket with uniform access, versioning or lifecycle rules, and no public objects.

### 3.4 Production Secrets and External Configuration

- [ ] Create separate Production secrets for database access, token protection, SMTP, Google identity, Apple identity, and external callbacks.
- [ ] Grant each workload access only to the exact secret versions it consumes.
- [ ] Load Production secrets directly from Secret Manager at runtime.
- [ ] Define a controlled rotation procedure for every Production secret.
- [ ] Destroy superseded secret versions after the approved recovery period.
- [ ] Register Production Google and Apple clients with exact allowed origins, application identifiers, and redirect destinations.
- [ ] Configure the Production SMTP sender, domain authentication, and return-path settings when the public domain is available.
- [ ] Verify Production and Development credentials cannot be used interchangeably.

## 4. Database and Persistence

### 4.1 Identity Data

- [ ] Create persistence mappings and migrations for users, normalized unique email, profile data, password hash, email-confirmation state, lockout state, security stamp, and lifecycle status.
- [ ] Create external identity records unique by provider and stable provider subject.
- [ ] Create protected token records or provider-compatible stores for confirmation, recovery, and other single-use account operations.
- [ ] Create MFA configuration and recovery-code storage using protected values or cryptographic hashes.
- [ ] Create session records with client type, device label, token-family reference, creation, recent activity, expiry, revocation, and revocation reason.
- [ ] Ensure usable passwords, refresh tokens, confirmation tokens, recovery tokens, invitation tokens, and recovery codes are never persisted in plaintext.

### 4.2 Organization Data

- [ ] Create organization records with name, legal data, time zone, access-code state, lifecycle status, and audit timestamps.
- [ ] Create membership records unique by user and organization with role, status, activation, suspension, departure, and removal metadata.
- [ ] Enforce that a user can have different roles in different organizations.
- [ ] Create invitation records with organization, invited email, intended role, token hash, expiry, acceptance, decline, revocation, and replacement state.
- [ ] Create join-request records with organization, requester, submission state, decision, deciding actor, and assigned role.
- [ ] Create ownership-transfer records or equivalent durable state for initiation, acceptance, expiry, cancellation, and completion.
- [ ] Enforce through transactional application rules that an organization always retains an active Owner.
- [ ] Generate organization access codes with sufficient entropy and enforce uniqueness.

### 4.3 Audit and Delivery Data

- [ ] Create append-only audit-event storage with actor, organization context, action, target, result, timestamp, correlation identifier, and safe request metadata.
- [ ] Prevent ordinary application operations from updating or deleting audit events.
- [ ] Create notification records for security and organization events visible inside the product.
- [ ] Create email-delivery records with template version, recipient, idempotency key, status, attempts, provider acceptance time, and safe failure code.
- [ ] Create a transactional outbox for reliable email, notification, and security-event dispatch.
- [ ] Define retention and personal-data treatment for identity, audit, notification, invitation, and email-delivery records.

### 4.4 Migration Safety

- [ ] Generate an empty-database migration path for all Version 0.0.1 data.
- [ ] Test upgrading from every retained pre-release schema state.
- [ ] Make migrations fail safely without leaving ambiguous partial application state.
- [ ] Run Production migrations only through the dedicated Cloud Run migration job.
- [ ] Prevent the API from applying migrations automatically at startup.
- [ ] Document forward recovery and rollback constraints for each Production migration.

## 5. Backend — Identity and Authentication

### 5.1 ASP.NET Core Identity Authority

- [ ] Configure ASP.NET Core Identity for users, password hashing, email confirmation, lockout, security state, and recovery.
- [ ] Configure OpenIddict as the OAuth 2.0 and OpenID Connect authority for Web, iOS, and Android clients.
- [ ] Register clients with the minimum approved redirect destinations and grant types.
- [ ] Implement Web authentication with secure, HTTP-only, same-site cookies and CSRF protection.
- [ ] Implement mobile Authorization Code Flow with PKCE through the system browser.
- [ ] Issue 15-minute access tokens and rotating refresh tokens with a 30-day maximum lifetime.
- [ ] Validate issuer, audience, signature, expiry, client, redirect destination, PKCE verifier, and authorization state.
- [ ] Revoke the session family when an invalidated refresh token is reused.

### 5.2 Registration and Confirmation

- [ ] Implement registration with normalized unique email and password-policy validation.
- [ ] Return a neutral conflict response that does not unnecessarily reveal registered accounts.
- [ ] Create the account in an unconfirmed state and queue a confirmation email transactionally.
- [ ] Issue a temporary single-use confirmation token and persist only its protected reference or hash.
- [ ] Implement confirmation, expiry, replay rejection, and safe invalid-token responses.
- [ ] Implement confirmation resend with throttling and replacement of the previous pending token.
- [ ] Prevent protected access until local-email confirmation succeeds.
- [ ] Record required legal-term acceptance with its version and timestamp when applicable.

### 5.3 Password Authentication and Recovery

- [ ] Implement password sign-in for confirmed, active users.
- [ ] Apply throttling, progressive delay where appropriate, and temporary lockout to failed attempts.
- [ ] Audit successful sign-in and security-relevant failures without storing submitted credentials.
- [ ] Implement authenticated password change with current-password or recent-authentication confirmation.
- [ ] Implement neutral password-recovery request responses for registered and unregistered email addresses.
- [ ] Issue throttled, temporary, single-use reset tokens and queue recovery email transactionally.
- [ ] Reject expired, superseded, invalid, or replayed reset tokens safely.
- [ ] Update password security state and revoke every active session after successful recovery.
- [ ] Ensure password recovery never disables or bypasses MFA.

### 5.4 Google and Apple Authentication

- [ ] Implement Google authorization and callback handling for each supported client.
- [ ] Implement Apple authorization and callback handling for each supported client.
- [ ] Validate provider signature, issuer, audience, nonce or state, expiry, and stable subject.
- [ ] Handle Apple's limited first-login profile data without assuming it will be returned again.
- [ ] Create or locate accounts by the external identity's provider and stable subject, never by email alone.
- [ ] Require authentication with the existing account before linking when a provider email already exists locally.
- [ ] Ensure external registration does not grant organization access automatically.
- [ ] Implement authenticated linking and unlinking after recent authentication.
- [ ] Reject an external identity already linked to another user.
- [ ] Prevent removal of the user's final usable authentication method.

### 5.5 Email Change and Account Profile

- [ ] Implement retrieval and update of allowed basic profile fields.
- [ ] Implement email-change initiation after recent authentication.
- [ ] Queue a single-use confirmation link to the new email address.
- [ ] Keep the current email active until the new address is verified.
- [ ] Enforce uniqueness again when the change is confirmed.
- [ ] Update normalized identity data atomically and revoke sessions when required by policy.
- [ ] Notify the previous email address after a completed change.

## 6. Backend — MFA and Sessions

### 6.1 TOTP and Recovery Codes

- [ ] Generate a unique protected TOTP secret and standards-compatible enrollment information.
- [ ] Do not enable MFA until a valid authenticator code confirms enrollment.
- [ ] Generate high-entropy single-use recovery codes and display usable values only once.
- [ ] Persist only protected recovery-code representations.
- [ ] Consume a recovery code atomically so concurrent requests cannot reuse it.
- [ ] Regenerating recovery codes must invalidate every previous unused code.
- [ ] Require a valid authenticator or recovery code to complete MFA challenges.
- [ ] Apply throttling and safe failure responses to MFA attempts.
- [ ] Require recent authentication plus the current factor or recovery code for MFA removal or reset.
- [ ] Prevent Owner, Admin, and Manager memberships from remaining active without MFA.

### 6.2 Recent Authentication and Sensitive Actions

- [ ] Define the approved recent-password and recent-MFA time windows.
- [ ] Expose a consistent challenge result when an operation requires reauthentication or recent MFA.
- [ ] Require recent authentication for password, email, external identity, MFA, account deletion, and ownership changes.
- [ ] Require recent MFA for privileged role changes, ownership transfer, session-wide revocation, and other defined high-risk operations.
- [ ] Record successful and failed sensitive-action confirmation events.

### 6.3 Session Management

- [ ] Create an independently identifiable session for each Web or mobile sign-in.
- [ ] Update recent activity without creating excessive database writes.
- [ ] List only the authenticated user's active sessions with safe device metadata.
- [ ] Revoke the current session, a selected session, every other session, or all sessions.
- [ ] Reject access and refresh operations for revoked, expired, compromised, or security-stamp-invalid sessions.
- [ ] Revoke relevant sessions after account blocking, password recovery, suspected compromise, or identity-security changes.
- [ ] Make session revocation idempotent and effective across all clients promptly.

## 7. Backend — Organizations and Membership

### 7.1 Organization Creation and Selection

- [ ] Implement organization creation for an authenticated user.
- [ ] Create the initial Owner membership atomically with the organization.
- [ ] Require MFA setup before the initial Owner receives privileged organization access.
- [ ] Generate the initial organization access code atomically.
- [ ] Implement organization retrieval and update with Owner and Admin authorization.
- [ ] Implement access-code viewing and renewal for Owner and Admin.
- [ ] Invalidate the old code immediately when a new code is issued.
- [ ] List only organizations in which the user has an active membership.
- [ ] Implement explicit organization switching and recompute all authorization from current persisted state.

### 7.2 Invitations

- [ ] Implement invitation creation with organization, normalized email, intended role, expiry, and inviter authority validation.
- [ ] Allow Owner to invite Admin, Manager, or Operator and Admin to invite Manager or Operator.
- [ ] Reject duplicate active membership and conflicting pending invitation states safely.
- [ ] Generate a single-use high-entropy invitation token and store only its hash.
- [ ] Queue the invitation email in the same transaction as invitation creation.
- [ ] Implement resend by replacing or invalidating the previous pending token.
- [ ] Implement inviter revocation and recipient decline as distinct terminal states.
- [ ] Implement acceptance by a newly registered or existing authenticated account.
- [ ] Require the authenticated account's verified email to match the invitation email.
- [ ] Create membership and consume the invitation atomically.
- [ ] Reject expired, revoked, declined, replaced, consumed, replayed, or mismatched invitations.
- [ ] Make repeated terminal requests idempotent without granting duplicate membership.

### 7.3 Access-Code Join Requests

- [ ] Implement access-code lookup without disclosing unnecessary organization data.
- [ ] Create a pending join request without assigning permissions.
- [ ] Prevent duplicate active membership and duplicate unresolved requests.
- [ ] List pending requests only to authorized Owner and Admin users.
- [ ] Allow Owner to approve any defined role and Admin to approve Manager or Operator.
- [ ] Create membership and close the request atomically on approval.
- [ ] Record rejection without creating membership.
- [ ] Prevent pending or rejected requesters from accessing organization data.

### 7.4 Membership Administration

- [ ] List members using role-appropriate fields and organization isolation.
- [ ] Implement role change with current-actor authority validation.
- [ ] Restrict Admin grant and removal to Owner.
- [ ] Prevent Admin from modifying Owner or Admin privileged status.
- [ ] Require the target to configure MFA before activating Owner, Admin, or Manager access.
- [ ] Implement suspension and reactivation while preserving historical references.
- [ ] Implement logical membership removal and immediate access invalidation.
- [ ] Allow a member to leave unless they are the only active Owner.
- [ ] Prevent every operation that would leave the organization without an active Owner.
- [ ] Re-evaluate active sessions immediately after role reduction, suspension, removal, or departure.

### 7.5 Ownership Transfer

- [ ] Allow only a current Owner with recent MFA to initiate transfer.
- [ ] Require the target to be an active organization member with configured MFA.
- [ ] Create a temporary, cancellable, single-use acceptance operation.
- [ ] Require explicit acceptance and recent MFA from the new Owner.
- [ ] Complete the new Owner grant and the previous Owner's selected resulting role atomically.
- [ ] Prevent concurrent transfers and protect the last-Owner invariant throughout the process.
- [ ] Audit initiation, cancellation, expiry, acceptance, and completion.

## 8. Backend — Authorization and Isolation

- [ ] Centralize organization-context resolution and reject missing, inactive, or ambiguous context.
- [ ] Load current membership and authorization state from trusted server-side data.
- [ ] Define named policies for Owner-only, Owner-or-Admin, active-member, recent-authentication, and recent-MFA operations.
- [ ] Enforce authorization inside application use cases in addition to transport-level checks.
- [ ] Scope every organization query and mutation by organization identifier.
- [ ] Prevent client-supplied role, membership, organization, or ownership fields from becoming authorization facts.
- [ ] Return `401` only for authentication failures and `403` for authenticated but unauthorized operations without leaking protected details.
- [ ] Apply identical authorization to API, background handlers, jobs, and administrative operations.
- [ ] Invalidate cached authorization state immediately after security or membership changes.
- [ ] Add explicit cross-organization and insecure-direct-object-reference protections to every resource operation.

## 9. Backend — Audit, Notifications, and Email

### 9.1 Audit Trail

- [ ] Define the complete Version 0.0.1 audit-event catalog and stable action names.
- [ ] Record authentication, recovery, MFA, session, external identity, account, organization, invitation, join, membership, and ownership events.
- [ ] Record actor, target, organization, result, correlation identifier, timestamp, and safe source context.
- [ ] Record system actors distinctly from human actors.
- [ ] Redact credentials, tokens, codes, secret values, email contents, and provider payloads.
- [ ] Provide authorized audit retrieval for the current user and organization administrators within their allowed scope.
- [ ] Make audit pagination deterministic and resistant to cross-organization access.

### 9.2 Transactional Email

- [ ] Define versioned templates for confirmation, confirmation resend, recovery, email change, invitation, security alert, membership change, and ownership transfer.
- [ ] Keep template content and links free of secret values beyond the required single-use token.
- [ ] Generate only approved HTTPS Production links and approved Development links.
- [ ] Persist email intent and its outbox message in the originating transaction.
- [ ] Dispatch through the provider-neutral SMTP adapter using MailKit.
- [ ] Apply stable idempotency keys and bounded retries with backoff.
- [ ] Record `Sent` only when SMTP accepts the message and never claim recipient delivery without provider evidence.
- [ ] Preserve permanent failures for diagnosis and controlled reprocessing.
- [ ] Prevent email logs and telemetry from exposing tokens or full message bodies.

### 9.3 In-App Notifications

- [ ] Create notifications for relevant security, invitation, membership, and ownership events.
- [ ] List notifications only for their recipient and active organization context where applicable.
- [ ] Support unread state, individual read state, and mark-all-as-read.
- [ ] Use safe navigation targets that reauthorize the destination when opened.
- [ ] Make repeated notification production idempotent.

## 10. API Contract

- [ ] Expose the complete Version 0.0.1 use cases through consistent authenticated or anonymous API operations.
- [ ] Define request validation, response DTOs, pagination, concurrency behavior, and error codes for every operation.
- [ ] Avoid exposing persistence entities, password hashes, security stamps, internal token data, or provider secrets.
- [ ] Mark anonymous, authenticated, MFA-challenged, and organization-scoped operations explicitly in the contract.
- [ ] Define idempotency behavior for invitation acceptance, join decisions, session revocation, and ownership transitions.
- [ ] Generate or validate client contracts from the approved OpenAPI definition.
- [ ] Add compatibility checks so backend changes cannot silently break Web or mobile clients.

## 11. Rust Core

- [ ] Define shared client-domain models for authentication state, account profile, organization summary, membership role, invitation state, session summary, notification, and safe API errors.
- [ ] Implement portable validation that is genuinely identical across iOS and Android.
- [ ] Implement client use cases that do not depend on SwiftUI, UIKit, Android APIs, storage engines, or HTTP libraries.
- [ ] Define ports for API access, secure session storage, local persistence, external browser authentication, deep links, clock, and telemetry.
- [ ] Keep tokens and platform credentials out of logs, debug formatting, and non-secure local persistence.
- [ ] Define session-expiry, refresh, revocation, and organization-switch state transitions.
- [ ] Generate and verify UniFFI bindings for supported Swift and Kotlin targets.
- [ ] Test Rust behavior natively and through minimal Swift and Kotlin bridge checks.

## 12. Web Application

### 12.1 Authentication Experience

- [ ] Implement registration, email-confirmation result, confirmation resend, login, logout, and locked-account states.
- [ ] Implement Google and Apple entry points and safe callback handling.
- [ ] Implement forgot-password request, reset result, password change, and email-change flows.
- [ ] Implement MFA enrollment with authenticator details, confirmation, one-time recovery-code display, and explicit acknowledgment.
- [ ] Implement MFA challenge, recovery-code use, regeneration, and permitted removal flows.
- [ ] Implement safe handling for expired, replayed, malformed, and superseded links.
- [ ] Prevent open redirects and return only to approved internal destinations after authentication.

### 12.2 Account and Sessions

- [ ] Implement profile viewing and editing.
- [ ] Implement linked Google and Apple identity management with last-method protection.
- [ ] Implement active-session list with current-session identification.
- [ ] Implement selected-session, other-session, and all-session revocation with confirmation where appropriate.
- [ ] Implement personal-data export request and secure retrieval or delivery flow.
- [ ] Implement account-deletion request with ownership constraints, recent authentication, and explicit irreversible-action confirmation.

### 12.3 Organizations and Membership

- [ ] Implement organization creation and initial Owner MFA requirement.
- [ ] Implement organization list and switching without retaining prior-organization data in the UI state.
- [ ] Implement organization detail and permitted settings updates.
- [ ] Implement access-code viewing, copying, renewal, and join-request submission.
- [ ] Implement invitation creation, resend, revocation, acceptance, decline, expiry, and error states.
- [ ] Implement pending invitation and join-request administration.
- [ ] Implement member list, role change, suspension, reactivation, removal, and departure flows.
- [ ] Implement ownership-transfer initiation, acceptance, cancellation, expiry, and completion states.
- [ ] Hide unavailable actions for usability while still relying on server authorization.
- [ ] Reauthorize and refresh organization state after every role or membership transition.

### 12.4 Web Security and Quality

- [ ] Keep authentication cookies inaccessible to JavaScript.
- [ ] Apply CSRF protection to every state-changing cookie-authenticated operation.
- [ ] Apply a restrictive Content Security Policy and safe security headers.
- [ ] Avoid placing tokens in persistent browser storage, analytics, URLs after consumption, or error reports.
- [ ] Meet keyboard, focus, label, error-announcement, and contrast accessibility requirements for every included flow.
- [ ] Provide responsive layouts for supported desktop and mobile browser widths.
- [ ] Add component, integration, and end-to-end tests for all critical Web flows.

## 13. iOS Application

- [ ] Integrate the Rust Core and generated Swift bindings using the documented adapter boundary.
- [ ] Implement native screens and navigation for registration, login, confirmation guidance, recovery, MFA, account, sessions, organizations, invitations, members, and notifications.
- [ ] Use `ASWebAuthenticationSession` or the approved system-browser mechanism for OAuth/OIDC authorization with PKCE.
- [ ] Integrate Sign in with Apple through the approved native flow and backend validation.
- [ ] Integrate Google sign-in through its approved native or system-browser flow and backend validation.
- [ ] Store mobile session material only in Keychain through the secure-storage adapter.
- [ ] Implement token refresh, refresh-token rotation, revoked-session handling, and return to authentication.
- [ ] Register and validate Universal Links or approved deep links for confirmation, recovery, invitations, and authentication callbacks.
- [ ] Clear organization-scoped presentation and cached state when switching organizations or losing membership.
- [ ] Prevent tokens, recovery codes, and protected data from appearing in logs, screenshots of sensitive views where preventable, or application backups.
- [ ] Add Swift adapter, navigation, accessibility, and critical-flow tests.
- [ ] Run the complete iOS build, Rust bridge verification, signing check, and release tests locally on the owner's Mac.

## 14. Android Application

- [ ] Integrate the Rust Core and generated Kotlin bindings using the documented adapter boundary.
- [ ] Implement native screens and navigation for registration, login, confirmation guidance, recovery, MFA, account, sessions, organizations, invitations, members, and notifications.
- [ ] Use Android App Links and the approved system-browser mechanism for OAuth/OIDC authorization with PKCE.
- [ ] Integrate Google sign-in through the approved native or system-browser flow and backend validation.
- [ ] Integrate Apple sign-in through the supported browser-based flow and backend validation.
- [ ] Store mobile session material only in an Android Keystore-backed secure-storage solution.
- [ ] Implement token refresh, refresh-token rotation, revoked-session handling, and return to authentication.
- [ ] Register and validate App Links or approved deep links for confirmation, recovery, invitations, and authentication callbacks.
- [ ] Clear organization-scoped presentation and cached state when switching organizations or losing membership.
- [ ] Prevent tokens, recovery codes, and protected data from appearing in logs, screenshots of sensitive views where preventable, or application backups.
- [ ] Add Kotlin adapter, navigation, accessibility, and critical-flow tests.
- [ ] Run the complete Android build, Rust bridge verification, signing check, and release tests locally on the owner's Mac.

## 15. Security Verification

### 15.1 Authentication and Token Security

- [ ] Verify password hashes use the approved adaptive algorithm and current work factor.
- [ ] Verify confirmation, recovery, invitation, ownership, authorization, access, and refresh tokens have distinct purposes and audiences.
- [ ] Verify single-use tokens cannot be replayed under concurrent requests.
- [ ] Verify expired, replaced, revoked, malformed, wrong-client, wrong-audience, and wrong-environment tokens fail safely.
- [ ] Verify redirect destinations, origins, deep links, and callback schemes are strictly allowlisted.
- [ ] Verify login, recovery, confirmation resend, invitation, MFA, and token endpoints have appropriate throttling.
- [ ] Verify error messages and timing do not expose account existence unnecessarily.
- [ ] Verify security-state changes invalidate the correct sessions and token families.

### 15.2 Authorization and Organization Isolation

- [ ] Test every operation as Owner, Admin, Manager, Operator, pending member, suspended member, removed member, and unauthenticated user.
- [ ] Test every organization-scoped operation with an identifier belonging to another organization.
- [ ] Verify role changes cannot exceed the acting user's authority.
- [ ] Verify no operation can remove, suspend, demote, delete, or transfer away the final active Owner incorrectly.
- [ ] Verify required-MFA roles cannot retain privileged access without MFA.
- [ ] Verify switching organizations cannot reuse prior organization data, permissions, navigation state, or cached responses.
- [ ] Verify background tasks and jobs enforce the same organization boundaries as interactive requests.

### 15.3 Application and Infrastructure Security

- [ ] Run dependency vulnerability checks for .NET, npm, Rust, Swift, Kotlin, and Terraform dependencies.
- [ ] Scan repositories and build outputs for committed credentials and high-confidence secret patterns.
- [ ] Review cookie attributes, CORS, CSRF, CSP, HSTS, TLS, cache headers, and sensitive-response behavior.
- [ ] Verify logs, traces, metrics, audit records, crash reports, and email-delivery failures contain no usable secrets or tokens.
- [ ] Verify Production services, buckets, tasks, jobs, database, and internal endpoints are not publicly accessible beyond intended entry points.
- [ ] Review IAM grants against the documented least-privilege matrix.
- [ ] Perform a structured threat-model review of registration, recovery, MFA loss, account linking, invitations, organization switching, and ownership transfer.

## 16. Automated Test Completion

### 16.1 Unit and Contract Tests

- [ ] Cover password, email, role, token, invitation, membership, and organization invariants with deterministic unit tests.
- [ ] Cover every allowed and rejected state transition.
- [ ] Cover clock-dependent expiry with an injected clock.
- [ ] Cover retry and idempotency behavior without a real database in application-component tests.
- [ ] Validate OpenAPI compatibility and generated client contracts.

### 16.2 Persistence Tests

- [ ] Test only the isolated persistence, migration, and recovery components against real PostgreSQL.
- [ ] Verify unique email, external subject, organization code, membership, invitation, and idempotency constraints.
- [ ] Verify transaction boundaries for registration email, invitation email, membership approval, ownership transfer, and audit creation.
- [ ] Verify concurrent token consumption, invitation acceptance, join approval, role changes, and last-Owner operations.
- [ ] Verify migrations from an empty and supported prior database state.
- [ ] Do not use EF Core InMemory as a PostgreSQL substitute.

### 16.3 Adapter and Integration Tests

- [ ] Test SMTP behavior through a fake or controlled Development server without coupling application tests to the provider.
- [ ] Test Google and Apple validation using provider fixtures, sandboxes, or approved fakes.
- [ ] Test Secret Manager, Cloud Tasks, Scheduler, and Cloud Run integration only at their infrastructure boundary.
- [ ] Test Web cookies and CSRF through the actual HTTP pipeline.
- [ ] Test OAuth/OIDC authorization, PKCE, refresh rotation, and revocation through the actual backend pipeline.

### 16.4 End-to-End Tests

- [ ] Complete email-and-password registration and confirmation.
- [ ] Complete Google and Apple registration, login, linking, and unlinking.
- [ ] Complete login lockout, recovery, password change, email change, and session revocation.
- [ ] Complete MFA enrollment, sign-in challenge, recovery-code use, regeneration, and required-role enforcement.
- [ ] Complete organization creation, switching, settings update, and access-code renewal.
- [ ] Complete invitation send, resend, revoke, decline, expiry, and acceptance with new and existing accounts.
- [ ] Complete access-code join request, approval, rejection, and pending-access denial.
- [ ] Complete role change, suspension, reactivation, removal, departure, and ownership transfer.
- [ ] Complete account export and deletion constraints.
- [ ] Repeat critical flows on Web, iOS, and Android using their real authentication and deep-link paths.

## 17. Observability and Operations

- [ ] Instrument API, authentication, email, outbox, task, job, database, and external-provider operations with OpenTelemetry.
- [ ] Propagate correlation identifiers through HTTP requests, outbox messages, Tasks, Jobs, email delivery, and audit events.
- [ ] Define structured event names and safe fields for authentication and organization operations.
- [ ] Create dashboards for availability, latency, error rate, sign-in failures, lockouts, MFA failures, email failures, outbox age, task retries, and database saturation.
- [ ] Alert on sustained availability failures, elevated authentication errors, email backlog, task failures, secret-access anomalies, database capacity, and unexpected spending.
- [ ] Configure log exclusions for noisy low-value events and retain required security evidence.
- [ ] Verify Error Reporting groups unhandled exceptions without exposing secrets.
- [ ] Verify Cloud Trace captures representative paths within the free allowance.
- [ ] Create runbooks for account compromise, MFA loss, email outage, provider outage, leaked secret, bad migration, failed deployment, and Owner recovery.
- [ ] Ensure support procedures never bypass authentication, MFA, organization authority, or audit requirements.

## 18. CI/CD

### 18.1 Pull Request Validation

- [ ] Configure GitHub Actions Linux workflows for backend restore, formatting, build, unit tests, integration tests, and security checks.
- [ ] Validate Web formatting, type checking, build, tests, and production bundle generation.
- [ ] Validate Rust formatting, linting, tests, and Linux-compatible build targets.
- [ ] Validate OpenAPI compatibility and generated-contract drift.
- [ ] Validate Terraform formatting, initialization, validation, security rules, and Production plan generation without applying changes.
- [ ] Enforce required checks and pull requests on the protected default branch.
- [ ] Configure Actions to stop when the included-usage budget is exhausted instead of generating charges.
- [ ] Keep mobile builds outside GitHub Actions and document the required local Mac verification evidence.

### 18.2 Production Deployment

- [ ] Build immutable API, Web, worker, and migration artifacts from the approved commit.
- [ ] Publish container images to Artifact Registry with commit and component-version identifiers.
- [ ] Obtain owner approval before the Production job proceeds.
- [ ] Authenticate to Google Cloud through Workload Identity Federation.
- [ ] Apply the reviewed Terraform plan when infrastructure changes are present.
- [ ] Run the dedicated migration job and stop deployment on migration failure.
- [ ] Deploy new Cloud Run revisions with zero traffic.
- [ ] Run health and smoke tests using isolated technical data only.
- [ ] Shift traffic gradually while monitoring errors, latency, saturation, authentication, and email behavior.
- [ ] Return traffic to the prior revision automatically or procedurally when release criteria fail.
- [ ] Record deployed component versions, migration version, commit, approver, and result.

## 19. Mobile Build and Distribution Readiness

- [ ] Create distinct Development and Production bundle/application identifiers and external identity configurations.
- [ ] Configure signing material under owner-controlled secure storage and keep it out of GitHub.
- [ ] Configure iOS entitlements, Sign in with Apple, Universal Links, privacy declarations, and required usage descriptions.
- [ ] Configure Android signing, App Links, OAuth fingerprints, network security, backup exclusions, and privacy declarations.
- [ ] Verify Development builds cannot connect accidentally to Production and Production builds cannot use Development credentials.
- [ ] Produce signed internal-test builds for iOS and Android from the owner's Mac.
- [ ] Run the shared-core release and language-bridge verification before each mobile build.
- [ ] Complete TestFlight and Google Play internal-testing distribution when store testing is required.
- [ ] Record the manual build, test, signing, upload, and rollback procedure.

## 20. Release Documentation

- [ ] Document setup and recovery for each local development dependency.
- [ ] Document Development secret bootstrap and refresh without exposing values.
- [ ] Document each required external account, client identifier, callback, domain, and owner.
- [ ] Document the role and permission matrix implemented in Version 0.0.1.
- [ ] Document authentication, MFA, session, invitation, membership, and ownership state diagrams or transition tables.
- [ ] Document token purposes, lifetimes, rotation, invalidation, and storage rules.
- [ ] Document data retention, export, deletion, and anonymization behavior.
- [ ] Document Production deployment, rollback, secret rotation, and incident procedures.
- [ ] Update `ExternalServices.md` if any provider, quota, or paid service changes during implementation.
- [ ] Update `Data.md`, `UseCases.md`, or `ARCHITECTURE.md` before accepting behavior that differs from their approved definitions.

## 21. Final Release Gate

### 21.1 Functional Acceptance

- [ ] Every item in `Version-0.0.1.md` is implemented or explicitly removed from the approved scope.
- [ ] Registration, confirmation, authentication, recovery, MFA, sessions, external identity, account lifecycle, organization, invitation, join, membership, and ownership flows pass acceptance testing.
- [ ] Web, iOS, and Android provide every flow applicable to their platform.
- [ ] Manager and Operator work as membership roles without requiring sectors or later operational entities.
- [ ] No billing, sector, shift, checklist, or customer-data dependency is required to operate the release.

### 21.2 Security Acceptance

- [ ] The complete role and organization-isolation test matrix passes.
- [ ] Token replay, account enumeration, brute force, unsafe redirect, CSRF, session fixation, refresh reuse, and privilege-escalation tests pass.
- [ ] No usable secret, credential, token, recovery code, or signing key exists in source control, artifacts, telemetry, or Terraform state.
- [ ] Owner, Admin, and Manager cannot exercise privileged organization access without MFA.
- [ ] The last active Owner invariant holds under normal, failure, retry, and concurrent operations.
- [ ] A security review approves authentication, authorization, IAM, external identity, email links, and deep links.

### 21.3 Quality Acceptance

- [ ] All required automated test categories pass from a clean environment.
- [ ] Web accessibility and supported responsive layouts pass review.
- [ ] iOS and Android accessibility, secure storage, deep links, and lifecycle behavior pass review.
- [ ] Database migrations succeed from an empty Production-equivalent database.
- [ ] No unresolved critical or high-severity defect remains.
- [ ] Known lower-severity limitations are documented with owner and follow-up version.

### 21.4 Operational Acceptance

- [ ] Production infrastructure is reproducible from reviewed Terraform configuration plus its documented bootstrap.
- [ ] CI/CD deploys a zero-traffic revision, validates it, promotes it, and supports rollback.
- [ ] Dashboards, alerts, audit events, correlation, and runbooks are verified with controlled tests.
- [ ] Production access matches the approved IAM matrix and remains restricted to the owner where required.
- [ ] External service usage remains within the approved Version 0.0.1 cost limits.
- [ ] Backups remain disabled until the documented First Customer gate and no customer data is stored before that gate.

### 21.5 Release Completion

- [ ] Assign and record the final backend, Web, Rust Core, iOS, and Android versions.
- [ ] Tag the approved component commits using the documented conventions.
- [ ] Record release notes, migrations, external configuration changes, known limitations, and rollback instructions.
- [ ] Archive the completed test evidence and owner approval.
- [ ] Mark Version 0.0.1 complete only after every applicable release-gate item is checked.
