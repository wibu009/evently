using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Tickets.TransferTicket;

internal sealed class TransferTicketCommandValidator : AbstractValidator<TransferTicketCommand>
{
    public TransferTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();

        RuleFor(x => x.ToCustomerId).NotEmpty();
    }
}
