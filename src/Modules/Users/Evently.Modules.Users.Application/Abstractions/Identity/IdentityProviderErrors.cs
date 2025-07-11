﻿using Evently.Common.Domain;

namespace Evently.Modules.Users.Application.Abstractions.Identity;

public static class IdentityProviderErrors
{
    public static readonly Error EmailIsNotUnique = Error.Conflict("IdentityProvider.EmailIsNotUnique", "Email is not unique");
}
