using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace Evently.IntegrationTests.Abstractions;

extern alias EventlyApi;

#pragma warning disable CA1515
public sealed class EventlyWebAppFactory(TestEnvironment env) : WebApplicationFactory<EventlyApi::Program>
#pragma warning restore CA1515
{
#pragma warning disable CA1823
#pragma warning disable IDE0052
    private readonly TestEnvironment _env = env;
#pragma warning restore IDE0052
#pragma warning restore CA1823

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            // Force override env variables.
            EnvironmentVariablesConfigurationSource? envSource = cfg.Sources.OfType<EnvironmentVariablesConfigurationSource>().FirstOrDefault();
            if (envSource != null)
            {
                cfg.Sources.Remove(envSource);
            }
            cfg.AddEnvironmentVariables();
        });
    }
}
