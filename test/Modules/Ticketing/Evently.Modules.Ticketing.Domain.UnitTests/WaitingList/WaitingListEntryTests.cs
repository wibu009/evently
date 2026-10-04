using Evently.Common.Domain;
using Evently.Modules.Ticketing.Domain.UnitTests.Abstractions;
using Evently.Modules.Ticketing.Domain.WaitingList;
using FluentAssertions;

namespace Evently.Modules.Ticketing.Domain.UnitTests.WaitingList;

public class WaitingListEntryTests : BaseTest
{
    [Fact]
    public void Create_ShouldStartInWaitingStatus_AndRaiseDomainEvent()
    {
        // Arrange
        var ticketTypeId = Guid.CreateVersion7();
        var customerId = Guid.CreateVersion7();

        // Act
        Result<WaitingListEntry> result = WaitingListEntry.Create(ticketTypeId, customerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WaitingListEntryStatus.Waiting);
        result.Value.NotifiedAtUtc.Should().BeNull();

        CustomerJoinedWaitingListDomainEvent domainEvent =
            AssertDomainEventWasPublished<CustomerJoinedWaitingListDomainEvent>(result.Value);

        domainEvent.TicketTypeId.Should().Be(ticketTypeId);
        domainEvent.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public void MarkNotified_ShouldSetStatusAndTimestamp_WhenEntryIsWaiting()
    {
        // Arrange
        WaitingListEntry entry = WaitingListEntry.Create(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;

        // Act
        Result result = entry.MarkNotified();

        // Assert
        result.IsSuccess.Should().BeTrue();
        entry.Status.Should().Be(WaitingListEntryStatus.Notified);
        entry.NotifiedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkNotified_ShouldReturnFailure_WhenEntryWasAlreadyNotified()
    {
        // Arrange
        WaitingListEntry entry = WaitingListEntry.Create(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;
        entry.MarkNotified();

        // Act
        Result result = entry.MarkNotified();

        // Assert
        result.Error.Should().Be(WaitingListErrors.AlreadyNotified);
    }
}
