# Minimal APIs com ASP.NET Core 10 - guia de referência

Padrão de backend moderno usando apenas o ecossistema oficial da Microsoft, exemplificado com um CRUD da entidade `Feature`.

Fluxo de uma requisição:

```
HTTP → FeatureEndpoints → IFeatureService → IFeatureRepository → AppDbContext → Postgres
```

---

## 1. Dependências

| Etapa | Framework / biblioteca | Origem |
|---|---|---|
| Runtime e linguagem | .NET 10 (LTS) + C# 14 | Microsoft |
| Servidor HTTP | Kestrel (incluso no ASP.NET Core) | Microsoft |
| Rotas e endpoints | ASP.NET Core Minimal APIs | Microsoft |
| Validação | `Microsoft.Extensions.Validation` + `System.ComponentModel.DataAnnotations` | Microsoft |
| Erros HTTP | ProblemDetails (`AddProblemDetails`, `UseExceptionHandler`) | Microsoft |
| Documentação da API | `Microsoft.AspNetCore.OpenApi` | Microsoft |
| Injeção de dependência | `Microsoft.Extensions.DependencyInjection` | Microsoft |
| Configuração | `Microsoft.Extensions.Configuration` | Microsoft |
| ORM | Entity Framework Core 10 | Microsoft |
| Driver Postgres | `Npgsql.EntityFrameworkCore.PostgreSQL` | Comunidade |
| Migrations | EF Core Migrations + `dotnet-ef` | Microsoft |
| Tempo | `TimeProvider` (BCL) | Microsoft |
| Serialização JSON | `System.Text.Json` | Microsoft |

Pacotes NuGet a instalar:

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef
```

---

## 2. Arquitetura

| Nome | Aplicação |
|---|---|
| Vertical Slice Architecture | Tudo de uma funcionalidade fica numa pasta (`Features/`), organizado por funcionalidade e não por camada técnica |
| Layered Architecture | Dentro da fatia: Endpoint → Service → Repository |
| REST | Recursos (`/features`), verbos HTTP e status codes semânticos |

### Responsabilidade de cada camada

| Camada | Conhece HTTP? | Conhece banco? | Papel |
|---|---|---|---|
| `FeatureEndpoints` | Sim | Não | Traduz HTTP em chamadas ao service e o resultado em status code |
| `FeatureService` | Não | Não | Aplica a regra de negócio |
| `FeatureRepository` | Não | Sim | Persiste dados com EF Core |
| `Feature` | Não | Não | Guarda o estado e as regras da própria entidade |

---

## 3. Técnicas e padrões

| Técnica | Onde aparece |
|---|---|
| Dependency Injection / Inversion of Control | Dependências recebidas pelo construtor e montadas pelo container |
| Dependency Inversion Principle (SOLID - D) | `FeatureService` depende de `IFeatureRepository`, não da implementação concreta |
| Single Responsibility Principle (SOLID - S) | Cada classe tem um único papel |
| Interface Segregation Principle (SOLID - I) | Interfaces pequenas e específicas por feature |
| Repository Pattern | `IFeatureRepository` / `FeatureRepository` |
| Service Layer | `IFeatureService` / `FeatureService` |
| DTO (Data Transfer Object) | `CreateFeatureRequest`, `UpdateFeatureRequest`, `FeatureResponse` |
| Result Pattern | `FeatureResult` + `FeatureError`: erro esperado vira retorno, não exceção |
| Static Factory Method | `Feature.Create(...)` |
| Encapsulamento | Setters privados em `Feature` |
| Rich Domain Model (DDD) | Comportamento na entidade (`Update`) |
| Unit of Work | O `DbContext` persiste as mudanças numa transação via `SaveChangesAsync` |
| Primary Constructors | `FeatureRepository(AppDbContext db)` |
| Extension Methods | `MapFeatureEndpoints()` e `AddFeatureServices()` |
| Route Groups | `MapGroup("/features")` e prefixo `/api` |
| Typed Results | `Results<Ok<T>, NotFound>`: contrato checado pelo compilador e exposto no OpenAPI |
| Pattern Matching | `switch` sobre `FeatureResult` e `FeatureError` |
| Fluent API (EF Core) | `FeatureConfiguration` |
| Options / Configuration Pattern | Connection string via `IConfiguration` |
| Cooperative Cancellation | `CancellationToken` propagado até o banco |
| RFC 9457 Problem Details | Formato padrão de erro HTTP |
| UUID v7 | `Guid.CreateVersion7()`: ID ordenável no tempo, melhor para índices |

---

## 4. Nomenclatura

| Tipo | Nome | Forma |
|---|---|---|
| Entidade | `Feature` | `sealed class`, sem sufixo |
| Erros | `FeatureError` | `enum` |
| Resultado | `FeatureResult` | `sealed record` |
| Entrada HTTP | `CreateFeatureRequest`, `UpdateFeatureRequest` | `sealed record` |
| Saída HTTP | `FeatureResponse` | `sealed record` |
| Contrato | `IFeatureRepository`, `IFeatureService` | `interface`, prefixo `I` |
| Implementação | `FeatureRepository`, `FeatureService` | `sealed class` |
| Mapeamento EF | `FeatureConfiguration` | `IEntityTypeConfiguration<T>` |
| Rotas | `FeatureEndpoints` + `MapFeatureEndpoints()` | `static class` + extension method |
| Registro no DI | `FeatureServiceCollectionExtensions` + `AddFeatureServices()` | `static class` + extension method |

Convenções gerais: PascalCase para tipos e membros, `_camelCase` para campos privados, sufixo `Async` em métodos assíncronos com `CancellationToken ct` por último, namespace igual ao caminho da pasta, `sealed` por padrão. Use `Endpoints` como uma palavra só (não `EndPoints`).

---

## 5. Estrutura de pastas

```
Platform.Api/
├── Program.cs
├── appsettings.json
├── Data/
│   └── AppDbContext.cs
└── Features/
    ├── Feature.cs
    ├── FeatureError.cs
    ├── FeatureResult.cs
    ├── CreateFeatureRequest.cs
    ├── UpdateFeatureRequest.cs
    ├── FeatureResponse.cs
    ├── IFeatureRepository.cs
    ├── FeatureRepository.cs
    ├── FeatureConfiguration.cs
    ├── IFeatureService.cs
    ├── FeatureService.cs
    ├── FeatureEndpoints.cs
    └── FeatureServiceCollectionExtensions.cs
