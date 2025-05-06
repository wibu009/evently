using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.PublicApi;
using Evently.Modules.Ticketing.Domain.Customers;
using Evently.Modules.Ticketing.Domain.Events;
using Evently.Modules.Users.PublicApi;

namespace Evently.Modules.Ticketing.Application.Carts.AddItemToCart;

internal sealed class AddItemToCartCommandHandler(
    CartService cartService,
    IUsersApi usersApi,
    IEventsApi eventsApi)
    : ICommandHandler<AddItemToCartCommand>
{
    public async Task<Result> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        // 1. Get customer
        UserResponse? customer = await usersApi.GetAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result.Failure(CustomerErrors.NotFound(request.CustomerId));
        }
        
        // 2. Get ticket type
        TicketTypeResponse? ticketType = await eventsApi.GetTicketTypeAsync(request.TicketTypeId, cancellationToken);
        if (ticketType is null)
        {
            return Result.Failure(TicketTypeErrors.NotFound(request.TicketTypeId));
        }
        
        // 3. Add item to cart
        var cartItem = new CartItem
        {
            TicketTypeId = ticketType.Id,
            Price = ticketType.Price,
            Quantity = request.Quantity,
            Currency = ticketType.Currency
        };
        await cartService.AddItemAsync(customer.Id, cartItem, cancellationToken);
        
        return Result.Success();
    }
}