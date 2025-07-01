using System.Reflection;
using Evently.Common.Application;
using Evently.Common.Application.EventBus;
using Evently.Common.Application.Messaging;
using Evently.Common.Infrastructure.Configuration;
using Evently.Common.Infrastructure.Outbox;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Events.IntegrationEvents.Events;
using Evently.Modules.Events.IntegrationEvents.TicketTypes;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Application.Abstractions.Payments;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Evently.Modules.Ticketing.Domain.Tickets;
using Evently.Modules.Ticketing.Infrastructure.Customers;
using Evently.Modules.Ticketing.Infrastructure.Database;
using Evently.Modules.Ticketing.Infrastructure.Events;
using Evently.Modules.Ticketing.Infrastructure.Inbox;
using Evently.Modules.Ticketing.Infrastructure.Orders;
using Evently.Modules.Ticketing.Infrastructure.Outbox;
using Evently.Modules.Ticketing.Infrastructure.Payments;
using Evently.Modules.Ticketing.Infrastructure.Tickets;
using Evently.Modules.Users.IntegrationEvents.Users;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Evently.Modules.Ticketing.Infrastructure;

public static class TicketingModule
{
    public static void AddTicketingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();
    }
    
    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database

        string databaseConnectionString = configuration.GetConnectionStringOrThrow("Database");
        
        services.AddDbContext<TicketingDbContext>((sp, options) => options
            .UseNpgsql(
                databaseConnectionString,
                npgsqlOptionsAction => npgsqlOptionsAction
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Ticketing))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TicketingDbContext>());

        #endregion

        #region Customer

        services.AddScoped<ICustomerRepository, CustomerRepository>();

        #endregion

        #region Events

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ITicketTypeRepository, TicketTypeRepository>();

        #endregion

        #region Tickets
        
        services.AddScoped<ITicketRepository, TicketRepository>();
        
        #endregion

        #region Orders

        services.AddScoped<IOrderRepository, OrderRepository>();

        #endregion
        
        #region Payments
        
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddSingleton<IPaymentService, PaymentService>();
        
        #endregion
        
        #region Cart
        
        services.AddSingleton<CartService>();
        
        #endregion

        #region Outbox

        services.Configure<OutboxOptions>(configuration.GetSection("Ticketing:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        #endregion

        #region Inbox

        services.Configure<InboxOptions>(configuration.GetSection("Ticketing:Inbox"));
        services.ConfigureOptions<ConfigureProcessInboxJob>();

        #endregion
    }

    private static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = Assembly.Load("Evently.Modules.Ticketing.Application");

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
        var presentationAssembly = Assembly.Load("Evently.Modules.Ticketing.Presentation");
        
        services.AddEndpointsFromAssembly(presentationAssembly);

        services.AddMassTransit(cfg =>
        {
            // Customers
            cfg.AddConsumer<IntegrationEventConsumer<UserRegisteredIntegrationEvent>>();
            cfg.AddConsumer<IntegrationEventConsumer<UserProfileUpdatedIntegrationEvent>>();
        
            //Events
            cfg.AddConsumer<IntegrationEventConsumer<EventPublishedIntegrationEvent>>();
        
            //Ticket Types
            cfg.AddConsumer<IntegrationEventConsumer<TicketTypePriceChangedIntegrationEvent>>();
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
