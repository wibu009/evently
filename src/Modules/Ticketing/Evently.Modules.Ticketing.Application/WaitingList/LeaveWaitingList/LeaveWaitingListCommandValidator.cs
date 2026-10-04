using FluentValidation;

namespace Evently.Modules.Ticketing.Application.WaitingList.LeaveWaitingList;

internal sealed class LeaveWaitingListCommandValidator : AbstractValidator<LeaveWaitingListCommand>
{
    public LeaveWaitingListCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.TicketTypeId).NotEmpty();
    }
}
