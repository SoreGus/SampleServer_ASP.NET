namespace SampleServer.Endpoints.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(
        this IEndpointRouteBuilder endpoints
    ) {
        endpoints.MapHealthChecks("/health");

        return endpoints;
    }
}