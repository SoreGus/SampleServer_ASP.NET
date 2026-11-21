# Checklist — Persisted Data

## Scope

This document defines data persisted by the Checklist product. PostgreSQL stores relational state and metadata. Binary files remain outside the database: Development uses the local file system and Production uses private Google Cloud Storage.

The entities are logical requirements. ASP.NET Core Identity, OpenIddict, and EF Core may use framework-specific physical tables, but they must not create duplicate authoritative records.

Technical logs, metrics, and traces are not application data and remain in the observability platform. Reports are queries over existing records unless saved reports become a future requirement.

## Conventions

- Primary keys are UUIDs.
- Technical timestamps use `timestamptz` and UTC; business dates and local times retain the organization's IANA time zone.
- Every entity has `CreatedAt`; mutable entities also have `UpdatedAt` and a concurrency token.
- Every organization-owned entity includes `OrganizationId`. Composite foreign keys and unique constraints preserve the tenant boundary in the database.
- Status and type names below are logical enums. Unknown persisted values must never be silently accepted.
- `jsonb` is used only for validated structures whose shape varies by configured input or event type. Every stored document carries a schema version.
- Passwords, usable tokens, recovery codes, secrets, and payment instruments are never stored in plaintext.

## Identity and Access

ASP.NET Core Identity owns users, credentials, confirmation, recovery, lockout, and identity security state. OpenIddict owns protocol registrations, authorizations, scopes, grants, and token metadata.

| Entity | Required data | Purpose |
|---|---|---|
| `User` | `Id`<br>`Name varchar(150)`<br>`Email varchar(255)`<br>`NormalizedEmail varchar(255) UNIQUE`<br>`EmailConfirmedAt timestamptz NULL`<br>`PasswordHash text NULL`<br>`Status` — `Active, Blocked, Disabled`<br>`FailedLoginAttempts integer`<br>`LockedUntil timestamptz NULL`<br>`SecurityStamp text`<br>`LastLoginAt timestamptz NULL` | Global account. `PasswordHash` is optional for an external-only account. No phone number is required. |
| `ExternalIdentity` | `Id`<br>`UserId FK User`<br>`Provider` — `Google, Apple`<br>`ProviderSubject varchar(255)`<br>`ProviderEmail varchar(255) NULL`<br>`LastLoginAt timestamptz NULL`<br>`UNIQUE (Provider, ProviderSubject)` | Links an account by the provider's stable subject. Matching email never links accounts automatically. |
| `UserActionToken` | `Id`<br>`UserId FK User`<br>`Type` — `EmailConfirmation, PasswordRecovery`<br>`TokenHash varchar(128) UNIQUE`<br>`ExpiresAt`<br>`UsedAt NULL`<br>`RevokedAt NULL` | Enforces temporary, single-use confirmation and recovery flows without storing usable tokens. A replacement revokes prior active tokens of the same type. |
| `UserMfa` | `UserId PK/FK User`<br>`SecretEncrypted text NULL`<br>`EnabledAt NULL`<br>`DisabledAt NULL`<br>`LastConfirmedAt NULL` | Stores TOTP enrollment and activation state. Role requirements are evaluated from active organization memberships. |
| `MfaRecoveryCode` | `Id`<br>`UserId FK User`<br>`CodeHash varchar(128)`<br>`UsedAt NULL`<br>`RevokedAt NULL`<br>`UNIQUE (UserId, CodeHash)` | Single-use recovery codes. Regeneration revokes every unused previous code. |
| `UserSession` | `Id`<br>`UserId FK User`<br>`ClientType` — `Web, iOS, Android`<br>`DeviceLabel varchar(200) NULL`<br>`CreatedAt`<br>`LastSeenAt`<br>`ExpiresAt NULL`<br>`RevokedAt NULL`<br>`RevocationReason NULL` | Independently identifiable and revocable session. Web uses its protected cookie; mobile tokens belong to a session. |
| `RefreshToken` | `Id`<br>`SessionId FK UserSession`<br>`TokenHash varchar(128) UNIQUE`<br>`ExpiresAt`<br>`ConsumedAt NULL`<br>`RevokedAt NULL`<br>`ReplacedByTokenId NULL FK RefreshToken` | Mobile refresh-token rotation and reuse detection. A detected reuse revokes the session family. |

