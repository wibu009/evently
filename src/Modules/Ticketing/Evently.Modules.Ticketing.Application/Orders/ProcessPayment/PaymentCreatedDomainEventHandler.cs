using Evently.Common.Application.Exceptions;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.Payments;
using MediatR;

namespace Evently.Modules.Ticketing.Application.Orders.ProcessPayment;

/// <summary>
/// Triggers the payment processing step whenever a new pending payment is created.
/// Runs through the outbox, so the gateway call never happens inside the checkout transaction.
/// </summary>
internal sealed class PaymentCreatedDomainEventHandler(ISender sender) : DomainEventHandler<PaymentCreatedDomainEvent>
{
    public override async Task Handle(PaymentCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        Result result = await sender.Send(new ProcessPaymentCommand(domainEvent.PaymentId), cancellationToken);

        if (result.IsFailure)
        {
            throw new EventlyException(nameof(ProcessPaymentCommand), result.Error);
        }
    }
}
