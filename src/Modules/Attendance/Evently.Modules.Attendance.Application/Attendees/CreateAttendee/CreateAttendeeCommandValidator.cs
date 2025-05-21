using FluentValidation;

namespace Evently.Modules.Attendance.Application.Attendees.CreateAttendee;

internal sealed class CreateAttendeeCommandValidator : AbstractValidator<CreateAttendeeCommand>
{
    public CreateAttendeeCommandValidator()
    {
        RuleFor(x => x.AttendeeId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}