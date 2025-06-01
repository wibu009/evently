using Evently.Api.Extensions;
using Evently.Api.Middleware;
using Evently.Common.Infrastructure;
using Evently.Common.Infrastructure.Configuration;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Attendance.Infrastructure;
using Evently.Modules.Events.Infrastructure;
using Evently.Modules.Ticketing.Infrastructure;
using Evently.Modules.Users.Infrastructure;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//Logging Setup
builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

// Module Setup
builder.Services.AddCoreServices(builder.Configuration);

builder.Configuration.AddModuleConfiguration("events", "users", "ticketing", "attendance");

builder.Services.AddEventsModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);
builder.Services.AddTicketingModule(builder.Configuration);
builder.Services.AddAttendanceModule(builder.Configuration);

// Other Setup
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApiDocumentation();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionStringOrThrow("Database"))
    .AddRedis(builder.Configuration.GetConnectionStringOrThrow("Cache"))
    .AddUrlGroup(new Uri(builder.Configuration.GetValueOrThrow<string>("KeyCloak:HealthUrl")), HttpMethod.Get, "keycloak");

WebApplication app = builder.Build();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    
    app.ApplyMigrations();
}

app.MapEndpoints();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();
