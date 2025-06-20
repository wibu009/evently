﻿using Evently.Common.Application.Messaging;

namespace Evently.Modules.Ticketing.Application.Events.CreateEvent;

public sealed record CreateEventCommand(
    Guid EventId,
    string Title,
    string Description,
    string Location,
    DateTime StartAtUtc,
    DateTime? EndAtUtc,
    IEnumerable<TicketTypeRequest> TicketTypes) : ICommand;
