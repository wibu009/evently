using System.Reflection;

namespace Evently.Modules.Users.ArchitectureTests.Abstractions;

#pragma warning disable CA1515 // A file name should match its class name
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected static readonly Assembly ApplicationAssembly = Assembly.Load("Evently.Modules.Users.Application");
    protected static readonly Assembly DomainAssembly = Assembly.Load("Evently.Modules.Users.Domain");
    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("Evently.Modules.Users.Infrastructure");
    protected static readonly Assembly PresentationAssembly = Assembly.Load("Evently.Modules.Users.Presentation");
}
