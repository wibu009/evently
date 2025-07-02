using Bogus;
using Evently.Common.Domain;

namespace Evently.Modules.Users.Domain.UnitTests.Abstractions;

#pragma warning disable CA1515
public abstract class BaseTest
#pragma warning restore CA1515
{
    protected static readonly Faker Faker = new();

    protected static T AssertDomainEventWasPublished<T>(Entity entity) where T : IDomainEvent
    {
        T? domainEvent = entity.DomainEvents.OfType<T>().SingleOrDefault();

        if (domainEvent is null)
        {
            throw new Exception($"{typeof(T).Name} as not published");
        }
        
        return domainEvent;
    }
}
