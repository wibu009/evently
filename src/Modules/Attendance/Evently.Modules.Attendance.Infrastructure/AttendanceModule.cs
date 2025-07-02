using System.Reflection;
using Evently.Common.Application;
using Evently.Common.Application.EventBus;
using Evently.Common.Application.Messaging;
using Evently.Common.Infrastructure.Configuration;
using Evently.Common.Infrastructure.Outbox;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Attendance.Application.Abstractions.Data;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.Domain.Events;
using Evently.Modules.Attendance.Domain.Tickets;
using Evently.Modules.Attendance.Infrastructure.Attendees;
using Evently.Modules.Attendance.Infrastructure.Database;
using Evently.Modules.Attendance.Infrastructure.Events;
using Evently.Modules.Attendance.Infrastructure.Inbox;
using Evently.Modules.Attendance.Infrastructure.Outbox;
using Evently.Modules.Attendance.Infrastructure.Tickets;
using Evently.Modules.Events.IntegrationEvents;
using Evently.Modules.Events.IntegrationEvents.Events;
using Evently.Modules.Ticketing.IntegrationEvents;
using Evently.Modules.Ticketing.IntegrationEvents.Tickets;
using Evently.Modules.Users.IntegrationEvents;
using Evently.Modules.Users.IntegrationEvents.Users;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Evently.Modules.Attendance.Infrastructure;

public static class AttendanceModule
{
    public static void AddAttendanceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database

        services.AddDbContext<AttendanceDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionStringOrThrow("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Attendance))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AttendanceDbContext>());

        #endregion

        #region Events
        
        services.AddScoped<IEventRepository, EventRepository>();

        #endregion

        #region Attendees

        services.AddScoped<IAttendeeRepository, AttendeeRepository>();

        #endregion
        
        #region Tickets

        services.AddScoped<ITicketRepository, TicketRepository>();

        #endregion

        #region Outbox

        services.Configure<OutboxOptions>(configuration.GetSection("Attendance:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        #endregion
        
        #region Inbox

        services.Configure<InboxOptions>(configuration.GetSection("Attendance:Inbox"));
        services.ConfigureOptions<ConfigureProcessInboxJob>();

        #endregion
    }
    
    private static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.Load("Evently.Modules.Attendance.Application");
            
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
        var presentationAssembly = Assembly.Load("Evently.Modules.Attendance.Presentation");
        
        services.AddEndpointsFromAssembly(presentationAssembly);

        #region Consumers

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

        #endregion
        
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
