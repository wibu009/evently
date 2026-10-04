using FluentValidation;

namespace Evently.Modules.Ticketing.Application.WaitingList.JoinWaitingList;

internal sealed class JoinWaitingListCommandValidator : AbstractValidator<JoinWaitingListCommand>
{
    public JoinWaitingListCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.TicketTypeId).NotEmpty();
    }
}
