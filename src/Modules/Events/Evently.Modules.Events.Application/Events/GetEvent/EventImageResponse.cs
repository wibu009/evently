#pragma warning disable CA1054 // Image URLs are transported as validated strings by design (JSON DTOs, validated as absolute http(s) at the boundary).

namespace Evently.Modules.Events.Application.Events.GetEvent;

public sealed record EventImageResponse(
    Guid ImageId,
    string ImageUrl,
    bool IsCover,
    int DisplayOrder);

