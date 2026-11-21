# Cross-Platform Application Architecture

## 1. Overview

The system uses independent native frontends, a shared Rust client core, and an ASP.NET Core backend hosted on Google Cloud. Infrastructure is managed with Terraform.

**Primary principle:** Share domain and application logic without sharing UI frameworks or coupling client code to server implementation.

## 2. Technology Stack

| Layer | Technology | Responsibility |
|---|---|---|
| Backend | C# / ASP.NET Core | REST APIs, server-side business rules, authorization, orchestration |
| Shared client core | Rust | Domain models, validation, use cases, portable algorithms and services |
| iOS | Swift / SwiftUI | Native UI, navigation, state, Apple platform integrations |
| Android | Kotlin / Jetpack Compose | Native UI, navigation, state, Android platform integrations |
| Web | React / TypeScript | Browser UI, navigation, state, browser integrations |
| Rust ↔ iOS/Android | UniFFI | Generated Swift/Kotlin bindings and supported callback interfaces |
| Rust ↔ Web | wasm-bindgen / WebAssembly | JavaScript/TypeScript interoperability |
| Backend container | Docker | Reproducible ASP.NET deployment |
| Backend compute | Google Cloud Run | Managed container execution and scaling |
| Web hosting | Cloud Run (static server or SSR) | Serve React assets and optional server rendering |
| Database | Cloud SQL for PostgreSQL | Central relational persistence |
| File storage | Google Cloud Storage | Private object storage for application files |
| Container registry | Artifact Registry | Store deployable container images |
| Infrastructure as code | Terraform | Provision and configure Google Cloud resources |
| Source control and CI/CD | GitHub / GitHub Actions | Private source hosting, automated validation, server builds, and deployments |
| Secrets | Secret Manager | Store runtime credentials and secrets |
| Service identity | Cloud IAM | Least-privilege identities and resource access |
| User identity | ASP.NET Core Identity, OpenIddict, Google, Apple | Local and external authentication through OAuth 2.0 and OpenID Connect |
| Billing | Stripe Billing | Hosted checkout, subscription management, and payment events |
| Background processing | C#/.NET, Cloud Scheduler, Cloud Tasks, Cloud Run Jobs | Scheduled, deferred, retried, and batch work |
| Email | SMTP / MailKit | Provider-neutral transactional email delivery |
| Observability | OpenTelemetry, Cloud Logging, Monitoring, Trace, Error Reporting | Logs, metrics, traces, exceptions, alerts |
| API contract | OpenAPI | Versioned external contract and optional typed-client generation |
| Database schema | EF Core Migrations | Versioned schema changes |
| Terraform state | Cloud Storage | Remote state with restricted access and versioning |

## 3. Logical Architecture

```text
+-------------------+ +--------------------+ +-----------------------+
| iOS               | | Android            | | Web                   |
| SwiftUI / Swift   | | Compose / Kotlin   | | React / TypeScript    |
| Native adapters   | | Native adapters    | | Browser adapters      |
| Rust Core         | | Rust Core          | | Rust Core (Wasm)      |
+---------+---------+ +---------+----------+ +-----------+-----------+
   |                           |                           |
   +---------------------------+---------------------------+
                               | HTTPS / JSON
                     +---------v----------+
                     | ASP.NET Core API   |
                     | Identity/OpenIddict|
                     | Docker / Cloud Run |
                     +---------+----------+
                               |
        +----------------------+----------------------+
        |                      |                      |
+-------v------------+ +-------v-----------+ +--------v-------------+
| Cloud SQL          | | Cloud Storage     | | External providers   |
| PostgreSQL         | | Private objects   | | Identity/Billing/SMTP|
+--------------------+ +-------------------+ +----------------------+

Background processing: Cloud Scheduler, Cloud Tasks, Cloud Run Jobs.
Cross-cutting: Terraform, Artifact Registry, IAM, Secret Manager,
CI/CD, Google Cloud Observability, OpenTelemetry.
```

The backend and shared client core are separate products: Rust is compiled into each client; the backend remains C# and communicates over versioned HTTP APIs.

## 4. Shared Rust Core

**Owns:**
- Domain entities, value objects, invariants, and client-side business rules.
- Use cases and workflow orchestration that must behave consistently across clients.
- Validation, transformation, serialization, and portable algorithms.
- Platform-independent ports (traits) for persistence, networking, secure storage, and device capabilities.
- Portable infrastructure implementations only where they are genuinely supported across all target environments.

