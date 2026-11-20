# Scope-Based Component Architecture for .NET

## 1. Overview

Scope-Based Component Architecture is a project organization pattern for .NET applications. It places each component according to the scope in which that component can be reused.

The architecture supports two application shapes:

- graphical applications, using `Features` and an `App` entry point;
- HTTP servers, using `Endpoints` and a `Server` entry point.

Both shapes share the same `Core` and `Context` concepts.

```text
Cross-application scope   Core
Application scope         Context
Interface scope           Features or Endpoints
Executable scope          App or Server
```

This is a custom architectural convention. It borrows useful ideas from layered architecture, dependency inversion, MVVM, vertical organization, and dependency injection, but it does not claim to be an official .NET framework pattern.

## 2. Fundamental principle

Every component is placed according to its widest realistic reuse scope:

```text
Core
└── Reusable by different applications.

Context
└── Specific to one application, but shareable by its interfaces and components.

Features or Endpoints
└── Specific to a user-facing feature or HTTP interface.

App or Server
└── Executable entry point and composition root.
```

A component should begin in the narrowest scope that accurately represents its responsibility. It can be promoted to a wider scope when reuse becomes real.

The architecture deliberately keeps application models, services, repositories, and their interfaces in `Context`, even when they are initially consumed by only one feature or endpoint. This prevents application capabilities from being owned by a delivery mechanism.

## 3. Dependency direction

Dependencies always point toward wider reuse scopes.

For a graphical application:

```text
App
 ↓
Features
 ↓
Context
 ↓
Core
```

For a server:

```text
Server
 ↓
Endpoints
 ↓
Context
 ↓
Core
```

The following dependencies are not allowed:

```text
Core     ✗→ Context
Core     ✗→ Features
Core     ✗→ Endpoints
Context  ✗→ Features
Context  ✗→ Endpoints
Features ✗→ App
Endpoints ✗→ Server
```

`App` and `Server` are composition roots. They are allowed to reference all layers needed to assemble the executable.

## 4. Top-level project layout

Project metadata remains outside `src`. Configuration files are grouped instead of being placed directly in the project root.

```text
Project/
├── .gitignore
├── README.md
├── Project.csproj
├── Taskfile.yml
├── Configuration/
├── Properties/
├── Resources/             # Graphical applications only
├── src/
└── tests/
```

A solution file is optional. When used, it may remain in the project root or in a workspace-level directory.

### 4.1 Source inclusion

Because the main `.csproj` is above both `src` and `tests`, the project should compile only source files under `src`. Otherwise, SDK default globbing can accidentally include test source files in the main executable.

One explicit approach is:

```xml
<PropertyGroup>
  <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
</PropertyGroup>

<ItemGroup>
  <Compile Include="src/**/*.cs" />
</ItemGroup>
```

Graphical frameworks can require additional project items for XAML, resources, assets, or platform files. Those items are framework-specific and should be added without changing the architectural scopes described here.

### 4.2 Task runner

