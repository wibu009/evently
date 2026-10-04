using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(Guid CustomerId, string? PromoCode = null) : ICommand<Guid>;