**Does not own:**
- UI components, navigation, accessibility, platform presentation state, or OS-specific permissions.
- Direct dependencies on SwiftData, UIKit, Android Room, Keychain, or browser DOM APIs.
- Server-only authorization, trusted business enforcement, database credentials, or secrets.

Client-side validation improves UX; the ASP.NET backend independently enforces all security-sensitive rules.

### Platform adapters and dependency injection

Use constructor injection and Rust traits. Each frontend creates its own composition root and provides platform adapters:

| Port | iOS adapter | Android adapter | Web adapter |
|---|---|---|---|
| Persistence | SwiftData | Room | IndexedDB |
| HTTP | URLSession | OkHttp | Fetch API |
| Secure storage | Keychain | Android Keystore-backed solution | No client-side authentication storage |
| Device features | Apple frameworks | Android APIs | Browser APIs, where available |

UniFFI can expose Rust objects and callback interfaces to Swift/Kotlin; browser integration uses a separate wasm-bindgen-compatible bridge. **Do not assume a Rust trait automatically becomes an injectable TypeScript interface.** Explicit adapters, asynchronous boundaries, error conversion, ownership, cancellation, and threading must be designed and tested.

Prefer coarse-grained use-case calls over numerous fine-grained cross-language calls. For OS APIs constrained to a main thread or actor (for example SwiftData ModelContext), bridge scheduling and async behavior explicitly.

## 5. Native Frontends

### iOS
- SwiftUI for UI and navigation; Observation for presentation state.
- SwiftData, URLSession, Keychain, and Apple APIs through Swift adapters.
- Rust compiled as an Apple-target library; UniFFI generates Swift bindings.

### Android
- Kotlin and Jetpack Compose for UI and navigation.
- Room, OkHttp, Keystore, and Android APIs through Kotlin adapters.
- Rust compiled for supported Android ABIs, packaged with the application; UniFFI generates Kotlin bindings.

### Web
- React + TypeScript for UI, routing, and browser-specific state.
- Rust compiled to WebAssembly; wasm-bindgen provides JavaScript interoperability.
- Fetch, IndexedDB, and browser APIs through explicit TypeScript adapters.
- Host the frontend on Cloud Run using a static server, or use an SSR-capable runtime if needed.
- Browser Wasm has different threading, file-system, and networking constraints than native Rust; test dependencies against the browser target.

## 6. Client State, Offline Operation, and Synchronization

The backend remains the source of truth. Clients support offline operation for operational workflows, while administrative configuration, access management, billing, and other privileged mutations remain online-only.

Initial sign-in and session renewal require network access. Local databases store only authorized cache data, drafts, a pending-command outbox, and synchronization metadata. Authentication tokens remain in platform secure storage and never enter the local application database.

Offline actions create immutable local commands with client-generated identifiers and idempotency keys. When connectivity returns, the Rust Core synchronization engine submits pending work through the platform network adapter.

The backend revalidates authorization, current rules, and referenced resource versions during synchronization. Device event time and server receipt time are preserved separately. A local action is not considered accepted until the backend confirms it.

Files captured offline remain protected in platform storage and upload after reconnection. A submission that depends on evidence remains pending until every required upload is confirmed.

Conflicts, expired access, or incompatible changes never cause silent overwrites. The client presents an explicit resolution or rejection state.

Local data is partitioned by identity and access context, minimized, expired when no longer needed, and removed or revalidated after sign-out or context changes. Local schemas have explicit migrations.

The Rust Core owns synchronization state and portable rules. SwiftData, Room, and IndexedDB provide platform persistence adapters without becoming sources of business authority.

## 7. ASP.NET Core Backend

Start with a modular monolith, with clear domain, application, infrastructure, and API boundaries.

- REST/JSON over HTTPS, described by OpenAPI.
- Validate browser sessions or access tokens and enforce authorization on every protected operation.
- Use EF Core with Npgsql for PostgreSQL; manage schema changes with migrations.
- Expose health endpoints and OpenTelemetry instrumentation.
- Limit database connection pools to account for Cloud Run autoscaling and Cloud SQL connection limits.
- Add queues, caching, or independent services only when justified by requirements.

## 8. Authentication Authority and Sign-in Methods

