using FluentValidation;

namespace Evently.Modules.Ticketing.Application.WaitingList.NotifyWaitingList;

internal sealed class NotifyWaitingListCommandValidator : AbstractValidator<NotifyWaitingListCommand>
{
    public NotifyWaitingListCommandValidator()
    {
        RuleFor(x => x.TicketTypeId).NotEmpty();

        RuleFor(x => x.Quantity).GreaterThan(decimal.Zero);
    }
}
