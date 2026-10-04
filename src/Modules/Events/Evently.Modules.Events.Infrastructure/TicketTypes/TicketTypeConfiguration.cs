using Evently.Modules.Events.Domain.Events;
using Evently.Modules.Events.Domain.TicketTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Evently.Modules.Events.Infrastructure.TicketTypes;

internal sealed class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.HasOne<Event>()
            .WithMany()
            .HasForeignKey(t => t.EventId);

        builder.Property(t => t.Color).HasMaxLength(7).IsRequired(false);

        builder.Property(t => t.BackgroundImageUrl).HasMaxLength(500).IsRequired(false);
    }
}
