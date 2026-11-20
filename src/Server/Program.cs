using SampleServer.Endpoints.Health;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(
        "Configuration/appsettings.json",
        optional: false,
        reloadOnChange: true
    )
    .AddJsonFile(
        $"Configuration/appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true
    )
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthEndpoints();

app.Run();