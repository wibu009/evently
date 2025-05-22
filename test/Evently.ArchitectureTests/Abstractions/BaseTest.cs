using System.Reflection;
using System.Runtime.Loader;

namespace Evently.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected const string UsersModule = "Users";
    protected const string UsersNamespace = "Evently.Modules.Users";
    protected const string UsersIntegrationEventsNamespace = "Evently.Modules.Users.IntegrationEvents";

    protected const string EventsModule = "Events";
    protected const string EventsNamespace = "Evently.Modules.Events";
    protected const string EventsIntegrationEventsNamespace = "Evently.Modules.Events.IntegrationEvents";

    protected const string TicketingModule = "Ticketing";
    protected const string TicketingNamespace = "Evently.Modules.Ticketing";
    protected const string TicketingIntegrationEventsNamespace = "Evently.Modules.Ticketing.IntegrationEvents";

    protected const string AttendanceModule = "Attendance";
    protected const string AttendanceNamespace = "Evently.Modules.Attendance";
    protected const string AttendanceIntegrationEventsNamespace = "Evently.Modules.Attendance.IntegrationEvents";
    
    protected static List<Assembly> GetModuleAssemblies(Assembly source, string moduleName, params string[] excludePatterns)
    {
        var loadedAssemblies = source.GetReferencedAssemblies()
            .Where(a => a.Name?.StartsWith($"Evently.Modules.{moduleName}", StringComparison.Ordinal) == true &&
                        excludePatterns.All(p => a.Name?.Contains(p) != true))
            .Select(a =>
            {
                try
                {
                    return Assembly.Load(a);
                }
                catch
                {
                    return null;
                }
            })
            .Where(a => a != null)
            .ToList();
        
        if (loadedAssemblies.Any())
        {
            return loadedAssemblies!;
        }

        string executingAssemblyPath = source.Location;
        string? directory = Path.GetDirectoryName(executingAssemblyPath);
        if (directory == null)
        {
            return loadedAssemblies!;
        }

        IEnumerable<string> potentialDlls = Directory.GetFiles(directory, $"Evently.Modules.{moduleName}*.dll")
            .Where(f => !excludePatterns.Any(f.Contains));

        foreach (string dll in potentialDlls)
        {
            try
            {
                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                loadedAssemblies.Add(assembly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly {dll}: {ex.Message}");
            }
        }

        return loadedAssemblies!;
    }
}
