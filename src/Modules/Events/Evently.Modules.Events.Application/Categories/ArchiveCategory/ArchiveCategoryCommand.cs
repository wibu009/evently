using Evently.Modules.Events.Application.Abstractions.Messaging;

namespace Evently.Modules.Events.Application.Categories.ArchiveCategory;

public sealed record ArchiveCategoryCommand(Guid CategoryId) : ICommand;
