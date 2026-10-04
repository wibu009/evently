using System.Net.Sockets;
using Evently.Modules.Ticketing.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Evently.Ticketing.Api.Extensions;

internal static class MigrationExtensions
{
    private const int MaxAttempts = 10;

    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        ApplyMigration<TicketingDbContext>(app.ApplicationServices);
    }

    /// <summary>
    /// Applies pending migrations, retrying transient infrastructure failures
    /// (e.g. the database still starting up when the orchestrator launches
    /// dependents). Genuine configuration errors fail fast on the first attempt.
    /// </summary>
    private static void ApplyMigration<TDbContext>(IServiceProvider services) where TDbContext : DbContext
    {
        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                using IServiceScope scope = services.CreateScope();
                using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();
                context.Database.Migrate();

                return;
            }
            catch (NpgsqlException exception) when (IsTransient(exception) && attempt < MaxAttempts)
            {
                Thread.Sleep(TimeSpan.FromSeconds(Math.Min(2 * attempt, 10)));
            }
        }
    }

    private static bool IsTransient(NpgsqlException exception)
    {
        if (exception.IsTransient)
        {
            return true;
        }

        return exception.InnerException is SocketException socketException &&
            socketException.SocketErrorCode is SocketError.ConnectionRefused or SocketError.TimedOut;
    }
}
