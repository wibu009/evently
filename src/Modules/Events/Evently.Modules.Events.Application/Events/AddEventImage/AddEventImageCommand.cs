using Evently.Common.Application.Messaging;

#pragma warning disable CA1054 // Image URLs are transported as validated strings by design (JSON DTOs, validated as absolute http(s) at the boundary).


namespace Evently.Modules.Events.Application.Events.AddEventImage;

public sealed record AddEventImageCommand(Guid EventId, string ImageUrl, bool SetAsCover) : ICommand<Guid>;
