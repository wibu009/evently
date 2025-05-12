using Evently.Common.Domain;

namespace Evently.Modules.Users.Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound("Users.NotFound", $"User with id {userId} not found");
    public static Error NotFound(string identityId) => Error.NotFound("Users.NotFound", $"User with IDP id {identityId} not found");
}
