﻿using Evently.Common.Application.EventBus;

namespace Evently.Modules.Ticketing.IntegrationEvents.TicketTypes;

public sealed class TicketTypeSoldOutIntegrationEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid ticketTypeId) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid TicketTypeId { get; init; } = ticketTypeId;
}
