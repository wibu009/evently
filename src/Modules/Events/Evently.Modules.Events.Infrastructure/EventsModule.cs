using System.Reflection;
using Evently.Common.Application;
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
using Evently.Modules.Events.Infrastructure.Outbox;
using Evently.Modules.Events.Infrastructure.TicketTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Evently.Modules.Events.Infrastructure;

public static class EventsModule
{
    public static void AddEventsModule( this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();
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

    private static void AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsFromAssembly(Assembly.Load("Evently.Modules.Events.Presentation"));
    }
}
