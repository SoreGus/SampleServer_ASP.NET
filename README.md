# Sample Server

Sample Server is a cross-platform HTTP API built with .NET 10 and ASP.NET Core Minimal APIs. It follows the project's Scope-Based Component Architecture, which separates executable startup, HTTP endpoints, application context, and reusable core components by reuse scope.

The application runs on macOS, Linux, and Windows.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Task](https://taskfile.dev/docs/installation) 3.x
- Git

Verify the required tools:

```bash
dotnet --version
task --version
git --version
```

## Getting started

Restore the project dependencies:

```bash
task restore
```

Build the project:

```bash
task build
```

Run the server:

```bash
task run
```

For development with automatic reload:

```bash
task watch
```

The development server listens at:

```text
http://localhost:5000
```

## Health check

After starting the server, open:

```text
http://localhost:5000/health
```

A healthy server responds with HTTP `200 OK` and:

```text
Healthy
```

## Available tasks

List all available tasks:

```bash
task --list
```

| Command | Description |
|---|---|
| `task` | Restore and build the project |
| `task restore` | Restore project dependencies |
| `task build` | Restore and build the project |
| `task run` | Run the server |
| `task watch` | Run with automatic reload |
| `task clean` | Clean build outputs |
| `task rebuild` | Clean and rebuild the project |
| `task format` | Format the C# source code |
| `task format-check` | Verify C# formatting without modifying files |
| `task check` | Build and verify formatting |
| `task publish` | Publish a Release build to `artifacts/publish` |

The underlying .NET CLI commands can also be used directly when Task is unavailable.

## Project structure

```text
.
├── Configuration/     Application settings by environment
├── Properties/        Local .NET launch settings
├── docs/              Architecture and reference documentation
├── src/
│   ├── Endpoints/     HTTP interface components
│   └── Server/        Executable entry point and composition root
├── SampleServer.csproj
└── Taskfile.yml
```

Additional scopes are introduced when the application needs them:

```text
Core       Components reusable across different applications
Context    Components shared within this application
Endpoints  HTTP-specific interface components
Server     Executable startup and final composition
```

See [Scope-Based Component Architecture for .NET](docs/dotnet-scope-based-component-architecture.md) for the complete architectural conventions.

## Configuration

Configuration files are stored under `Configuration`:

```text
Configuration/
├── appsettings.json
└── appsettings.Development.json
```

Do not commit passwords, connection strings containing credentials, tokens, certificates, or other secrets. Use .NET User Secrets for local development and environment variables or a secret provider in deployed environments.

## Cross-platform support

The application and its task commands use the cross-platform .NET CLI. Task provides the same development commands on macOS, Linux, and Windows. Taskfile commands should avoid operating-system-specific shell syntax so the workflow remains portable.

