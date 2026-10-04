using Evently.Common.Application.Messaging;

#pragma warning disable CA1054 // Image URLs are transported as validated strings by design (JSON DTOs, validated as absolute http(s) at the boundary).


namespace Evently.Modules.Events.Application.TicketTypes.UpdateTicketTypeDesign;

public sealed record UpdateTicketTypeDesignCommand(Guid TicketTypeId, string? Color, string? BackgroundImageUrl) : ICommand;