## Organizations and People

| Entity | Required data | Purpose |
|---|---|---|
| `Organization` | `Id`<br>`Name varchar(160)`<br>`LegalName varchar(200) NULL`<br>`TaxId varchar(30) NULL`<br>`AccessCode varchar(20) UNIQUE`<br>`TimeZone varchar(100)`<br>`Latitude numeric(9,6) NULL`<br>`Longitude numeric(9,6) NULL`<br>`AllowedRadiusMeters integer NULL`<br>`Status` — `Active, Suspended, Disabled` | Tenant and default location boundary. Subscription access is stored separately. |
| `OrganizationMember` | `Id`<br>`OrganizationId FK Organization`<br>`UserId FK User`<br>`SupervisorMemberId NULL FK OrganizationMember`<br>`Role NULL` — `Owner, Admin, Manager, Operator`<br>`Status` — `Pending, Active, Suspended, Removed`<br>`ApprovedByMemberId NULL`<br>`ApprovedAt NULL`<br>`UNIQUE (OrganizationId, UserId)` | Membership, role, pending access-code request, and organization chart. `Role` may be null only while approval is pending. |
| `OrganizationInvitation` | `Id`<br>`OrganizationId`<br>`Email varchar(255)`<br>`NormalizedEmail varchar(255)`<br>`Role` — `Admin, Manager, Operator`<br>`InvitedByMemberId`<br>`TokenHash varchar(128) UNIQUE`<br>`ExpiresAt`<br>`AcceptedByUserId NULL`<br>`AcceptedAt NULL`<br>`RevokedAt NULL`<br>one active invitation per organization and normalized email | Single-use invitation with a predefined role. |
| `Sector` | `Id`<br>`OrganizationId`<br>`ParentSectorId NULL`<br>`Name varchar(160)`<br>`Description text NULL`<br>`Latitude numeric(9,6) NULL`<br>`Longitude numeric(9,6) NULL`<br>`AllowedRadiusMeters integer NULL`<br>`IsActive boolean` | Hierarchical operational area with an optional location override. |
| `SectorMember` | `Id`<br>`OrganizationId`<br>`SectorId`<br>`MemberId`<br>`UNIQUE (SectorId, MemberId)` | Associates Managers and Operators with one or more sectors. |
| `Shift` | `Id`<br>`OrganizationId`<br>`SectorId`<br>`Name varchar(120)`<br>`StartTime time`<br>`EndTime time`<br>`ScheduleType` — `Weekly, Monthly, SpecificDate`<br>`DaysOfWeek smallint[] NULL`<br>`DaysOfMonth smallint[] NULL`<br>`SpecificDate date NULL`<br>`ValidFrom date NULL`<br>`ValidUntil date NULL`<br>`IsActive boolean` | Sector shift, recurrence, and validity. Overnight interpretation uses the organization time zone. |
| `ShiftMember` | `Id`<br>`OrganizationId`<br>`ShiftId`<br>`MemberId`<br>`ValidFrom date`<br>`ValidUntil date NULL`<br>`UNIQUE (ShiftId, MemberId, ValidFrom)` | Effective-dated assignment of a sector member to a shift. An omitted start in the user flow is persisted as the effective assignment date. |

## Checklist Definitions and Scheduling

