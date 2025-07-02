using System.Reflection;
using Evently.Common.Application;
using Evently.Common.Application.EventBus;
using Evently.Common.Application.Messaging;
using Evently.Common.Infrastructure.Configuration;
using Evently.Common.Infrastructure.Outbox;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Events.Application.Abstractions.Data;
using Evently.Modules.Events.Domain.Categories;
using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.Domain.TicketTypes;
using Evently.Modules.Events.Infrastructure.Categories;
using Evently.Modules.Events.Infrastructure.Database;
using Evently.Modules.Events.Infrastructure.Events;
using Evently.Modules.Events.Infrastructure.Inbox;
using Evently.Modules.Events.Infrastructure.Outbox;
using Evently.Modules.Events.Infrastructure.TicketTypes;
using Evently.Modules.Events.Presentation.Events;
using MassTransit;
using MassTransit.Configuration;
using MassTransit.RedisIntegration.Saga;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace Evently.Modules.Events.Infrastructure;

public static class EventsModule
{
    public static void AddEventsModule( this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation(configuration);
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database

        string databaseConnectionString = configuration.GetConnectionStringOrThrow("Database");

        services.AddDbContext<EventsDbContext>((sp, options) => options
            .UseNpgsql(
                databaseConnectionString,
                npgsqlOptionsAction => npgsqlOptionsAction
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Events))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EventsDbContext>());

        #endregion

        #region Events

        services.AddScoped<IEventRepository, EventRepository>();

        #endregion

        #region Ticket Types

        services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();

        #endregion

        #region Categories

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        
        #endregion
        
        #region Outbox

        services.Configure<OutboxOptions>(configuration.GetSection("Events:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        #endregion
        
        #region Inbox

        services.Configure<InboxOptions>(configuration.GetSection("Events:Inbox"));
        services.ConfigureOptions<ConfigureProcessInboxJob>();

        #endregion
    }
    
    private static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.Load("Evently.Modules.Events.Application");
            
        services.AddApplicationFromAssembly(applicationAssembly);

        Type[] domainEventHandlers = applicationAssembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToArray();
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

    private static void AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        var presentationAssembly = Assembly.Load("Evently.Modules.Events.Presentation");

        services.AddEndpointsFromAssembly(presentationAssembly);

        string redisConnectionString = configuration.GetConnectionStringOrThrow("Cache");

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
            
            ISagaRegistrationConfigurator<CancelEventState>? cancelEventSagaConfig = cfg.AddSagaStateMachine<CancelEventSaga, CancelEventState>();

            try
            {
                ConnectionMultiplexer.Connect(redisConnectionString);
                cancelEventSagaConfig.RedisRepository(redisConnectionString);
            }
            catch
            {
                cancelEventSagaConfig.InMemoryRepository();
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
