using Evently.Modules.Ticketing.Domain.PromoCodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Evently.Modules.Ticketing.Infrastructure.PromoCodes;

internal sealed class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code).HasMaxLength(50);

        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Currency).HasMaxLength(3);
    }
}
