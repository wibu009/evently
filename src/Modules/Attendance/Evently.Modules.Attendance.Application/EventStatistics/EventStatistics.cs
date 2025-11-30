namespace Evently.Modules.Attendance.Application.EventStatistics;

public sealed class EventStatistics
{
    private EventStatistics() { }   
    
    public Guid EventId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public int TicketsSold { get; set; }
    public int AttendeesCheckedIn { get; set; }
    public List<TicketModel> DuplicateCheckInTickets { get; set; }
    public List<TicketModel> InvalidCheckInTickets { get; set; }
    
    public static EventStatistics Create(
        Guid id,
        string title,
        string description,
        string location,
        DateTime startsAtUtc,
        DateTime? endsAtUtc)
    {
        var @event = new EventStatistics
        {
            EventId = id,
            Title = title,
            Description = description,
            Location = location,
            StartAtUtc = startsAtUtc,
            EndAtUtc = endsAtUtc
        };

        return @event;
    }
}
