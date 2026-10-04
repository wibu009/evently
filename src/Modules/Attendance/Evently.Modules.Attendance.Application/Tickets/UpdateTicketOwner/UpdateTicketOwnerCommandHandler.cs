using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Abstractions.Data;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.Domain.Tickets;

namespace Evently.Modules.Attendance.Application.Tickets.UpdateTicketOwner;

/// <summary>
/// Updates the owner of a ticket after it was transferred in the Ticketing module,
/// so check-in validates against the new owner.
/// </summary>
internal sealed class UpdateTicketOwnerCommandHandler(
    ITicketRepository ticketRepository,
    IAttendeeRepository attendeeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTicketOwnerCommand>
{
    public async Task<Result> Handle(UpdateTicketOwnerCommand request, CancellationToken cancellationToken)
    {
        Ticket? ticket = await ticketRepository.GetAsync(request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(TicketErrors.NotFound);
        }

        Attendee? attendee = await attendeeRepository.GetAsync(request.AttendeeId, cancellationToken);
        if (attendee is null)
        {
            return Result.Failure(AttendeeErrors.NotFound(request.AttendeeId));
        }

        Result result = ticket.TransferTo(request.AttendeeId);
        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