| Entity | Required data | Purpose |
|---|---|---|
| `Checklist` | `Id`<br>`OrganizationId`<br>`FamilyId uuid`<br>`PreviousVersionId NULL`<br>`Name varchar(180)`<br>`Description text NULL`<br>`VersionNumber integer`<br>`Status` — `Draft, PendingApproval, Published, Archived`<br>`CreatedByMemberId`<br>`ApprovedByMemberId NULL`<br>`ApprovedAt NULL`<br>`PublishedAt NULL`<br>`UNIQUE (OrganizationId, FamilyId, VersionNumber)` | Immutable published process version. A change creates a linked draft in the same family. |
| `Task` | `Id`<br>`OrganizationId`<br>`ChecklistId`<br>`Name varchar(180)`<br>`Description text NULL`<br>`Instructions text NULL`<br>`InputTypes smallint[]` — `Boolean, Text, Number, SingleChoice, MultipleChoice, Photo, Video, Date, Time, Signature`<br>`Configuration jsonb NULL`<br>`ScheduledTime time NULL`<br>`IsRequired boolean`<br>`RequiresLocation boolean`<br>`SortOrder integer`<br>`UNIQUE (ChecklistId, SortOrder)` | Ordered step and its validated input configuration. Expected values are part of the immutable published version. |
| `TaskInstructionFile` | `Id`<br>`OrganizationId`<br>`TaskId`<br>`FileAssetId`<br>`SortOrder integer`<br>`UNIQUE (TaskId, FileAssetId)` | Connects private instructional media to a task with referential integrity. |
| `ChecklistSchedule` | `Id`<br>`OrganizationId`<br>`ChecklistId`<br>`SectorId`<br>`ShiftId NULL`<br>`ScheduledTime time`<br>`ScheduleType` — `Daily, Weekly, Monthly, SpecificDate`<br>`DaysOfWeek smallint[] NULL`<br>`DaysOfMonth smallint[] NULL`<br>`SpecificDate date NULL`<br>`ValidFrom date`<br>`ValidUntil date NULL`<br>`ToleranceMinutes integer`<br>`IsActive boolean` | Defines future occurrences for a published checklist. |
| `ChecklistScheduleMember` | `Id`<br>`OrganizationId`<br>`ScheduleId`<br>`MemberId`<br>`UNIQUE (ScheduleId, MemberId)` | Optional explicit assignees. Without rows, assignees resolve from sector and shift. |
| `ChecklistEscalationLevel` | `Id`<br>`OrganizationId`<br>`ChecklistId`<br>`Trigger` — `LateSubmission, LateReview, CriticalDivergence`<br>`LevelNumber smallint`<br>`RecipientMemberId`<br>`DelayMinutes integer`<br>`UNIQUE (ChecklistId, Trigger, LevelNumber)` | Ordered escalation configuration for each trigger. |

## Operational Records

