using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Orders.RestockOrderItems;

public sealed record RestockOrderItemsCommand(Guid OrderId) : ICommand;
