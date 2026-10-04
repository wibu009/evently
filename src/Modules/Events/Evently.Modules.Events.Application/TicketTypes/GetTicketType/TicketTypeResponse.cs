#pragma warning disable CA1054 // Image URLs are transported as validated strings by design (JSON DTOs, validated as absolute http(s) at the boundary).

namespace Evently.Modules.Events.Application.TicketTypes.GetTicketType;

public sealed record TicketTypeResponse(
    Guid Id,
    Guid EventId,
    string Name,
    decimal Price,
    string Currency,
    decimal Quantity,
    string? Color,
    string? BackgroundImageUrl);

