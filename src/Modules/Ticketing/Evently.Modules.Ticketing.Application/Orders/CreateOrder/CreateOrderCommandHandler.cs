using System.Data.Common;
using Evently.Common.Application.Clock;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Evently.Modules.Ticketing.Domain.PromoCodes;
using Microsoft.Extensions.Options;

namespace Evently.Modules.Ticketing.Application.Orders.CreateOrder;

/// <summary>
/// The first step of the order fulfillment saga:
/// <list type="bullet">
/// <item>Reserves the ticket inventory transactionally (pessimistic row locks prevent overselling).</item>
/// <item>Applies an optional promo code and creates the order in the <see cref="OrderStatus.Pending"/> state with a payment deadline.</item>
/// <item>Creates the payment in the <see cref="PaymentStatus.Pending"/> state for the net amount.</item>
/// </list>
/// The charge itself is processed asynchronously through the outbox
/// (<see cref="ProcessPayment.ProcessPaymentCommandHandler"/>); if it fails or the payment
/// deadline passes, the reserved inventory is released as compensation.
/// </summary>
internal sealed class CreateOrderCommandHandler(
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    ITicketTypeRepository ticketTypeRepository,
    IPaymentRepository paymentRepository,
    IPromoCodeRepository promoCodeRepository,
    CartService cartService,
    IDateTimeProvider dateTimeProvider,
    IOptions<OrdersOptions> ordersOptions,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        Customer? customer = await customerRepository.GetAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure<Guid>(CustomerErrors.NotFound(request.CustomerId));
        }

        Cart cart = await cartService.GetAsync(request.CustomerId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            return Result.Failure<Guid>(CartErrors.Empty);
        }

        PromoCode? promoCode = null;
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            promoCode = await promoCodeRepository.GetByCodeAsync(request.PromoCode.Trim(), cancellationToken);
            if (promoCode is null)
            {
                return Result.Failure<Guid>(PromoCodeErrors.NotFoundByCode(request.PromoCode.Trim()));
            }
        }

        DateTime paymentDueUtc = dateTimeProvider.UtcNow.AddMinutes(ordersOptions.Value.PaymentTimeoutMinutes);

        var order = Order.Create(customer, paymentDueUtc);

        foreach (CartItem cartItem in cart.Items)
        {
            // This acquires a pessimistic lock or throws an exception if already locked
            TicketType? ticketType = await ticketTypeRepository.GetWithLockAsync(cartItem.TicketTypeId, cancellationToken);
            if (ticketType is null)
            {
                return Result.Failure<Guid>(TicketTypeErrors.NotFound(cartItem.TicketTypeId));
            }

            Result result = ticketType.UpdateQuantity(cartItem.Quantity);
            if (result.IsFailure)
            {
                return Result.Failure<Guid>(result.Error);
            }

            order.AddItem(ticketType, cartItem.Quantity, ticketType.Price, ticketType.Currency);
        }

        if (promoCode is not null)
        {
            if (promoCode.Currency != order.Currency)
            {
                return Result.Failure<Guid>(PromoCodeErrors.CurrencyMismatch(promoCode.Currency, order.Currency));
            }

            Result<decimal> discountResult = promoCode.CalculateDiscount(order.TotalPrice, dateTimeProvider.UtcNow);
            if (discountResult.IsFailure)
            {
                return Result.Failure<Guid>(discountResult.Error);
            }

            Result applyResult = order.ApplyPromoCode(promoCode.Id, discountResult.Value);
            if (applyResult.IsFailure)
            {
                return Result.Failure<Guid>(applyResult.Error);
            }
        }

        orderRepository.Insert(order);

        var payment = Payment.Create(order, order.NetPrice, order.Currency);

        paymentRepository.Insert(payment);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        await cartService.ClearAsync(request.CustomerId, cancellationToken);

        return order.Id;
    }
}
