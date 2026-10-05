using System.Data.Common;
using Dapper;
using Evently.Common.Application.Data;
using Evently.Modules.Users.Application.Abstractions.Data;
using Evently.Modules.Users.Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Users.Infrastructure.Identity;

/// <summary>
/// With Keycloak-hosted registration (Authorization Code + PKCE), users can be
/// created outside of this module — self-registration and admin provisioning both
/// happen in Keycloak. This background service reconciles Keycloak users into the
/// Users module: the local row is created with the Member role and
/// <c>User.Create</c> raises the same <c>UserRegisteredDomainEvent</c> the
/// API-driven registration raises, so role → permission mapping keeps working no
/// matter where a user was provisioned.
/// </summary>
internal sealed class KeycloakUserSyncService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<KeycloakUserSyncService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Give the migrations a moment to complete before the first pass.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SynchronizeAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Keycloak user synchronization failed; retrying on the next interval");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    private async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope();
        KeyCloakClient keyCloakClient = scope.ServiceProvider.GetRequiredService<KeyCloakClient>();
        IDbConnectionFactory dbConnectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        IUserRepository userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        HashSet<string> localIdentityIds;
        await using (DbConnection connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken))
        {
            IEnumerable<string> ids = await connection.QueryAsync<string>("SELECT identity_id FROM users.users");
            localIdentityIds = [.. ids];
        }

        const int pageSize = 100;
        int importedCount = 0;
        int lastPageCount;

        int page = 0;
        while (true)
        {
            List<UserSummaryRepresentation> users = await keyCloakClient.ListUsersAsync(page, pageSize, cancellationToken);

            foreach (UserSummaryRepresentation user in users)
            {
                if (!user.Enabled || localIdentityIds.Contains(user.Id))
                {
                    continue;
                }

                var localUser = User.Create(user.Email, user.FirstName, user.LastName, user.Id);
                userRepository.Insert(localUser);

                // Saving raises UserRegisteredDomainEvent, which propagates the
                // UserRegisteredIntegrationEvent to Ticketing and Attendance.
                await unitOfWork.SaveChangesAsync(cancellationToken);
                localIdentityIds.Add(localUser.IdentityId);

                importedCount++;
            }

            lastPageCount = users.Count;
            if (lastPageCount < pageSize)
            {
                break;
            }

            page++;
        }

        if (importedCount > 0)
        {
            logger.LogInformation("Imported {Count} Keycloak user(s) into the Users module", importedCount);
        }
    }
}
