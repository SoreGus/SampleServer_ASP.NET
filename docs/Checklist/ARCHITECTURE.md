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
| Container registry | Artifact Registry | Store deployable container images |
| Infrastructure as code | Terraform | Provision and configure Google Cloud resources |
| CI/CD | GitHub Actions | Tests, builds, image publishing, deployments |
| Secrets | Secret Manager | Store runtime credentials and secrets |
| Service identity | Cloud IAM | Least-privilege identities and resource access |
| User identity | OIDC / OAuth 2.0 | Authentication and token-based access |
| Observability | Cloud Logging / Monitoring, OpenTelemetry | Logs, metrics, traces, alerts |
| API contract | OpenAPI | Document and optionally generate typed clients |
| Database schema | EF Core Migrations | Versioned schema changes |
| Terraform state | Cloud Storage | Remote state with restricted access and versioning |

## 3. Logical Architecture

```text
                   +-----------------------+
                   |  Identity Provider    |
                   |    OIDC / OAuth 2.0   |
                   +-----------+-----------+
                               |
   +---------------------------+---------------------------+
   |                           |                           |
+--v----------------+ +--------v-----------+ +-------------v---------+
| iOS               | | Android            | | Web                   |
| SwiftUI / Swift   | | Compose / Kotlin   | | React / TypeScript    |
| Native adapters   | | Native adapters    | | Browser adapters      |
| Rust Core         | | Rust Core          | | Rust Core (Wasm)      |
+--+----------------+ +--------+-----------+ +-------------+---------+
   |                           |                           |
   +---------------------------+---------------------------+
                               | HTTPS / JSON
                     +---------v----------+
                     | ASP.NET Core API   |
                     | Docker / Cloud Run|
                     +---------+----------+
                               |
                     +---------v----------+
                     | Cloud SQL          |
                     | PostgreSQL         |
                     +--------------------+

Cross-cutting: Terraform, Artifact Registry, IAM, Secret Manager,
GitHub Actions, Cloud Logging/Monitoring, OpenTelemetry.
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
| Secure storage | Keychain | Android Keystore-backed solution | Browser-appropriate session/auth approach |
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

## 6. ASP.NET Core Backend

Start with a modular monolith, with clear domain, application, infrastructure, and API boundaries.

- REST/JSON over HTTPS, described by OpenAPI.
- Validate access tokens and enforce authorization on every protected operation.
- Use EF Core with Npgsql for PostgreSQL; manage schema changes with migrations.
- Use health endpoints, structured logging, request correlation, and OpenTelemetry instrumentation.
- Keep secrets out of source control and container images.
- Limit database connection pools to account for Cloud Run autoscaling and Cloud SQL connection limits.
- Add queues, caching, or independent services only when justified by requirements.

## 7. Google Cloud Infrastructure

**Initial resources:**
1. Cloud Run service for the ASP.NET Core API.
2. Cloud Run service for the Web frontend, if Cloud Run is the chosen hosting platform.
3. Cloud SQL PostgreSQL instance and application database.
4. Artifact Registry Docker repository.
5. Dedicated service accounts and minimum IAM permissions.
6. Secret Manager secrets and controlled runtime access.
7. Cloud Logging, Cloud Monitoring, and basic alerts.
8. Cloud Storage bucket for Terraform remote state (bootstrap separately).
9. Appropriate network connectivity between Cloud Run and Cloud SQL, with restricted database access.

**Production considerations:** TLS/custom domains, ingress restrictions, CORS, rate limiting/edge protection when needed, Cloud SQL backups and restore tests, database availability requirements, quotas, budgets, and cost alerts.

Cloud SQL is typically a persistent cost even at low traffic. Estimate costs before deployment.

## 8. Terraform

Terraform provisions cloud resources; CI/CD builds and releases application artifacts. Avoid using Terraform as the primary mechanism for every application image rollout unless that workflow is deliberately chosen.

Suggested layout:

```text
infrastructure/
  modules/
    cloud-run/
    cloud-sql/
    artifact-registry/
    iam/
    secret-manager/
    monitoring/
  environments/
    dev/
    prod/
  bootstrap/
    terraform-state/
```

Use separate environment variables and state, restricted service identities, reviewed plans, provider version pinning, and remote state protection. Never commit Terraform state or plaintext secrets. Treat secret values separately from infrastructure metadata to avoid exposing them in state.

## 9. CI/CD

**Pull requests:**
- Run Rust unit tests, formatting/linting, and relevant target compilation checks.
- Run ASP.NET tests and static analysis.
- Run Swift, Kotlin, and React tests/build checks where runners and toolchains permit.
- Validate OpenAPI compatibility and Terraform formatting/validation/plans.

**Deployments:**
1. Authenticate CI to Google Cloud via Workload Identity Federation (avoid long-lived JSON keys).
2. Build the ASP.NET Docker image and publish to Artifact Registry.
3. Deploy the new image to Cloud Run with controlled rollout.
4. Apply database migrations through an explicitly controlled release step.
5. Build and deploy the Web container independently.
6. Publish mobile apps through their respective store pipelines.

Use versioned shared-core releases and test all three language bridges before promoting changes.

## 10. Suggested Repository Organization

```text
workspace/
  shared-core/             # Rust crate(s), UniFFI, wasm-bindgen bridges
  ios/                     # SwiftUI app and Apple adapters
  android/                 # Kotlin/Compose app and Android adapters
  web/                     # React/TypeScript app and browser adapters
  backend/                 # ASP.NET Core API and EF Core migrations
  infrastructure/          # Terraform modules and environments
  .github/workflows/       # CI/CD pipelines
  docs/                    # API, architecture, deployment decisions
```

A monorepo is optional; this layout illustrates ownership boundaries rather than requiring a specific repository strategy.

## 11. Key Architectural Decisions

- Native UIs remain independent; no shared UI framework is required.
- Rust is a client-side library, not the backend implementation.
- Swift/Kotlin/TypeScript adapters own platform-specific capabilities.
- Shared logic is reused through compiled libraries and explicit language bindings.
- ASP.NET Core remains the trusted source of server-side authorization and persistence rules.
- Terraform manages infrastructure; GitHub Actions manages delivery.
- Prefer simple deployment topology first; add operational complexity only when evidence justifies it.
