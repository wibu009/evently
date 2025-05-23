using System.Reflection;
using Evently.Common.Application;
using Evently.Common.Infrastructure;
using Evently.Common.Infrastructure.Interceptors;
using Evently.Common.Presentation.Endpoints;
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
using Evently.Modules.Ticketing.Infrastructure.Orders;
using Evently.Modules.Ticketing.Infrastructure.Payments;
using Evently.Modules.Ticketing.Infrastructure.Tickets;
using Evently.Modules.Ticketing.Presentation.Customers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Evently.Modules.Ticketing.Infrastructure;

public static class TicketingModule
{
    private static readonly Assembly CurrentAssembly = typeof(TicketingModule).Assembly;
    
    public static void AddTicketingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();
    }
    
    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database

        string databaseConnectionString = configuration.GetConnectionString("Database")!;
        
        services.AddDbContext<TicketingDbContext>((sp, options) => options
            .UseNpgsql(
                databaseConnectionString,
                npgsqlOptionsAction => npgsqlOptionsAction
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Ticketing))
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));
        
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
    }
    
    private static void AddApplication(this IServiceCollection services)
    {
        services.AddApplicationFromAssembly(CurrentAssembly.GetLayerAssembly("Application"));
    }

    private static void AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsFromAssembly(CurrentAssembly.GetLayerAssembly("Presentation"));
    }
}
