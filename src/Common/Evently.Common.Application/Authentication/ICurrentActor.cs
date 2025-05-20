namespace Evently.Common.Application.Authentication;

public interface ICurrentActor
{
    Guid Id { get; }
    string IdentityId { get; }
}
