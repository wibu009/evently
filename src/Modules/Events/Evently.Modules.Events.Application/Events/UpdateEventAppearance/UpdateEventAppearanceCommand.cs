using Evently.Common.Application.Messaging;

#pragma warning disable CA1054 // Image URLs are transported as validated strings by design (JSON DTOs, validated as absolute http(s) at the boundary).


namespace Evently.Modules.Events.Application.Events.UpdateEventAppearance;

public sealed record UpdateEventAppearanceCommand(Guid EventId, string? HeroBannerUrl, string? AccentColor) : ICommand;