```

---

## 6. Arquivos

### Modelo

#### `Features/Feature.cs`

Entidade de domínio. O estado só muda pelos métodos `Create` e `Update`. O construtor privado sem parâmetros existe para o EF Core.

```csharp
namespace Platform.Api.Features;

public sealed class Feature
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private Feature() { } // usado pelo EF Core

    public static Feature Create(string name, string? description, DateTimeOffset now) => new()
    {
        Id = Guid.CreateVersion7(),
        Name = name,
        Description = description,
        CreatedAt = now
    };

    public void Update(string name, string? description, DateTimeOffset now)
    {
        Name = name;
        Description = description;
        UpdatedAt = now;
    }
}
```

#### `Features/FeatureError.cs`

Erros de negócio esperados da feature.

```csharp
namespace Platform.Api.Features;

public enum FeatureError
{
    NotFound,
    DuplicateName
}
```

#### `Features/FeatureResult.cs`

Retorno do service: contém o valor ou o erro, nunca os dois.

```csharp
namespace Platform.Api.Features;

public sealed record FeatureResult
{
    public Feature? Value { get; private init; }
    public FeatureError? Error { get; private init; }

    public static FeatureResult Success(Feature value) => new() { Value = value };
    public static FeatureResult Failure(FeatureError error) => new() { Error = error };
}
```

### Contratos HTTP (DTOs)

#### `Features/CreateFeatureRequest.cs`

Body do `POST`. Os atributos são validados automaticamente antes do handler rodar.

```csharp
using System.ComponentModel.DataAnnotations;

namespace Platform.Api.Features;

public sealed record CreateFeatureRequest
{
    [Required, StringLength(100)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }
}
```

#### `Features/UpdateFeatureRequest.cs`

Body do `PUT`.

```csharp
using System.ComponentModel.DataAnnotations;

namespace Platform.Api.Features;

public sealed record UpdateFeatureRequest
{
    [Required, StringLength(100)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }
}
```

#### `Features/FeatureResponse.cs`

Formato de saída da API. A entidade nunca é exposta diretamente.

```csharp
namespace Platform.Api.Features;

public sealed record FeatureResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static FeatureResponse From(Feature feature) => new(
        feature.Id, feature.Name, feature.Description, feature.CreatedAt, feature.UpdatedAt);
}
```

### Dados

#### `Features/IFeatureRepository.cs`

Contrato de persistência. É o ponto de troca por um fake nos testes.

```csharp
namespace Platform.Api.Features;

public interface IFeatureRepository
{
    Task<IReadOnlyList<Feature>> ListAsync(CancellationToken ct);
    Task<Feature?> FindAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, Guid? exceptId, CancellationToken ct);
    Task AddAsync(Feature feature, CancellationToken ct);
    Task UpdateAsync(Feature feature, CancellationToken ct);
    Task RemoveAsync(Feature feature, CancellationToken ct);
}
```

#### `Features/FeatureRepository.cs`

Implementação com EF Core. `AsNoTracking` em leitura pura para não rastrear entidades sem necessidade.

```csharp
using Microsoft.EntityFrameworkCore;
using Platform.Api.Data;

