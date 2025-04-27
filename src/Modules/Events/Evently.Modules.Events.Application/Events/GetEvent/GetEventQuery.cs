using Evently.Modules.Events.Application.Abstractions.Messaging;
using MediatR;

namespace Evently.Modules.Events.Application.Events.GetEvent;

public sealed record GetEventQuery(Guid EventId) : IQuery<EventResponse>;