ASP.NET Core is the system's identity authority. ASP.NET Core Identity manages users, local credentials, email confirmation, account recovery, lockout, and identity security state. OpenIddict provides the OAuth 2.0 and OpenID Connect flows required by the Web, iOS, and Android clients.

Supported sign-in methods are:

- Email and password.
- Google.
- Apple.

External identities are linked by the provider's stable subject identifier, never by email alone. When an email already belongs to an account, the user must authenticate with that account before linking another sign-in method. An external-only account does not require a local password.

Authentication establishes identity only. Application authorization is evaluated separately by the backend. A sign-in method never grants application access by itself.

Identity data is stored in PostgreSQL: locally in Development and in Cloud SQL for Production. The database stores user details, password hashes, email confirmation state, security state, external identity links, and the minimum data required for recovery and session invalidation.

Passwords, confirmation tokens, recovery tokens, provider credentials, and other authentication secrets are never stored in plaintext. Provider passwords are never received or stored.

Identity records remain separate from application-specific authorization and access records.

## 9. Sessions and Tokens

The Web client uses an encrypted ASP.NET Core authentication cookie marked HttpOnly, Secure, and SameSite. Authentication tokens are never exposed to React or stored in browser storage. Requests that change state require CSRF protection.

iOS and Android use the OAuth 2.0 Authorization Code Flow with PKCE through the system browser. Access tokens are JWTs valid for 15 minutes. Refresh tokens are valid for 30 days and rotate after every use. Reuse of an invalidated refresh token revokes the entire session family.

Mobile clients store tokens only in Keychain or a Keystore-backed solution. Each device has an independent, identifiable, and revocable session.

Access tokens contain only stable user and session identifiers and required protocol claims. They are not authorization snapshots.

PostgreSQL stores session and token metadata plus protected refresh-token references or cryptographic hashes. Usable refresh-token values are never stored in plaintext.

Signing out revokes the current session. Signing out from all devices, account recovery, account blocking, or suspected compromise revokes every active session for the user.

## 10. Multi-Factor Authentication and Recovery

Users are classified by security importance rather than by application-specific role names:

- **Privileged users:** Users with administrative access or authority over high-impact actions. MFA is required.
- **Standard users:** Users with ordinary application access. MFA is optional unless the application policy requires it.

MFA uses time-based one-time passwords generated by a compatible authenticator application. Email supports confirmation, recovery, and security alerts but is not treated as a second factor. SMS authentication is not used.

External sign-in does not bypass an application MFA requirement. MFA secrets are encrypted at rest, and their Production protection keys remain in Secret Manager.

Recovery codes are single-use, stored only as cryptographic hashes, and invalidated when regenerated. Removing or resetting MFA requires recent authentication plus the current factor or a recovery code. Email recovery alone cannot disable MFA.

Sensitive actions may require recent MFA confirmation. MFA activation, removal, recovery, and relevant failures are audited.

## 11. Authorization and Access Enforcement

ASP.NET Core uses policy-based authorization. Authentication is required by default, and access is denied unless an applicable policy explicitly allows the requested action.

The backend resolves current roles, permissions, and application scopes. Tokens identify the user and session but never determine access by themselves. Authorization changes therefore take effect without waiting for token expiration.

Operations on protected resources use resource-based authorization that evaluates both the requested action and the loaded resource. Client-provided identifiers never prove access.

Queries and mutations apply the authorized scope before reading or changing data. Client applications may hide unavailable actions for usability, but the ASP.NET Core backend remains the only security authority.

Privileged actions and relevant denied-access attempts are audited.

## 12. Payments, Subscriptions, and Entitlements

Stripe Billing is the initial payment provider and is isolated behind a backend-owned billing abstraction so another provider can be added or substituted without changing application authorization rules.

Checkout and subscription management use Stripe-hosted pages. The application never receives or stores card details or other sensitive payment-instrument data.

Stripe is the financial source of truth. The backend maintains a local projection of subscription state and entitlements for reliable access enforcement. Access is never granted from a browser redirect alone.

Subscription changes are synchronized through signed webhooks. Events are verified, deduplicated, processed idempotently, and reconciled with the current provider state before changing local access.

PostgreSQL stores only provider identifiers, subscription state, billing periods, cancellation metadata, processed event identifiers, and the minimum data required for reconciliation.

