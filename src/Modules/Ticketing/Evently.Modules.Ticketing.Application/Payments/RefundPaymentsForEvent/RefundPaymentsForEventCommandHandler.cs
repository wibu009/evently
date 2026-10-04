using System.Data.Common;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;

namespace Evently.Modules.Ticketing.Application.Payments.RefundPaymentsForEvent;

/// <summary>
/// Refunds every successful payment that belongs to the given event. This command is executed
/// as part of the event cancellation saga. Orders that end up fully refunded transition to the
/// refunded state, which triggers the compensation steps (ticket archival and inventory release).
/// </summary>
internal sealed class RefundPaymentsForEventCommandHandler(
    IEventRepository eventRepository,
    IPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefundPaymentsForEventCommand>
{
    public async Task<Result> Handle(RefundPaymentsForEventCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        Event? @event = await eventRepository.GetAsync(request.EventId, cancellationToken);
        if (@event is null)
        {
            return Result.Failure(EventErrors.NotFound(request.EventId));
        }

        IEnumerable<Payment> payments = await paymentRepository.GetForEventAsync(@event, cancellationToken);
        foreach (Payment payment in payments)
        {
            if (payment.Status != PaymentStatus.Succeeded)
            {
                continue;
            }

            Result refundResult = payment.Refund(payment.Amount - (payment.AmountRefunded ?? decimal.Zero));
            if (refundResult.IsFailure)
            {
                continue;
            }

            if (payment.IsFullyRefunded)
            {
                Order? order = await orderRepository.GetAsync(payment.OrderId, cancellationToken);

                if (order is not null)
                {
                    _ = order.Refund();
                }
            }
        }

        @event.PaymentsRefunded();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
