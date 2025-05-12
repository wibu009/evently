using System.Data.Common;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Ticketing.Application.Abstractions.Data;
using Evently.Modules.Ticketing.Application.Abstractions.Payments;
using Evently.Modules.Ticketing.Application.Carts;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(Guid CustomerId) : ICommand;

internal sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}

internal sealed class CreateOrderCommandHandler(
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository,
    ITicketTypeRepository ticketTypeRepository,
    IPaymentRepository paymentRepository,
    IPaymentService paymentService,
    CartService cartService,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateOrderCommand>
{
    public async Task<Result> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        
        Customer? customer = await customerRepository.GetAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(request.CustomerId));
        }
        
        var order = Order.Create(customer);
        
        Cart cart = await cartService.GetAsync(request.CustomerId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            return Result.Failure(CartErrors.Empty);
        }

        foreach (CartItem cartItem in cart.Items)
        {
            // This acquires a pessimistic lock or throws an exception if already locked
            TicketType? ticketType = await ticketTypeRepository.GetWithLockAsync(cartItem.TicketTypeId, cancellationToken);
            if (ticketType is null)
            {
                return Result.Failure(TicketTypeErrors.NotFound(cartItem.TicketTypeId));
            }

            Result result = ticketType.UpdateQuantity(cartItem.Quantity);
            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }
            
            order.AddItem(ticketType, cartItem.Quantity, ticketType.Price, ticketType.Currency);
        }
        
        orderRepository.Insert(order);
        
        //We're faking a payment gateway request here
        PaymentResponse paymentResponse = await paymentService.ChargeAsync(order.TotalPrice, order.Currency);
        
        var payment = Payment.Create(order, paymentResponse.TransactionId, paymentResponse.Amount, paymentResponse.Currency);
        
        paymentRepository.Insert(payment);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        await transaction.CommitAsync(cancellationToken);
        
        await cartService.ClearAsync(request.CustomerId, cancellationToken);
        
        return Result.Success();
    }
}