namespace Platform.Api.Features;

public sealed class FeatureRepository(AppDbContext db) : IFeatureRepository
{
    public async Task<IReadOnlyList<Feature>> ListAsync(CancellationToken ct) =>
        await db.Features.AsNoTracking().OrderBy(f => f.Name).ToListAsync(ct);

    public Task<Feature?> FindAsync(Guid id, CancellationToken ct) =>
        db.Features.FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<bool> ExistsByNameAsync(string name, Guid? exceptId, CancellationToken ct) =>
        db.Features.AnyAsync(f => f.Name == name && f.Id != exceptId, ct);

    public async Task AddAsync(Feature feature, CancellationToken ct)
    {
        db.Features.Add(feature);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Feature feature, CancellationToken ct)
    {
        db.Features.Update(feature);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(Feature feature, CancellationToken ct)
    {
        db.Features.Remove(feature);
        await db.SaveChangesAsync(ct);
    }
}
```

#### `Features/FeatureConfiguration.cs`

Mapeamento da entidade para a tabela, via Fluent API. O índice único garante a regra de nome único também no banco.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Platform.Api.Features;

public sealed class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("features");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).HasMaxLength(100).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(500);
        builder.HasIndex(f => f.Name).IsUnique();
    }
}
```

#### `Data/AppDbContext.cs`

Contexto do EF Core compartilhado entre features. Carrega automaticamente todas as classes `IEntityTypeConfiguration` do projeto.

```csharp
using Microsoft.EntityFrameworkCore;
using Platform.Api.Features;

namespace Platform.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Feature> Features => Set<Feature>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

### Regra de negócio

#### `Features/IFeatureService.cs`

Contrato da regra de negócio. Recebe valores simples, não DTOs, para não depender do formato da API e poder ser chamado por jobs ou outras features.

```csharp
namespace Platform.Api.Features;

public interface IFeatureService
{
    Task<IReadOnlyList<Feature>> ListAsync(CancellationToken ct);
    Task<Feature?> GetAsync(Guid id, CancellationToken ct);
    Task<FeatureResult> CreateAsync(string name, string? description, CancellationToken ct);
    Task<FeatureResult> UpdateAsync(Guid id, string name, string? description, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
```

#### `Features/FeatureService.cs`

Aplica as regras: nome único, existência do registro e datas. `TimeProvider` permite controlar o tempo nos testes.

```csharp
namespace Platform.Api.Features;

public sealed class FeatureService(IFeatureRepository repository, TimeProvider clock) : IFeatureService
{
    public Task<IReadOnlyList<Feature>> ListAsync(CancellationToken ct) =>
        repository.ListAsync(ct);

    public Task<Feature?> GetAsync(Guid id, CancellationToken ct) =>
        repository.FindAsync(id, ct);

    public async Task<FeatureResult> CreateAsync(string name, string? description, CancellationToken ct)
    {
        if (await repository.ExistsByNameAsync(name, exceptId: null, ct))
            return FeatureResult.Failure(FeatureError.DuplicateName);

        var feature = Feature.Create(name, description, clock.GetUtcNow());
        await repository.AddAsync(feature, ct);
        return FeatureResult.Success(feature);
    }

    public async Task<FeatureResult> UpdateAsync(Guid id, string name, string? description, CancellationToken ct)
    {
        var feature = await repository.FindAsync(id, ct);
        if (feature is null)
            return FeatureResult.Failure(FeatureError.NotFound);

        if (await repository.ExistsByNameAsync(name, exceptId: id, ct))
            return FeatureResult.Failure(FeatureError.DuplicateName);

        feature.Update(name, description, clock.GetUtcNow());
        await repository.UpdateAsync(feature, ct);
        return FeatureResult.Success(feature);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var feature = await repository.FindAsync(id, ct);
        if (feature is null) return false;

        await repository.RemoveAsync(feature, ct);
        return true;
    }
}
```

### Rotas

#### `Features/FeatureEndpoints.cs`

Declara as rotas da feature e traduz `FeatureResult` em status HTTP. Cada handler é um método estático; seus parâmetros vêm da rota (`id`), do body (`request`), do DI (`service`) e da requisição (`ct`).

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Platform.Api.Features;

public static class FeatureEndpoints
{
    const string GetByIdRoute = "GetFeatureById";

    public static RouteGroupBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/features").WithTags("Features");

        group.MapGet("/", List);
        group.MapGet("/{id:guid}", GetById).WithName(GetByIdRoute);
        group.MapPost("/", Create);
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);

        return group;
    }

    static async Task<Ok<FeatureResponse[]>> List(
        IFeatureService service, CancellationToken ct)
    {
        var features = await service.ListAsync(ct);
        return TypedResults.Ok(features.Select(FeatureResponse.From).ToArray());
    }

    static async Task<Results<Ok<FeatureResponse>, NotFound>> GetById(
        Guid id, IFeatureService service, CancellationToken ct)
    {
        var feature = await service.GetAsync(id, ct);
        return feature is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(FeatureResponse.From(feature));
    }

    static async Task<Results<CreatedAtRoute<FeatureResponse>, ProblemHttpResult>> Create(
        CreateFeatureRequest request, IFeatureService service, CancellationToken ct)
    {
        var result = await service.CreateAsync(request.Name, request.Description, ct);
        return result switch
        {
            { Error: FeatureError.DuplicateName } => DuplicateName(),
            { Value: { } feature } => TypedResults.CreatedAtRoute(
                FeatureResponse.From(feature), GetByIdRoute, new { id = feature.Id }),
            _ => throw new UnreachableException()
        };
    }

    static async Task<Results<NoContent, NotFound, ProblemHttpResult>> Update(
        Guid id, UpdateFeatureRequest request, IFeatureService service, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request.Name, request.Description, ct);
        return result.Error switch
        {
            null => TypedResults.NoContent(),
            FeatureError.NotFound => TypedResults.NotFound(),
            FeatureError.DuplicateName => DuplicateName(),
            _ => throw new UnreachableException()
        };
    }

    static async Task<Results<NoContent, NotFound>> Delete(
        Guid id, IFeatureService service, CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    static ProblemHttpResult DuplicateName() =>
        TypedResults.Problem("Já existe uma feature com esse nome.", statusCode: StatusCodes.Status409Conflict);
}
```

### Injeção de dependência

#### `Features/FeatureServiceCollectionExtensions.cs`

Registra os serviços da feature no container. O `Program.cs` chama uma linha por feature.

```csharp
namespace Platform.Api.Features;

public static class FeatureServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureServices(this IServiceCollection services)
    {
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IFeatureService, FeatureService>();
        return services;
    }
}
```

#### `Program.cs`

Ponto de entrada. Registra a infraestrutura compartilhada, os serviços de cada feature e mapeia as rotas.

```csharp
using Microsoft.EntityFrameworkCore;
using Platform.Api.Data;
using Platform.Api.Features;

var builder = WebApplication.CreateBuilder(args);

// Infraestrutura compartilhada
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Platform")));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddOpenApi();

// Features
builder.Services.AddFeatureServices();

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapGroup("/api").MapFeatureEndpoints();

app.Run();
```

#### `appsettings.json`

Connection string. Em ambiente real, a senha vai em `dotnet user-secrets` ou variável de ambiente.

```json
{
  "ConnectionStrings": {
    "Platform": "Host=localhost;Database=platform;Username=postgres;Password=postgres"
  }
}
```

---

## 7. Injeção no Program

### Duas etapas

1. **Registro (`builder.Services`)**: informa ao container qual implementação atende cada interface e qual o seu ciclo de vida.
2. **Mapeamento (`app.Map...`)**: registra as rotas. Os handlers pedem as interfaces como parâmetro e o container entrega as implementações.

### Ciclo de vida

| Registro | Ciclo de vida | Motivo |
|---|---|---|
| `AppDbContext` | Scoped (padrão do `AddDbContext`) | Uma instância por requisição; não é thread-safe |
| `IFeatureRepository` | Scoped | Depende do `DbContext` |
| `IFeatureService` | Scoped | Depende do repository |
| `TimeProvider` | Singleton | Sem estado, seguro para compartilhar |

Regra: um serviço nunca deve depender de outro com ciclo de vida mais curto (ex.: Singleton recebendo Scoped).

### Cadeia resolvida em `POST /api/features`

```
Create(handler)
└── IFeatureService → FeatureService
    ├── IFeatureRepository → FeatureRepository
    │   └── AppDbContext
    └── TimeProvider → TimeProvider.System
```

Toda a cadeia Scoped nasce e é descartada junto com a requisição.

### Adicionando uma nova feature

1. Criar a pasta da feature com os mesmos tipos (`Xxx`, `IXxxRepository`, `XxxEndpoints` etc.).
2. Adicionar o `DbSet<Xxx>` no `AppDbContext`.
3. No `Program.cs`, incluir `builder.Services.AddXxxServices();` e `api.MapXxxEndpoints();`.

---

## 8. Rotas geradas

| Verbo | Rota | Respostas |
|---|---|---|
| GET | `/api/features` | 200 |
| GET | `/api/features/{id}` | 200, 404 |
| POST | `/api/features` | 201 (com `Location`), 400, 409 |
| PUT | `/api/features/{id}` | 204, 400, 404, 409 |
| DELETE | `/api/features/{id}` | 204, 404 |

O status 400 vem da validação automática (`AddValidation`), antes do handler executar.

---

## 9. Banco de dados

```bash
dotnet ef migrations add InitialFeatures
dotnet ef database update
```
