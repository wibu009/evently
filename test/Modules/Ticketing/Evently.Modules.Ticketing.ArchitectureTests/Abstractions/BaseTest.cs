using System.Reflection;
using System.Runtime.Loader;

namespace Evently.Modules.Ticketing.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected static readonly Assembly ApplicationAssembly = GetAssembly("Application");
    protected static readonly Assembly DomainAssembly = GetAssembly("Domain");
    protected static readonly Assembly InfrastructureAssembly = GetAssembly("Infrastructure");
    protected static readonly Assembly PresentationAssembly = GetAssembly("Presentation");

    private static Assembly GetAssembly(string layer)
    {
        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string path = Path.Combine(dir, $"Evently.Modules.Ticketing.{layer}.dll");

        return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
    }
}
