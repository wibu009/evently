using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Orders.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string? Reason) : ICommand;
