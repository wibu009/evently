using FluentValidation;

namespace Evently.Modules.Ticketing.Application.Tickets.ArchiveOrderTickets;

internal sealed class ArchiveOrderTicketsCommandValidator : AbstractValidator<ArchiveOrderTicketsCommand>
{
    public ArchiveOrderTicketsCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
