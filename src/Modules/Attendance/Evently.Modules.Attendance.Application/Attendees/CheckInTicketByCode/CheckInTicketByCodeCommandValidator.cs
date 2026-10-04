using FluentValidation;

namespace Evently.Modules.Attendance.Application.Attendees.CheckInTicketByCode;

internal sealed class CheckInTicketByCodeCommandValidator : AbstractValidator<CheckInTicketByCodeCommand>
{
    public CheckInTicketByCodeCommandValidator()
    {
        RuleFor(x => x.TicketCode).NotEmpty().MaximumLength(30);
    }
}
