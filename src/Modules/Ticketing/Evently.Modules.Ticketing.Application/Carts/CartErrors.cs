﻿using Evently.Common.Domain;

namespace Evently.Modules.Ticketing.Application.Carts;

public static class CartErrors
{
    public static readonly Error Empty = Error.NotFound("Carts.Empty", "Cart is empty");
}