Development uses Stripe Test Mode. Production credentials remain in Secret Manager and are available only to the billing workload. Billing actions are restricted to privileged users and may require recent MFA confirmation.

Plans, prices, grace periods, and application-specific access rules remain outside the architecture document.

## 13. File Storage and Delivery

Application file storage is isolated behind a storage abstraction. Development uses the local file system; Production uses private Google Cloud Storage buckets.

Binary content remains in object storage. PostgreSQL stores only file metadata, state, ownership context, and the object reference.

Public bucket access is permanently disabled. After authorizing the operation, the backend issues short-lived, operation-specific signed URLs for uploads or downloads. Signed URLs are transmitted only over HTTPS.

Large files, photos, and videos transfer directly between the client and Cloud Storage instead of passing through Cloud Run. The backend creates an upload intent before transfer and confirms availability only after validating size, content type, checksum, and object metadata.

Object keys are generated by the backend and never derived directly from client-provided file names. Incomplete and orphaned uploads are removed automatically.

Every download requires current authorization before a temporary URL is issued. Runtime service accounts receive only the bucket permissions required by their workloads.

Retention, deletion, and file-size policies are configurable by the application.

## 14. Background Processing and Scheduling

All background logic is written in C#/.NET and reuses the backend domain and application layers. Cloud services coordinate execution but do not contain business rules.

Development runs scheduling, outbox dispatch, and workers locally through .NET hosted background services.

In Production, Cloud Scheduler triggers recurring operations, Cloud Tasks delivers immediate or delayed work to private ASP.NET Core handlers, and Cloud Run Jobs executes finite batch, maintenance, and migration workloads.

Operations that must persist data and schedule asynchronous work use a transactional outbox stored with the application transaction in PostgreSQL. The dispatcher publishes pending work after the transaction commits.

Tasks carry only identifiers and minimal execution context. Workers load current data before processing. Delivery is at least once, so every handler is idempotent and uses stable deduplication keys or processing records where side effects require them.

Transient failures use bounded retries with exponential backoff. Terminal failures are persisted, monitored, and available for controlled reprocessing.

Internal calls use service identities. Technical timestamps use UTC.

## 15. Transactional Email Delivery

The application layer defines a provider-neutral email delivery interface. SMTP through MailKit is the only implementation in the initial architecture.

Development and Production both send real email through SMTP using separate configuration and credentials. Connections use TLS on the secure submission ports supported by the selected SMTP server.

The backend creates email messages from version-controlled templates and places delivery work in the transactional outbox. Background workers send messages asynchronously with internal idempotency and bounded retries for transient failures.

Delivery state is recorded as Pending, Sent, or Failed. Sent means that the SMTP server accepted the message; it does not guarantee delivery to the recipient. Permanent failures remain available for diagnosis and controlled reprocessing.

Email transport contains no provider-specific domain rules or persistence models. A future API-based provider can be added as another adapter without changing templates, application workflows, or existing business logic. No future adapter is implemented in advance.

Email supports confirmation, recovery, and application alerts. SMS delivery is not supported.

## 16. Observability and Audit Boundaries

OpenTelemetry is the standard instrumentation layer for traces and metrics across the API, background workloads, and external integrations.

Development exposes structured logs, metrics, and traces locally. Production sends structured logs to Cloud Logging, metrics to Cloud Monitoring, traces to Cloud Trace, and unhandled exceptions to Error Reporting.

Application logs are emitted as structured JSON through standard output and error streams. Trace, span, and correlation identifiers connect requests, database operations, outbox entries, tasks, jobs, emails, uploads, webhooks, and external calls.

Telemetry never contains passwords, tokens, secrets, payment data, or unnecessary sensitive content. Redaction occurs before data leaves the application process.

Health checks distinguish process liveness from dependency readiness. Alerts cover error rate, latency, unavailable dependencies, failed jobs, queue backlog, database pressure, and external integration failures.

Sampling, retention, and log exclusions control telemetry volume and cost without removing security-relevant failures.

Technical telemetry supports diagnosis and operational monitoring. Application audit records are separate, immutable business or security data with independent retention and access policies. Technical logs never replace the audit trail.

## 17. Relational Data, Migrations, and Recovery

PostgreSQL is the only relational database. Development uses a local instance; Production uses Cloud SQL for PostgreSQL. Only the backend and authorized internal workloads access the database.

