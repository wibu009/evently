using System.Reflection;
using Evently.Common.Application;
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
using Evently.Modules.Attendance.Infrastructure.Outbox;
using Evently.Modules.Attendance.Infrastructure.Tickets;
using Evently.Modules.Attendance.Presentation.Attendees;
using Evently.Modules.Attendance.Presentation.Events;
using Evently.Modules.Attendance.Presentation.Tickets;
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
    }
    
    private static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.Load("Evently.Modules.Attendance.Application");
            
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
        #region Endpoints

        services.AddEndpointsFromAssembly(Assembly.Load("Evently.Modules.Attendance.Presentation"));

        #endregion

        #region Consumers

        // Attendees
        services.RegisterConsumer<UserProfileUpdatedIntegrationEventConsumer>();
        services.RegisterConsumer<UserRegisteredIntegrationEventConsumer>();
        
        // Events
        services.RegisterConsumer<EventPublishedIntegrationEventConsumer>();
        
        // Tickets
        services.RegisterConsumer<TicketIssuedIntegrationEventConsumer>();

        #endregion
    }
}
