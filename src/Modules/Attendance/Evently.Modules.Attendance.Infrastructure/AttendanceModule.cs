using System.Reflection;
using System.Runtime.Loader;
using Evently.Common.Application;
using Evently.Common.Infrastructure.Interceptors;
using Evently.Common.Presentation.Endpoints;
using Evently.Modules.Attendance.Application.Abstractions.Data;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.Domain.Events;
using Evently.Modules.Attendance.Domain.Tickets;
using Evently.Modules.Attendance.Infrastructure.Attendees;
using Evently.Modules.Attendance.Infrastructure.Database;
using Evently.Modules.Attendance.Infrastructure.Events;
using Evently.Modules.Attendance.Infrastructure.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Evently.Modules.Attendance.Infrastructure;

public static class AttendanceModule
{
    public static void AddAttendanceModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region Database

        services.AddDbContext<AttendanceDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Attendance))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>()));
        
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
    }
    
    private static void AddApplication(this IServiceCollection services)
    {
        services.AddApplicationFromAssembly(GetAssembly("Application"));
    }
    
    private static void AddPresentation(this IServiceCollection services)
    {
        services.AddEndpointsFromAssembly(GetAssembly("Presentation"));
    }
    
    private static Assembly GetAssembly(string layer)
    {
        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string path = Path.Combine(dir, $"Evently.Modules.Attendance.{layer}.dll");

        return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    }
}