| Entity | Required data | Purpose |
|---|---|---|
| `ChecklistRecord` | `Id`<br>`OrganizationId`<br>`ChecklistId`<br>`ScheduleId NULL`<br>`OccurrenceKey varchar(200)`<br>`SectorId`<br>`ShiftId NULL`<br>`AssignedMemberId NULL`<br>`ExecutedByMemberId NULL`<br>`ScheduledFor NULL`<br>`DueAt NULL`<br>`ScheduleSnapshot jsonb NULL`<br>`StartedAt NULL`<br>`CompletedAt NULL`<br>`AcceptedAt NULL`<br>`Status` — `Pending, InProgress, Completed, CompletedWithIssues, Missed, Cancelled`<br>`Latitude NULL`<br>`Longitude NULL`<br>`LocationRuleSnapshot jsonb NULL`<br>`IsInsideAllowedArea NULL`<br>`CancelledByMemberId NULL`<br>`CancelledAt NULL`<br>`ReviewedByMemberId NULL`<br>`ReviewDueAt NULL`<br>`ReviewedAt NULL`<br>`ReviewStatus NULL` — `Pending, Approved, Rejected`<br>`ReviewNotes NULL`<br>`UNIQUE (OrganizationId, OccurrenceKey)` | Expected execution, immutable scheduling context, completion, location result, cancellation, and final review. |
| `TaskRecord` | `Id`<br>`OrganizationId`<br>`ChecklistRecordId`<br>`TaskId`<br>`ExecutedByMemberId NULL`<br>`Value jsonb NULL`<br>`Notes text NULL`<br>`Status` — `Pending, Completed, Skipped, NotApplicable`<br>`DivergenceStatus NULL` — `WithinExpected, DifferentFromExpected, BelowMinimum, AboveMaximum`<br>`Latitude NULL`<br>`Longitude NULL`<br>`OccurredAt NULL`<br>`ReceivedAt NULL`<br>`ReviewStatus NULL` — `Pending, Confirmed, Escalated`<br>`ReviewedByMemberId NULL`<br>`ReviewedAt NULL`<br>`UNIQUE (ChecklistRecordId, TaskId)` | Validated response and review. Device occurrence and server receipt times remain distinct. |
| `TaskRecordFile` | `Id`<br>`OrganizationId`<br>`TaskRecordId`<br>`FileAssetId`<br>`InputKey varchar(100) NULL`<br>`UNIQUE (TaskRecordId, FileAssetId)` | Evidence attached to a response. A dependent response is accepted only after every required file is available. |
| `EscalationRecord` | `Id`<br>`OrganizationId`<br>`ChecklistRecordId`<br>`TaskRecordId NULL`<br>`EscalationLevelId`<br>`RecipientMemberId`<br>`Reason` — `LateSubmission, LateReview, CriticalDivergence`<br>`DeduplicationKey varchar(200) UNIQUE`<br>`TriggeredAt` | Immutable record of each escalation actually triggered. |
| `Notification` | `Id`<br>`OrganizationId`<br>`MemberId`<br>`Type` — `Alert, Checklist, Divergence, System`<br>`Title varchar(180)`<br>`Message text`<br>`ReferenceType varchar(50) NULL`<br>`ReferenceId uuid NULL`<br>`ReadAt NULL` | In-app notification and read state. Email delivery is tracked separately. |

## Files and Exports

| Entity | Required data | Purpose |
|---|---|---|
| `FileAsset` | `Id`<br>`OrganizationId`<br>`StorageKey varchar(500) UNIQUE`<br>`OriginalFileName varchar(255)`<br>`DeclaredContentType varchar(150)`<br>`DetectedContentType varchar(150) NULL`<br>`ExpectedSizeBytes bigint NULL`<br>`SizeBytes bigint NULL`<br>`Checksum varchar(128) NULL`<br>`Status` — `Pending, Available, Rejected, Deleted`<br>`CreatedByMemberId NULL`<br>`ConfirmedAt NULL`<br>`DeletedAt NULL` | Metadata and lifecycle for a private object. Binary content remains in local storage or Cloud Storage. |
| `DataExport` | `Id`<br>`OrganizationId`<br>`RequestedByMemberId`<br>`Status` — `Pending, Processing, Ready, Failed, Expired`<br>`FileAssetId NULL`<br>`RequestedAt`<br>`CompletedAt NULL`<br>`ExpiresAt NULL`<br>`FailureCode NULL` | Auditable organization export available to the Owner, including during restricted subscription access. |

Incomplete or rejected objects never become available through `TaskInstructionFile`, `TaskRecordFile`, or `DataExport`. Abandoned objects and expired exports are removed by background processing.

## Billing and Entitlements

