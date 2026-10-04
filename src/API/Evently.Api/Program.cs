using Evently.Api.Extensions;
using Evently.Api.Middleware;
using Evently.Api.OpenTelemetry;
using Evently.Api.Seeding;
using Evently.Common.Infrastructure;
using Evently.Common.Infrastructure.Configuration;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Attendance.Infrastructure;
using Evently.Modules.Events.Infrastructure;
using Evently.Modules.Users.Infrastructure;
using Evently.ServiceDefaults;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MongoDB.Driver;
using RabbitMQ.Client;
using Scalar.AspNetCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//Logging Setup
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

// Module Setup
builder.Configuration.AddModuleConfiguration("events", "users", "attendance");

builder.Services.AddEventsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddAttendanceModule(builder.Configuration);

builder.Services.AddInfrastructure(DiagnosticsConfig.ServiceName, builder.Configuration);

builder.Services.AddHostedService<SampleDataSeeder>();

// Other Setup
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddCors();

builder.Services.AddOpenApiDocumentation();

builder.Services
    .AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionStringOrThrow("writedb"))
    .AddMongoDb(_ => new MongoClient(builder.Configuration.GetConnectionStringOrThrow("readdatabase")))
    .AddRedis(builder.Configuration.GetConnectionStringOrThrow("cache"))
    .AddRabbitMQ(_ => new ConnectionFactory
        {
            Uri = new Uri(builder.Configuration.GetConnectionStringOrThrow("queue"))
        }
        .CreateConnectionAsync().GetAwaiter().GetResult())
    .AddUrlGroup(new Uri(builder.Configuration.GetValueOrThrow<string>("KeyCloak:HealthUrl")), HttpMethod.Get, "keycloak");

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors(corsPolicyBuilder =>
        corsPolicyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

    app.MapOpenApi();
    app.MapScalarApiReference();
    
    app.ApplyMigrations();
}

app.MapEndpoints();

app.MapDefaultEndpoints();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseLogContextTraceLogging();
app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();

#pragma warning disable CA1515
public partial class Program;
#pragma warning restore CA1515
