using System.Reflection;
using Evently.Common.Domain;
using Evently.Modules.Events.ArchitectureTests.Abstractions;
using FluentAssertions;
using NetArchTest.Rules;

namespace Evently.Modules.Events.ArchitectureTests.Domain;

public class DomainTests : BaseTest
{
    #region DomainEvent Tests
    
    [Fact]
    public void DomainEvents_Should_BeSealed()
    {
        Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(DomainEvent))
            .Or()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }
    
    [Fact]
    public void DomainEvent_ShouldHave_DomainEventPostfix()
    {
        Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(DomainEvent))
            .Or()
            .ImplementInterface(typeof(IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .GetResult()
            .ShouldBeSuccessful();
    }
    
    #endregion
    
    #region Entity Tests
    
    [Fact]
    public void Entity_ShouldHave_PrivateParameterlessConstructor()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.NonPublic |
                                                                        BindingFlags.Instance);

            if (!constructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
            {
                failingTypes.Add(entityType);
            }
        }

        failingTypes.Should().BeEmpty();
    }
    
    [Fact]
    public void Entity_ShouldOnlyHave_PrivateConstructors()
    {
        IEnumerable<Type> entityTypes = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(Entity))
            .GetTypes();

        var failingTypes = new List<Type>();
        foreach (Type entityType in entityTypes)
        {
            ConstructorInfo[] constructors = entityType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructors.Any())
            {
                failingTypes.Add(entityType);
            }
        }

        failingTypes.Should().BeEmpty();
    }
    
    #endregion
}