| Entity | Required data | Purpose |
|---|---|---|
| `BillingCustomer` | `Id`<br>`OrganizationId UNIQUE`<br>`Provider` — `Stripe`<br>`ProviderCustomerId varchar(255) UNIQUE` | Maps one organization to its provider customer without storing payment instruments. |
| `Subscription` | `Id`<br>`OrganizationId`<br>`BillingCustomerId`<br>`ProviderSubscriptionId varchar(255) UNIQUE`<br>`ProviderPriceId varchar(255) NULL`<br>`PlanKey varchar(100) NULL`<br>`Status` — `Trialing, Active, PastDue, Canceled, Expired, Unpaid`<br>`CurrentPeriodStart NULL`<br>`CurrentPeriodEnd NULL`<br>`TrialEndsAt NULL`<br>`GraceEndsAt NULL`<br>`CancelAtPeriodEnd boolean`<br>`CanceledAt NULL`<br>`ProviderUpdatedAt NULL` | Local subscription projection. Historical subscriptions remain stored; only one may control current access. |
| `OrganizationEntitlement` | `Id`<br>`OrganizationId`<br>`Key varchar(100)`<br>`IsGranted boolean`<br>`SourceSubscriptionId NULL`<br>`ValidFrom`<br>`ValidUntil NULL`<br>`UNIQUE (OrganizationId, Key)` | Current access projection used by authorization. Prices and entitlement definitions remain configurable business policy. |
| `BillingEvent` | `Id`<br>`OrganizationId NULL`<br>`Provider` — `Stripe`<br>`ProviderEventId varchar(255) UNIQUE`<br>`ProviderCustomerId varchar(255) NULL`<br>`ProviderSubscriptionId varchar(255) NULL`<br>`EventType varchar(150)`<br>`PayloadHash varchar(128)`<br>`ReceivedAt`<br>`ProcessedAt NULL`<br>`Status` — `Pending, Processed, Failed`<br>`FailureCode NULL` | Verified webhook deduplication and reconciliation history. Card or payment-instrument details are not copied locally. |

## Delivery, Idempotency, and Audit

| Entity | Required data | Purpose |
|---|---|---|
| `OutboxMessage` | `Id`<br>`OrganizationId NULL`<br>`Type varchar(150)`<br>`AggregateType varchar(100) NULL`<br>`AggregateId uuid NULL`<br>`Payload jsonb`<br>`OccurredAt`<br>`AvailableAt`<br>`Attempts integer`<br>`ProcessedAt NULL`<br>`LastErrorCode NULL` | Transactional publication of background work after the owning database transaction commits. Payloads are minimized and exclude usable secrets. |
| `IdempotencyRecord` | `Id`<br>`OrganizationId NULL`<br>`UserId NULL`<br>`PartitionKey varchar(100)`<br>`Scope varchar(100)`<br>`Key varchar(200)`<br>`RequestHash varchar(128)`<br>`Status` — `Processing, Completed, Failed`<br>`ResultReference varchar(300) NULL`<br>`ExpiresAt`<br>`UNIQUE (PartitionKey, Scope, Key)` | Prevents duplicate API, offline synchronization, task, and side-effect processing. |
| `BackgroundExecution` | `Id`<br>`OrganizationId NULL`<br>`Operation varchar(150)`<br>`ExecutionKey varchar(200)`<br>`Status` — `Running, Succeeded, Failed`<br>`Attempts integer`<br>`StartedAt`<br>`CompletedAt NULL`<br>`FailureCode NULL`<br>`UNIQUE (Operation, ExecutionKey)` | Persists scheduled, batch, and terminal background outcomes for controlled retry. |
| `EmailDelivery` | `Id`<br>`OrganizationId NULL`<br>`UserId NULL`<br>`NotificationId NULL`<br>`RecipientEmail varchar(255)`<br>`TemplateName varchar(150)`<br>`TemplateVersion integer`<br>`IdempotencyKey varchar(200) UNIQUE`<br>`Status` — `Pending, Sent, Failed`<br>`Attempts integer`<br>`AcceptedAt NULL`<br>`LastErrorCode NULL` | Provider-neutral transactional email state. `Sent` means accepted by SMTP, not delivered to the recipient. |
| `AuditLog` | `Id`<br>`OrganizationId NULL`<br>`UserId NULL`<br>`MemberId NULL`<br>`Action varchar(100)`<br>`EntityType varchar(100)`<br>`EntityId uuid NULL`<br>`Data jsonb NULL`<br>`IpAddress varchar(45) NULL`<br>`CorrelationId uuid`<br>`CreatedAt` | Append-only security and business audit trail. Secrets and unnecessary sensitive values are excluded. |

