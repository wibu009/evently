using Evently.Modules.Ticketing.Domain.WaitingList;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Evently.Modules.Ticketing.Infrastructure.WaitingList;

internal sealed class WaitingListEntryConfiguration : IEntityTypeConfiguration<WaitingListEntry>
{
    public void Configure(EntityTypeBuilder<WaitingListEntry> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.TicketTypeId, e.CustomerId }).IsUnique();

        builder.HasIndex(e => new { e.TicketTypeId, e.Status });
    }
}
