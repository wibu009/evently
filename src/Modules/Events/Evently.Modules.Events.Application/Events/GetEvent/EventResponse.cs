#pragma warning disable CA1054 // Image URLs are transported as validated strings by design (JSON DTOs, validated as absolute http(s) at the boundary).

namespace Evently.Modules.Events.Application.Events.GetEvent;

public sealed record EventResponse(
    Guid Id,
    Guid CategoryId,
    string Title,
    string Description,
    string Location,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    string Status,
    string? HeroBannerUrl,
    string? AccentColor)
{
    public List<TicketTypeResponse> TicketTypes { get; } = [];

    public List<EventImageResponse> Images { get; } = [];
}