EF Core with Npgsql manages persistence and versioned schema migrations. Runtime and migration workloads use separate credentials, and the runtime identity cannot modify the schema.

Migrations are reviewed and executed by a dedicated Cloud Run Job during deployment. The API never applies migrations automatically at startup.

Schema changes remain compatible with the active and previous application revisions. Destructive changes use an expand-migrate-contract sequence so controlled rollout and rollback do not depend on an immediate breaking migration.

Connection pool limits and Cloud Run scaling limits are coordinated with the Cloud SQL connection capacity.

Before Production has an active customer, the database is disposable, contains no real customer data, and keeps automated backups and point-in-time recovery disabled. Enabling both protections is a prerequisite for activating the first customer. After activation, they remain mandatory under normal operation.

Restore tests begin after valid backups exist and restore into a separate instance. Multi-zone high availability is not enabled initially and may be added when a formal availability requirement justifies its cost.

Development uses synthetic data. Production data is never copied directly into Development.

## 18. API Contract, Versioning, and Compatibility

The backend exposes a versioned REST/JSON API. Major versions appear in the URL, beginning with `/api/v1`.

The OpenAPI document generated by ASP.NET Core is the authoritative external contract. Request and response DTOs remain separate from domain and persistence models, and EF Core entities are never serialized directly.

Compatible additions remain in the current major version. Breaking changes create a new major version. The previous version remains available during an explicit migration period for independently released clients, and usage is monitored before deprecation and removal.

CI compares OpenAPI changes and blocks accidental contract breaks. Generated clients may use the OpenAPI contract, but generated transport models never replace application domain models.

Errors use Problem Details with stable application error codes, safe messages, and correlation identifiers. Internal exceptions and implementation details are never returned to clients.

Input validation rejects malformed or invalid requests before the application layer. Growing collections use cursor-based pagination. Dates use ISO 8601 with UTC or an explicit offset.

Operations exposed to client or network retries accept idempotency keys. Concurrency conflicts return explicit responses instead of silently overwriting newer data.

Request-size limits and rate-limiting policies are applied according to endpoint risk and cost.

## 19. Testing and Release Safety

Tests follow architectural boundaries. Application components receive ports and adapters through dependency injection; only persistence and recovery components access a real PostgreSQL instance.

| Category | Scope | Real database |
|---|---|:---:|
| Domain unit tests | Entities, value objects, invariants, and pure rules | No |
| Application tests | Use cases, commands, handlers, and orchestration with injected ports | No |
| API tests | Endpoints, validation, DTOs, HTTP status codes, and Problem Details | No |
| Worker tests | Jobs, retries, idempotency, and asynchronous processing | No |
| Authorization tests | Policies, permissions, and contextual access | No |
| Protocol contract tests | Port and adapter contract conformance | No |
| Persistence component tests | EF Core, Npgsql, mappings, queries, constraints, and transactions | Yes, isolated |
| Migration tests | Migration application and compatibility | Yes, isolated |
| External adapter tests | SMTP, Stripe, Cloud Storage, and external identity providers using fakes or sandboxes | No application database |
| Rust Core tests | Shared domain logic, validation, and algorithms | No |
| Bridge tests | Rust interoperability with Swift, Kotlin, and WebAssembly | No |
| Client tests | Presentation state, navigation, and platform adapters | No |
| OpenAPI contract tests | Backend and client contract compatibility | No |
| Release smoke tests | Health, startup, and revision connectivity | Technical reads only |
| Backup and restore tests | Operational recovery validation | Yes, isolated |

EF Core InMemory is not used as a PostgreSQL substitute. Database behavior is verified only by the isolated persistence test component; all consumers use injected test doubles.

Pull requests must pass every applicable automated category before merge. GitHub Actions validates Linux-compatible backend, Web, Rust Core, OpenAPI, and Terraform changes. Platform-specific mobile builds and bridge checks run locally before a mobile release.

Production deployments create a Cloud Run revision with zero traffic. Smoke tests use isolated technical data and never modify customer data. After validation, traffic moves gradually while errors, latency, and saturation are monitored. A failed rollout returns traffic to the previous revision.

Schema compatibility supports application rollback. Incomplete functionality may be deployed behind disabled feature flags.

## 20. Application and Software Supply Chain Security

