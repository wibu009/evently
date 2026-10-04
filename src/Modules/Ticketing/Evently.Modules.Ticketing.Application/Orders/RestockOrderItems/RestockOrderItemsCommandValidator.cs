using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Orders.RestockOrderItems;

internal sealed class RestockOrderItemsCommandValidator : AbstractValidator<RestockOrderItemsCommand>
{
    public RestockOrderItemsCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
