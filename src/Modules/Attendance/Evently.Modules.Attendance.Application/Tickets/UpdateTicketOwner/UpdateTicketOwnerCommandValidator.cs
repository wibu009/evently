using FluentValidation;

namespace Evently.Modules.Attendance.Application.Tickets.UpdateTicketOwner;

internal sealed class UpdateTicketOwnerCommandValidator : AbstractValidator<UpdateTicketOwnerCommand>
{
    public UpdateTicketOwnerCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();

        RuleFor(x => x.AttendeeId).NotEmpty();
    }
}