All external traffic uses HTTPS, and Production enforces HSTS. CORS allows only explicitly configured origins. Web responses apply a Content Security Policy and appropriate security headers.

Internal endpoints are not public and require service identity authentication.

Production containers use minimal base images, run as a non-root user, and use a read-only filesystem where supported. CI builds each image once, records its immutable digest, and promotes the same artifact without rebuilding it.

Each release produces a software bill of materials and build provenance. Application dependencies and CI workflow dependencies are version-pinned.

CI runs vulnerability checks for .NET, Rust, JavaScript dependencies, container images, and committed secrets using free tooling. High or critical findings block deployment unless the project owner explicitly approves and documents a time-limited exception.

Secrets never enter source code, container images, build artifacts, telemetry, or Terraform state.

Security patches take priority over functional releases. Administrative access, IAM changes, and security-policy changes are audited.

Managed artifact scanning may be enabled later but is not required initially.

## 21. Google Cloud Infrastructure

**Initial resources:**
1. Cloud Run service for the ASP.NET Core API.
2. Cloud Run service for the Web frontend, if Cloud Run is the chosen hosting platform.
3. Cloud SQL PostgreSQL instance and application database.
4. Private Cloud Storage buckets for application files.
5. Artifact Registry Docker repository.
6. Cloud Tasks queues, Cloud Scheduler jobs, and required Cloud Run Jobs.
7. Cloud Logging, Monitoring, Trace, Error Reporting, and alerts.
8. Cloud Storage bucket for Terraform remote state (bootstrap separately).
9. Appropriate network connectivity between Cloud Run and Cloud SQL, with restricted database access.

**Production considerations:** Custom domains, ingress restrictions, edge protection when needed, database availability requirements, quotas, budgets, and cost alerts.

Cloud SQL is typically a persistent cost even at low traffic. Estimate costs before deployment.

## 22. Environments

The system has two isolated environments:

- **Development:** The application and all supporting infrastructure run locally without containers, including PostgreSQL, file storage, background jobs, and auxiliary services.
- **Production:** The application and infrastructure run entirely on Google Cloud using managed services, including Cloud Run, Cloud SQL, Cloud Storage, and Secret Manager.

Development and Production never share data, infrastructure, credentials, or configuration. There is no staging environment.

## 23. Configuration, Secrets, and Access

Non-sensitive settings are stored in version-controlled .NET configuration files.

Development and Production secrets are isolated in separate Google Cloud projects. The Development project contains only secrets and no runtime infrastructure.

An idempotent bootstrap retrieves Development secrets during the initial workstation setup or an explicit refresh. Secrets are stored in the operating system credential vault and exposed to the application through the .NET configuration system. They are never committed or stored in plaintext configuration files.

Developers may access source code and Development secrets only. The project owner is the only human administrator of Production.

Production secrets remain in Secret Manager. Runtime service accounts receive access only to the specific secrets required by their workloads. No human, application, or service receives broader access than necessary.

GitHub Actions uses temporary credentials, references secrets without reading their values, and requires the project owner's approval for Production deployment.

Long-lived service account keys are not used.

## 24. Terraform

Terraform provisions cloud resources; CI/CD builds and releases application artifacts. Avoid using Terraform as the primary mechanism for every application image rollout unless that workflow is deliberately chosen.

Suggested layout:

```text
infrastructure/
  modules/
    cloud-run/
    cloud-sql/
    cloud-storage/
    cloud-tasks/
    cloud-scheduler/
    artifact-registry/
    iam/
    secret-manager/
    monitoring/
  environments/
    production/
  bootstrap/
    terraform-state/
```

Terraform manages Production infrastructure only. Use reviewed plans, provider version pinning, restricted service identities, and protected remote state. Secret values remain outside Terraform state.

## 25. CI/CD

Source code is hosted in a private GitHub repository. The default branch is protected: changes require a pull request and successful required checks before merge.

**Pull requests:**
- Run every applicable Linux-compatible category defined in the testing strategy.
- Run formatting, linting, static analysis, and relevant target build checks.
- Validate OpenAPI compatibility and Terraform formatting/validation/plans.
- Use path filters so unrelated components do not trigger unnecessary workflows.

**Backend and Web deployments:**
1. Authenticate GitHub Actions to Google Cloud through Workload Identity Federation and short-lived credentials.
2. Build the ASP.NET Docker image and publish to Artifact Registry.
3. Deploy the API and background workloads to Cloud Run with controlled rollout.
4. Run database migrations through the dedicated migration identity.
5. Build and deploy the Web container independently.

