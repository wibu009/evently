using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Attendance.Application.Abstractions.Data;
using Evently.Modules.Attendance.Domain.Attendees;
using Evently.Modules.Attendance.Domain.Tickets;
using Microsoft.Extensions.Logging;

namespace Evently.Modules.Attendance.Application.Attendees.CheckInTicketByCode;

/// <summary>
/// Gate check-in by QR code. Architected for crowd throughput:
/// <list type="bullet">
/// <item>A single indexed lookup by the unique ticket code (no joins, no scans).</item>
/// <item>The attendee is resolved from the ticket itself, so staff can scan any ticket.</item>
/// <item>Duplicate and invalid scans are explicit outcomes persisted as domain events
/// (feeding the statistics projections) — scanners never need to retry.</item>
/// <item>One <c>SaveChanges</c> per scan keeps each gate transaction short.</item>
/// </list>
/// </summary>
internal sealed class CheckInTicketByCodeCommandHandler(
    ITicketRepository ticketRepository,
    IAttendeeRepository attendeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<CheckInTicketByCodeCommandHandler> logger)
    : ICommandHandler<CheckInTicketByCodeCommand, CheckInTicketByCodeResponse>
{
    public async Task<Result<CheckInTicketByCodeResponse>> Handle(
        CheckInTicketByCodeCommand request,
        CancellationToken cancellationToken)
    {
        Ticket? ticket = await ticketRepository.GetByCodeAsync(request.TicketCode.Trim(), cancellationToken);
        if (ticket is null)
        {
            return new CheckInTicketByCodeResponse("NotFound", null, null);
        }

        Attendee? attendee = await attendeeRepository.GetAsync(ticket.AttendeeId, cancellationToken);
        if (attendee is null)
        {
            logger.LogWarning(
                "Check in failed: ticket {TicketId} belongs to unknown attendee {AttendeeId}",
                ticket.Id,
                ticket.AttendeeId);

            return new CheckInTicketByCodeResponse("NotFound", null, ticket.EventId);
        }

        Result result = attendee.CheckIn(ticket);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (result.IsFailure)
        {
            string outcome = result.Error == TicketErrors.DuplicateCheckIn ? "Duplicate" : "Invalid";

            logger.LogWarning(
                "Check in {Outcome}: attendee {AttendeeId}, ticket {TicketId}",
                outcome,
                attendee.Id,
                ticket.Id);

            return new CheckInTicketByCodeResponse(outcome, attendee.Id, ticket.EventId);
        }

        return new CheckInTicketByCodeResponse("CheckedIn", attendee.Id, ticket.EventId);
    }
}
