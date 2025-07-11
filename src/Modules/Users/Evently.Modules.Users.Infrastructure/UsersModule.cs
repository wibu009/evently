﻿using System.Reflection;
using Evently.Common.Application;
using Evently.Common.Application.Authorization;
using Evently.Common.Application.EventBus;
using Evently.Common.Application.Messaging;
using Evently.Common.Infrastructure.Configuration;
using Evently.Common.Infrastructure.Outbox;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Users.Application.Abstractions.Data;
using Evently.Modules.Users.Application.Abstractions.Identity;
using Evently.Modules.Users.Domain.Users;
using Evently.Modules.Users.Infrastructure.Authorization;
using Evently.Modules.Users.Infrastructure.Database;
using Evently.Modules.Users.Infrastructure.Identity;
using Evently.Modules.Users.Infrastructure.Inbox;
using Evently.Modules.Users.Infrastructure.Outbox;
using Evently.Modules.Users.Infrastructure.Users;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Evently.Modules.Users.Infrastructure;

public static class UsersModule
{
    public static void AddUsersModule( this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();
    }
    
    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database

        string databaseConnectionString = configuration.GetConnectionStringOrThrow("Database");

        services.AddDbContext<UsersDbContext>((sp, options)
            => options
                .UseNpgsql(databaseConnectionString, npgsqlOptions => npgsqlOptions
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Users))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UsersDbContext>());

        #endregion

        #region Users

        services.AddScoped<IUserRepository, UserRepository>();

        #endregion

        #region Identity
        
        services.Configure<KeyCloakOptions>(configuration.GetSection("Users:KeyCloak"));

        services.AddTransient<KeyCloakAuthDelegatingHandler>();

        services.AddHttpClient<KeyCloakClient>((serviceProvider, httpClient) =>
            {
                KeyCloakOptions keyCloakOptions = serviceProvider.GetRequiredService<IOptions<KeyCloakOptions>>().Value;
                httpClient.BaseAddress = new Uri(keyCloakOptions.AdminUrl);
            })
            .AddHttpMessageHandler<KeyCloakAuthDelegatingHandler>();

        services.AddTransient<IIdentityProviderService, IdentityProviderService>();

        #endregion

        #region Authorization

        services.AddScoped<IPermissionService, PermissionService>();

        #endregion

        #region Outbox

        services.Configure<OutboxOptions>(configuration.GetSection("Users:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        #endregion

        #region Inbox
        
        services.Configure<InboxOptions>(configuration.GetSection("Users:Inbox"));
        services.ConfigureOptions<ConfigureProcessInboxJob>();

        #endregion
    }

    private static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.Load("Evently.Modules.Users.Application");
            
        services.AddApplicationFromAssembly(applicationAssembly);

        Type[] domainEventHandlers = [.. applicationAssembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))];
        foreach (Type domainEventHandler in domainEventHandlers)
        {
            services.TryAddScoped(domainEventHandler);

            Type domainEvent = domainEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();
            
            Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);
            
            services.Decorate(domainEventHandler, closedIdempotentHandler);
        }
    }


    private static void AddPresentation(this IServiceCollection services)
    {
        var presentationAssembly = Assembly.Load("Evently.Modules.Users.Presentation");

        services.AddEndpointsFromAssembly(presentationAssembly);
        
        services.AddMassTransit(cfg =>
        {
            Type[] integrationEventTypes = presentationAssembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
                .Select(t => t.GetInterfaces()
                    .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>))
                    .GetGenericArguments()
                    .Single())
                .ToArray();
            
            foreach (Type eventType in integrationEventTypes)
            {
                Type consumerType = typeof(IntegrationEventConsumer<>).MakeGenericType(eventType);
                cfg.AddConsumer(consumerType);
            }
        });
        
        Type[] integrationEventHandlers = [.. presentationAssembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))];
        foreach (Type integrationEventHandler in integrationEventHandlers)
        {
            services.TryAddScoped(integrationEventHandler);

            Type integrationEvent = integrationEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler =
                typeof(IdempotentIntegrationEventHandler<>).MakeGenericType(integrationEvent);

            services.Decorate(integrationEventHandler, closedIdempotentHandler);
        }
    }
}