GitHub Actions uses Linux runners only. Spending is disabled beyond the included allowance so exhausted usage stops workflows instead of generating charges.

iOS and Android builds, platform-specific tests, signing, and store submissions run locally on the project owner's Mac. Mobile signing credentials never enter GitHub. The project owner verifies the relevant shared-core release and native language bridge before manually publishing each application.

## 26. Repository Strategy and Organization

The system uses a private monorepo. Backend, clients, shared code, infrastructure, automation, and documentation remain isolated by top-level directory.

```text
workspace/
  shared-core/             # Rust crate(s), UniFFI, wasm-bindgen bridges
  ios/                     # SwiftUI app and Apple adapters
  android/                 # Kotlin/Compose app and Android adapters
  web/                     # React/TypeScript app and browser adapters
  backend/                 # ASP.NET Core API, background workloads, and EF Core migrations
  infrastructure/          # Terraform modules and environments
  automation/              # Reusable build, validation, and release scripts
  .github/workflows/       # GitHub Actions validation and deployment workflows
  docs/                    # API, architecture, deployment decisions
```

The repository layout does not couple component release cycles. Changes remain scoped to the affected directories, and generated files, build outputs, local databases, credentials, and IDE state are excluded from source control.

Commits and pull requests should preserve component boundaries. Split repositories only when a concrete security boundary or independently operated team requires it.

## 27. Component Versioning and Releases

The backend, Web application, iOS application, Android application, and Rust Core use independent semantic versions and component-scoped Git tags.

The Rust Core is compiled directly from the monorepo and is not published to an external package registry initially. Every client release records the exact Rust Core version and supported API major version.

Each release records its component version, source commit, dependencies on other internal components, and deployable artifact identifier. Component-specific changelogs describe externally relevant changes.

Backend releases preserve the API compatibility window required by published mobile clients. Web releases target a compatible backend contract without requiring both components to share a version or deployment.

Mobile release versions and build numbers follow the respective store requirements. Their builds, signing, validation, and submission remain manual.

## 28. Key Architectural Decisions

- Native UIs remain independent; no shared UI framework is required.
- Rust is a client-side library, not the backend implementation.
- Swift/Kotlin/TypeScript adapters own platform-specific capabilities.
- Shared logic is reused through compiled libraries and explicit language bindings.
- Operational client workflows support offline capture and explicit synchronization; privileged mutations remain online-only.
- ASP.NET Core Identity and OpenIddict form the central identity authority.
- Web authentication uses protected cookies; mobile authentication uses Authorization Code with PKCE and revocable tokens.
- MFA is mandatory for privileged users and uses authenticator-generated time-based codes.
- ASP.NET Core enforces policy-based and resource-based authorization for every protected operation.
- Stripe Billing is isolated behind a provider abstraction; verified webhooks drive the local entitlement projection.
- Application files use local storage in Development and private Cloud Storage with authorized temporary access in Production.
- Background processing uses C#/.NET with a transactional outbox and idempotent Google Cloud task execution.
- Transactional email uses a provider-neutral interface with SMTP as the initial adapter in both environments.
- OpenTelemetry provides vendor-neutral instrumentation; technical telemetry remains separate from application audit data.
- EF Core migrations run as a dedicated workload; Production recovery protections become mandatory before the first customer is activated.
- OpenAPI defines a versioned external contract using explicit request and response DTOs.
- Tests use injected boundaries; only isolated persistence, migration, and recovery components access PostgreSQL.
- Production revisions receive no traffic until smoke tests pass, then use gradual rollout with rollback.
- Containers and dependencies pass supply-chain security gates before release.
- Development is fully local; Production runs entirely on Google Cloud.
- Configuration, secrets, identities, and data remain isolated between environments.
- GitHub hosts the private source repository and GitHub Actions validates and deploys the backend and Web through Linux runners.
- Mobile applications are built, signed, tested, and published manually from the project owner's Mac.
- A private monorepo keeps components isolated by directory while allowing coordinated contract and shared-core changes.
- Components use independent versions and releases while recording their exact internal compatibility relationships.
- Prefer simple deployment topology first; add operational complexity only when evidence justifies it.
