using Evently.Modules.Events.Domain.Categories;
using Evently.Modules.Events.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Evently.Modules.Events.Infrastructure.Events;

internal sealed class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(e => e.CategoryId);

        builder.Property(e => e.HeroBannerUrl).HasMaxLength(500).IsRequired(false);

        builder.Property(e => e.AccentColor).HasMaxLength(7).IsRequired(false);

        builder.HasMany(e => e.Images)
            .WithOne()
            .HasForeignKey(i => i.EventId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
