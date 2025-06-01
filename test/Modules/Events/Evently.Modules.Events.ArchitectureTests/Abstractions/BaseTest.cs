using System.Reflection;

namespace Evently.Modules.Events.ArchitectureTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected static readonly Assembly ApplicationAssembly = Assembly.Load("Evently.Modules.Events.Application");
    protected static readonly Assembly DomainAssembly = Assembly.Load("Evently.Modules.Events.Domain");
    protected static readonly Assembly InfrastructureAssembly = Assembly.Load("Evently.Modules.Events.Infrastructure");
    protected static readonly Assembly PresentationAssembly = Assembly.Load("Evently.Modules.Events.Presentation");
}