## Client-Local Data

iOS uses SwiftData, Android uses Room, and Web uses IndexedDB through platform adapters. Their physical schemas may differ, but they persist the same logical categories:

| Local category | Required data | Purpose |
|---|---|---|
| `LocalContext` | User identifier, organization identifier, authorization context version, last synchronization cursor | Partitions and revalidates all local data. |
| `CachedDefinition` | Published checklist, tasks, schedule context, and version | Makes previously synchronized operational work available offline. |
| `LocalExecutionDraft` | Execution identifier, draft responses, notes, device timestamps, and local state | Preserves incomplete and locally completed work until backend acceptance. |
| `PendingCommand` | Client-generated identifier, idempotency key, type, validated payload, occurred time, retry state, and dependency identifiers | Immutable offline action waiting for synchronization. |
| `PendingFile` | Local protected-file reference, checksum, size, content type, related command, and transfer state | Protects evidence and resumes upload before its dependent command is accepted. |
| `SyncConflict` | Command identifier, reason code, server version or rejection context, and resolution state | Presents explicit conflicts and rejections without silent overwrite. |

Authentication tokens never enter these databases. Native tokens remain in Keychain or Keystore-backed storage; browser authentication remains in the protected server cookie. Local data is minimized, partitioned by user and organization, migrated explicitly, and removed or revalidated after sign-out or context changes.

## Location Inheritance

The effective location is resolved as one indivisible latitude, longitude, and radius configuration:

1. Execution sector.
2. First configured ancestor sector.
3. Organization.
4. No geographic restriction when none is configured.

`ChecklistRecord` preserves the effective rule snapshot, captured coordinates, and calculated result. `TaskRecord` preserves task-level coordinates when required. Later configuration changes never rewrite historical validation.

## Persistence Rules

- Tenant-owned references must use matching `OrganizationId` values; filtering in application code is not sufficient protection.
- A normalized email is globally unique, but a user may have one membership in each of multiple organizations.
- An access code creates a pending membership only. It never grants a role or organization access.
- An organization always retains at least one active Owner.
- Privileged access requires active MFA; recovery by email never disables MFA.
- External identities are unique by provider and subject and are linked to an existing account only after that account authenticates.
- Invitations and user action tokens expire, are single-use, and may be revoked or superseded.
- Supervisor relationships remain within one organization, cannot reference themselves, and cannot create cycles.
- Sector hierarchies remain within one organization and cannot contain cycles. Sector and shift assignments accept only members from their valid organizational context.
- Published checklist versions and accepted operational records are immutable. Corrections use new versions or explicit audited state transitions.
- The first checklist version establishes its `FamilyId`; every later version retains that family and links to its immediate predecessor.
- Schedules reference published checklist versions. Schedule changes affect future occurrences and never rewrite already generated records.
- Schedule generation uses `OccurrenceKey`; retries cannot create duplicate expected executions.
- Schedule, deadline, and location snapshots preserve the meaning of generated records after configuration changes.
- Offline commands, webhooks, outbox dispatch, email, and escalation handlers are idempotent.
- Mutable aggregates use optimistic concurrency; conflicts return explicit outcomes rather than overwriting newer data.
- Storage keys are generated by the backend and never derived directly from file names. Files become referencable only after validation and confirmation. Access always requires current authorization and a temporary URL or authenticated flow.
- Stripe remains the financial source of truth. Local subscriptions and entitlements change only after verified event processing and provider reconciliation.
- One nonterminal subscription controls an organization's current access. Expired and unpaid organizations retain only the Owner operations defined in the use cases.
- Audit records are append-only and remain separate from technical logs.
- Production data is never copied directly into Development; Development uses synthetic data.
