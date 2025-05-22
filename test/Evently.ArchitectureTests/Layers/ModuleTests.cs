using System.Reflection;
using Evently.ArchitectureTests.Abstractions;
using NetArchTest.Rules;

namespace Evently.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
    [Fact]
    public void UsersModule_ShouldNoHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules =
        [
            EventsNamespace,
            TicketingNamespace,
            AttendanceNamespace
        ];
        string[] integrationEventsModules =
        [
            EventsIntegrationEventsNamespace,
            TicketingIntegrationEventsNamespace,
            AttendanceIntegrationEventsNamespace
        ];
        List<Assembly> usersAssemblies = GetModuleAssemblies(
            Assembly.GetExecutingAssembly(),
            UsersModule, 
            UsersIntegrationEventsNamespace);
        
        Types.InAssemblies(usersAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void EventsModule_ShouldNoHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules =
        [
            UsersNamespace,
            TicketingNamespace,
            AttendanceNamespace
        ];
        string[] integrationEventsModules =
        [
            UsersIntegrationEventsNamespace,
            TicketingIntegrationEventsNamespace,
            AttendanceIntegrationEventsNamespace
        ];
        List<Assembly> eventsAssemblies = GetModuleAssemblies(
            Assembly.GetExecutingAssembly(),
            EventsModule, 
            EventsIntegrationEventsNamespace);
        
        Types.InAssemblies(eventsAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
    
    [Fact]
    public void TicketingModule_ShouldNoHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules =
        [
            UsersNamespace,
            EventsNamespace,
            AttendanceNamespace
        ];
        string[] integrationEventsModules =
        [
            UsersIntegrationEventsNamespace,
            EventsIntegrationEventsNamespace,
            AttendanceIntegrationEventsNamespace
        ];
        List<Assembly> ticketingAssemblies = GetModuleAssemblies(
            Assembly.GetExecutingAssembly(),
            TicketingModule, 
            TicketingIntegrationEventsNamespace);
        
        Types.InAssemblies(ticketingAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
    
    [Fact]
    public void AttendanceModule_ShouldNoHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules =
        [
            UsersNamespace,
            EventsNamespace,
            TicketingNamespace
        ];
        string[] integrationEventsModules =
        [
            UsersIntegrationEventsNamespace,
            EventsIntegrationEventsNamespace,
            TicketingIntegrationEventsNamespace
        ];
        List<Assembly> attendanceAssemblies = GetModuleAssemblies(
            Assembly.GetExecutingAssembly(),
            AttendanceModule, 
            AttendanceIntegrationEventsNamespace);
        
        Types.InAssemblies(attendanceAssemblies)
            .That()
            .DoNotHaveDependencyOnAny(integrationEventsModules)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
