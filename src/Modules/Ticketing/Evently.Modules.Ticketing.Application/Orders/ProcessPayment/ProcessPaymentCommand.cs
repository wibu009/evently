using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Orders.ProcessPayment;

public sealed record ProcessPaymentCommand(Guid PaymentId) : ICommand;
