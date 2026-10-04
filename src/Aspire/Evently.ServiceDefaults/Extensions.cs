// Copied from the .NET Aspire ServiceDefaults template and trimmed to what
// Evently needs: the modules already configure OpenTelemetry (tracing, metrics,
// Seq/Jaeger exporters) in Evently.Common.Infrastructure and already expose a
// custom /health endpoint, so this file only adds service discovery, resilient
// HTTP clients, and a /alive liveness probe for the orchestrator.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Evently.ServiceDefaults;

/// <summary>
/// Shared cross-cutting defaults for the Evently hosts running under .NET Aspire.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Registers service discovery, standard HTTP resilience, and a self liveness check.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static void AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Outgoing HTTP (e.g. the Users module Keycloak client) gets retries,
            // circuit breaking, timeouts, and service discovery names for free.
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        builder.Services
            .AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);
    }

    /// <summary>
    /// Maps the liveness probe. The readiness/detail endpoint (/health) is mapped
    /// separately by each host with the HealthChecks UI response writer.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application.</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });

        return app;
    }
}