The repository uses [Task](https://taskfile.dev/) as its cross-platform task runner. `Taskfile.yml` remains in the project root because it orchestrates the entire repository rather than belonging to an application scope under `src`.

Common commands are:

```text
task restore
task build
task run
task watch
task clean
task rebuild
task format
task format-check
task check
task publish
```

Task runs on macOS, Linux, and Windows. The `task` executable must be installed on the development machine. Commands inside the Taskfile should use cross-platform .NET CLI operations whenever possible.

## 5. Core

`Core` contains components that can be reused by different applications.

Application independence does not always mean technology independence:

- `Core/Domain` is application-independent and technology-neutral;
- `Core/Infrastructure` contains application-independent implementations and may depend on a specific reusable technology.

For example, a generic Redis cache implementation may belong to `Core/Infrastructure` if it contains no knowledge of the current application.

### 5.1 Core structure

```text
Core/
├── Domain/
│   ├── Models/
│   │   ├── Entities/
│   │   │   └── Entity.cs
│   │   └── Results/
│   │       ├── Result.cs
│   │       └── ResultOfT.cs
│   ├── Errors/
│   │   └── General/
│   │       └── DomainError.cs
│   ├── Interfaces/
│   │   ├── Services/
│   │   │   └── Clock/
│   │   │       └── IClockService.cs
│   │   └── Repositories/
│   └── Utils/
│       └── Strings/
│           └── StringNormalizer.cs
└── Infrastructure/
    ├── Services/
    │   └── Clock/
    │       └── ClockService.cs
    ├── Repositories/
    └── Containers/
        └── CoreContainer.cs
```

Directories should be created only when they contain real components. An empty `Repositories` directory is not required merely to preserve symmetry.

### 5.2 Core rules

- It must not contain product-specific or application-specific terminology.
- It must not reference `Context`, `Features`, `Endpoints`, `App`, or `Server`.
- Generic models such as `Result<T>` belong under `Domain/Models`.
- Interfaces belong to the scope that owns the abstraction.
- Technology-neutral logic belongs in `Domain`.
- Reusable technological implementations belong in `Infrastructure`.

## 6. Context

`Context` contains everything that is specific to the current application but can be shared by different features, endpoints, jobs, commands, or other entry mechanisms.

It owns application models, application errors, service interfaces and implementations, repository interfaces and implementations, persistence, shared routing abstractions for graphical applications, and application-level dependency registrations.

The name `Context` means application context. It must not be confused with Entity Framework's `DbContext`.

### 6.1 Context structure

```text
Context/
├── Models/
│   ├── Products/
│   │   └── Product.cs
│   └── Users/
│       └── User.cs
├── Errors/
│   ├── Products/
│   │   └── ProductError.cs
│   └── Users/
│       └── UserError.cs
├── Services/
│   ├── Interfaces/
│   │   ├── Products/
│   │   │   └── IProductService.cs
│   │   └── Users/
│   │       └── IUserService.cs
│   ├── Products/
│   │   └── ProductService.cs
│   └── Users/
│       └── UserService.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── Products/
│   │   │   └── IProductRepository.cs
│   │   └── Users/
│   │       └── IUserRepository.cs
│   ├── Products/
│   │   └── ProductRepository.cs
│   └── Users/
│       └── UserRepository.cs
├── Persistence/
│   ├── AppDbContext.cs
│   └── Configurations/
│       ├── Products/
│       │   └── ProductConfiguration.cs
│       └── Users/
│           └── UserConfiguration.cs
└── Containers/
    └── ContextContainer.cs
```

Graphical applications may additionally define shared routing capabilities:

```text
Context/
└── Routers/
    ├── Interfaces/
    │   └── Application/
    │       └── IApplicationRouter.cs
    └── Application/
        └── ApplicationRouter.cs
```

### 6.2 Context rules

- Application models are never owned by a feature or endpoint.
- Application services and repositories remain in `Context`, even when currently used by one interface component.
- Service logic must not depend on HTTP, UI controls, pages, windows, or visual state.
- Repository implementations may depend on persistence technologies such as Entity Framework Core and PostgreSQL.
- `Context` may depend on `Core`.
- `Context` must not depend on `Features`, `Endpoints`, `App`, or `Server`.

## 7. Component-based subdirectories

The first directory level describes the architectural type. The next level describes the component or concept.

```text
Context/Models/Products/Product.cs
        │      └── Component
        └── Architectural type
```

Examples:

```text
Core/Domain/Models/Results/Result.cs
Core/Domain/Interfaces/Services/Clock/IClockService.cs
Context/Services/Products/ProductService.cs
Context/Repositories/Products/ProductRepository.cs
```

Additional subdivisions are introduced only when they improve navigation. Avoid directories that permanently contain a single file without representing a meaningful concept.

## 8. Graphical .NET application pattern

A graphical application uses `Features` as its interface scope and `App` as its executable scope.

This pattern is UI-framework independent. It can be adapted to .NET MAUI, Avalonia, WPF, WinUI, or another .NET graphical framework. XAML files shown below are illustrative and may be replaced with C# views.

### 8.1 Complete graphical application hierarchy

```text
SampleApp/
├── .gitignore
├── README.md
├── SampleApp.csproj
├── Taskfile.yml
├── Configuration/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── appsettings.Production.json
├── Resources/
│   ├── Assets/
│   │   ├── Images/
│   │   ├── Icons/
│   │   └── Fonts/
│   ├── Styles/
│   └── Localization/
├── src/
│   ├── Core/
│   │   ├── Domain/
│   │   │   ├── Models/
│   │   │   │   ├── Entities/
│   │   │   │   └── Results/
│   │   │   ├── Errors/
│   │   │   ├── Interfaces/
│   │   │   └── Utils/
│   │   └── Infrastructure/
│   │       ├── Services/
│   │       ├── Repositories/
│   │       └── Containers/
│   ├── Context/
│   │   ├── Models/
│   │   │   ├── Products/
│   │   │   └── Users/
│   │   ├── Errors/
│   │   │   ├── Products/
│   │   │   └── Users/
│   │   ├── Services/
│   │   │   ├── Interfaces/
│   │   │   ├── Products/
│   │   │   └── Users/
│   │   ├── Repositories/
│   │   │   ├── Interfaces/
│   │   │   ├── Products/
│   │   │   └── Users/
│   │   ├── Routers/
│   │   │   ├── Interfaces/
│   │   │   └── Application/
│   │   └── Containers/
│   ├── Features/
│   │   ├── Products/
│   │   │   ├── Views/
│   │   │   │   ├── ProductListView.xaml
│   │   │   │   ├── ProductListView.xaml.cs
│   │   │   │   ├── ProductDetailsView.xaml
│   │   │   │   └── ProductDetailsView.xaml.cs
│   │   │   ├── ViewModels/
│   │   │   │   ├── ProductListViewModel.cs
│   │   │   │   └── ProductDetailsViewModel.cs
│   │   │   ├── Interactors/
│   │   │   │   ├── LoadProductsInteractor.cs
│   │   │   │   └── LoadProductDetailsInteractor.cs
│   │   │   ├── Routers/
│   │   │   │   └── ProductRouter.cs
│   │   │   └── ProductFeatureContainer.cs
│   │   └── Authentication/
│   │       ├── Views/
│   │       ├── ViewModels/
│   │       ├── Interactors/
│   │       ├── Routers/
│   │       └── AuthenticationFeatureContainer.cs
│   └── App/
│       ├── Program.cs
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── AppContainer.cs
│       └── AppConfiguration.cs
└── tests/
    ├── SampleApp.UnitTests/
    │   ├── SampleApp.UnitTests.csproj
    │   ├── Context/
    │   └── Features/
    └── SampleApp.UITests/
        ├── SampleApp.UITests.csproj
        └── Features/
```

### 8.2 Graphical responsibilities

```text
View
 ↓
ViewModel
 ↓
Interactor
 ↓
Context Service
 ↓
Context Repository
```

#### View

A view renders state and forwards user interactions. It should not contain application rules or persistence logic.

#### ViewModel

`ViewModel` is an established .NET MVVM concept. It exposes observable state and commands in a form suitable for binding by a view.

#### Interactor

`Interactor` is an architectural convention rather than a .NET framework requirement. It coordinates one UI-specific user interaction using one or more `Context` services.

An interactor is useful when a view model would otherwise accumulate orchestration logic. A trivial view model may call a context service directly when no meaningful orchestration exists.

#### Router

A feature router performs feature-specific navigation. Shared application navigation abstractions and implementations belong to `Context/Routers`.

#### App

`App` owns executable startup and composition. Exact startup files vary by graphical framework, but the responsibility remains the same.

## 9. ASP.NET Core server pattern

A server uses `Endpoints` as its interface scope and `Server` as its executable scope.

The recommended HTTP implementation is ASP.NET Core Minimal APIs. Controllers can still be used when required, without changing the wider scope rules.

### 9.1 Complete server hierarchy

```text
SampleServer/
├── .gitignore
├── README.md
├── SampleServer.csproj
├── Taskfile.yml
├── Configuration/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── appsettings.Production.json
├── Properties/
│   └── launchSettings.json
├── src/
│   ├── Core/
│   │   ├── Domain/
│   │   │   ├── Models/
│   │   │   │   ├── Entities/
│   │   │   │   └── Results/
│   │   │   ├── Errors/
│   │   │   ├── Interfaces/
│   │   │   └── Utils/
│   │   └── Infrastructure/
│   │       ├── Services/
│   │       ├── Repositories/
│   │       └── Containers/
│   ├── Context/
│   │   ├── Models/
│   │   │   ├── Products/
│   │   │   └── Users/
│   │   ├── Errors/
│   │   │   ├── Products/
│   │   │   └── Users/
│   │   ├── Services/
│   │   │   ├── Interfaces/
│   │   │   │   ├── Products/
│   │   │   │   └── Users/
│   │   │   ├── Products/
│   │   │   └── Users/
│   │   ├── Repositories/
│   │   │   ├── Interfaces/
│   │   │   │   ├── Products/
│   │   │   │   └── Users/
│   │   │   ├── Products/
│   │   │   └── Users/
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   └── Configurations/
│   │   │       ├── Products/
│   │   │       └── Users/
│   │   └── Containers/
│   │       └── ContextContainer.cs
│   ├── Endpoints/
│   │   ├── Products/
│   │   │   ├── Requests/
│   │   │   │   ├── Creation/
│   │   │   │   │   └── CreateProductRequest.cs
│   │   │   │   └── Updating/
│   │   │   │       └── UpdateProductRequest.cs
│   │   │   ├── Responses/
│   │   │   │   ├── Details/
│   │   │   │   │   └── ProductResponse.cs
│   │   │   │   └── Listing/
│   │   │   │       └── ProductListResponse.cs
│   │   │   ├── Operations/
│   │   │   │   ├── Creation/
│   │   │   │   │   └── CreateProductOperation.cs
│   │   │   │   ├── Reading/
│   │   │   │   │   ├── GetProductOperation.cs
│   │   │   │   │   └── ListProductsOperation.cs
│   │   │   │   ├── Updating/
│   │   │   │   │   └── UpdateProductOperation.cs
│   │   │   │   └── Removal/
│   │   │   │       └── DeleteProductOperation.cs
│   │   │   ├── ProductEndpoints.cs
│   │   │   └── ProductEndpointRegistration.cs
│   │   └── Users/
│   │       ├── Requests/
│   │       ├── Responses/
│   │       ├── Operations/
│   │       ├── UserEndpoints.cs
│   │       └── UserEndpointRegistration.cs
│   └── Server/
│       └── Program.cs
└── tests/
    ├── SampleServer.UnitTests/
    │   ├── SampleServer.UnitTests.csproj
    │   ├── Core/
    │   ├── Context/
    │   └── Endpoints/
    └── SampleServer.IntegrationTests/
        ├── SampleServer.IntegrationTests.csproj
        └── Endpoints/
```

### 9.2 Server responsibilities

```text
HTTP request
 ↓
Endpoint
 ↓
Operation
 ↓
Context Service
 ↓
Context Repository
 ↓
Database or external system
```

The return flow is:

```text
Context Model or Result
 ↓
Operation
 ↓
Endpoint Response
 ↓
HTTP response
```

#### Request

A request is an HTTP input contract. It belongs to an endpoint because it represents the external protocol, not the internal application model.

#### Response

A response is an HTTP output contract. Internal `Context` models should not be serialized directly unless that coupling is explicitly intended.

#### Operation

An operation coordinates one endpoint action using one or more `Context` services. It is the server counterpart to a graphical feature interactor.

`Operation` is preferred over `Handler` in this architecture because `Handler` is overloaded in ASP.NET Core, routing, exception handling, and CQRS libraries.

Operations are optional for trivial routes. An endpoint may call a context service directly when it performs no meaningful composition. A project may nevertheless choose to use operations consistently to guarantee thin endpoints.

#### Endpoint

An endpoint handles only protocol responsibilities:

- route and method declaration;
- parameter and body binding;
- authentication and authorization metadata;
- cancellation propagation;
- mapping application outcomes to HTTP status codes and response contracts.

Endpoints must not contain persistence logic or application rules.

#### Server

`Server` contains the executable entry point, shared HTTP pipeline configuration, and the final dependency composition.

For a small server, these responsibilities remain directly in `Program.cs`. Extension classes are introduced only when concrete configuration, dependency registration, or middleware composition becomes large enough to justify extraction.

### 9.3 Simple operational health endpoint

Even an operational endpoint belongs to the `Endpoints` interface scope. A simple health endpoint uses ASP.NET Core's built-in health check APIs without requests, responses, operations, services, or repositories.

```text
src/
├── Endpoints/
│   └── Health/
│       └── HealthEndpoints.cs
└── Server/
    └── Program.cs
```

`Program.cs` registers the framework service with `AddHealthChecks`. `HealthEndpoints.cs` owns the HTTP mapping through a focused `MapHealthEndpoints` extension method that calls `MapHealthChecks`.

## 10. Dependency injection

All dependencies are composed through the built-in .NET dependency injection container unless a framework requires a different adapter.

Interfaces are not required merely to use dependency injection. Concrete view models, interactors, and operations can be registered and injected directly.

Interfaces are appropriate when they define an architectural boundary, support multiple implementations, isolate an external dependency, or provide meaningful substitution in tests.

Example server registrations:

```csharp
public static class ContextContainer
{
    public static IServiceCollection AddContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
```

```csharp
public static class ProductEndpointRegistration
{
    public static IServiceCollection AddProductEndpoints(
        this IServiceCollection services)
    {
        services.AddScoped<CreateProductOperation>();
        services.AddScoped<UpdateProductOperation>();
        return services;
    }
}
```

`Containers` in this architecture contain registration extensions. They do not replace the built-in dependency injection container. The final composition remains in `App` or `Server`.

## 11. Configuration

Application configuration is stored under `Configuration`:

```text
Configuration/
├── appsettings.json
├── appsettings.Development.json
└── appsettings.Production.json
```

Secrets must not be committed to these files. Use environment variables, `dotnet user-secrets`, or a dedicated secret provider.

ASP.NET Core looks for `appsettings.json` in the project content root by default. When configuration files are moved into `Configuration`, the server must explicitly add them while preserving provider precedence.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(
        "Configuration/appsettings.json",
        optional: false,
        reloadOnChange: true)
    .AddJsonFile(
        $"Configuration/appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true);

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>(optional: true);

builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args);
```

The order ensures the following precedence, from lowest to highest:

```text
Base JSON
Environment JSON
User secrets in development
Environment variables
Command-line arguments
```

The project file should preserve configuration files during build and publication:

```xml
<ItemGroup>
  <Content Update="Configuration/appsettings*.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </Content>
</ItemGroup>
```

`Properties/launchSettings.json` remains in the conventional tooling location.

## 12. Entry points and composition roots

### 12.1 Graphical application

The exact entry point varies by UI framework, but it belongs under `src/App`.

```text
src/App/
├── Program.cs
├── App.xaml
├── App.xaml.cs
├── AppContainer.cs
└── AppConfiguration.cs
```

### 12.2 Server

The ASP.NET Core entry point belongs under `src/Server`.

```text
src/Server/
└── Program.cs
```

Example:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCore()
    .AddContext(builder.Configuration)
    .AddProductEndpoints();

var app = builder.Build();

var api = app.MapGroup("/api");
api.MapProductEndpoints();

app.Run();
```

## 13. Naming conventions

- Use PascalCase for directories, namespaces, types, and public members.
- Prefix interfaces with `I` in conventional C# style.
- Suffix asynchronous methods with `Async`.
- Place `CancellationToken` last in asynchronous method parameters.
- Prefer one primary public type per file.
- Keep namespaces aligned with the directory hierarchy.
- Use `Container` for dependency registration groups in this architecture.
- Use `Interactor` for graphical feature orchestration.
- Use `Operation` for endpoint orchestration.
- Use `Request` and `Response` for HTTP contracts.
- Use `ViewModel` only for graphical presentation state.
- Do not use `ViewModel` as a synonym for an HTTP response.

## 14. Placement guide

| Component | Location |
|---|---|
| Generic result model | `Core/Domain/Models/Results` |
| Cross-application interface | `Core/Domain/Interfaces` |
| Reusable interface implementation | `Core/Infrastructure` |
| Application model | `Context/Models/<Component>` |
| Application error | `Context/Errors/<Component>` |
| Application service interface | `Context/Services/Interfaces/<Component>` |
| Application service implementation | `Context/Services/<Component>` |
| Application repository interface | `Context/Repositories/Interfaces/<Component>` |
| Application repository implementation | `Context/Repositories/<Component>` |
| Graphical view | `Features/<Feature>/Views` |
| Graphical presentation state | `Features/<Feature>/ViewModels` |
| Graphical interaction orchestration | `Features/<Feature>/Interactors` |
| Feature navigation | `Features/<Feature>/Routers` |
| HTTP input contract | `Endpoints/<Endpoint>/Requests` |
| HTTP output contract | `Endpoints/<Endpoint>/Responses` |
| HTTP action orchestration | `Endpoints/<Endpoint>/Operations` |
| Minimal API route declarations | `Endpoints/<Endpoint>/<Name>Endpoints.cs` |
| Graphical executable startup | `App` |
| Server executable startup | `Server` |

## 15. Evolution rules

1. Create directories only when real components require them.
2. Keep application capabilities in `Context`, not in a delivery interface.
3. Keep HTTP concerns in `Endpoints` and visual concerns in `Features`.
4. Promote components to `Core` only when cross-application reuse is realistic.
5. Do not create interfaces solely to mirror every concrete class.
6. Do not create interactors or operations that add no orchestration unless uniformity is an explicit project goal.
7. Split a component into additional subdirectories only when the subdivision improves navigation.
8. Keep entry points small; move registrations and pipeline configuration to focused container and configuration extensions.
9. Protect dependency direction through project references when the codebase grows large enough to justify multiple `.csproj` files.
10. Prefer clarity and consistent placement over adherence to a textbook architecture.

## 16. Summary

Scope-Based Component Architecture organizes .NET code by reuse scope first and component second.

```text
Graphical application
App → Features → Context → Core

HTTP server
Server → Endpoints → Context → Core
```

`Core` is reusable across applications. `Context` owns application capabilities. `Features` and `Endpoints` adapt those capabilities to a specific interface. `App` and `Server` assemble and start the executable.

The result is a predictable structure that supports both graphical .NET applications and ASP.NET Core servers without forcing UI or HTTP concepts into application services and models.
