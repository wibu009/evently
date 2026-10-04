using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Orders.ExpireOrder;

public sealed record ExpireOrderCommand(Guid OrderId) : ICommand;
