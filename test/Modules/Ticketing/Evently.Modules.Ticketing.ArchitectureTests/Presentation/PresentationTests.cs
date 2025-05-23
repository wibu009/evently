using Evently.Modules.Ticketing.ArchitectureTests.Abstractions;
using MassTransit;
using NetArchTest.Rules;

namespace Evently.Modules.Ticketing.ArchitectureTests.Presentation;

public class PresentationTests : BaseTest
{
    #region IntegrationEventHandler Tests
    
    [Fact]
    public void IntegrationEventHandler_Should_BeSealed()
    {
        Types.InAssembly(PresentationAssembly)
            .That()
            .ImplementInterface(typeof(IConsumer<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void IntegrationEventHandler_ShouldHave_NameEndingWith_Consumer()
    {
        Types.InAssembly(PresentationAssembly)
            .That()
            .ImplementInterface(typeof(IConsumer<>))
            .Should()
            .HaveNameEndingWith("Consumer")
            .GetResult()
            .ShouldBeSuccessful();
    }
    
    #endregion
}
