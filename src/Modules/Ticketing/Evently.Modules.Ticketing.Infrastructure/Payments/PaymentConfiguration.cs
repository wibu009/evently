using Evently.Modules.Ticketing.Domain.Orders;
using Evently.Modules.Ticketing.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Evently.Modules.Ticketing.Infrastructure.Payments;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne<Order>().WithMany().HasForeignKey(p => p.OrderId);

        builder.Property(p => p.TransactionReference).HasMaxLength(100).IsRequired(false);

        builder.Property(p => p.Currency).HasMaxLength(3);

        builder.Property(p => p.Status).IsRequired();

        builder.Property(p => p.FailureReason).HasMaxLength(500);

        // The gateway reference is only known after the gateway accepted the charge,
        // so pending payments have a null transaction reference.
        builder.HasIndex(p => p.TransactionReference).IsUnique();
    }
}
