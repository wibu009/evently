using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Payments.RefundOrder;

public sealed record RefundOrderCommand(Guid OrderId, string? Reason) : ICommand;
