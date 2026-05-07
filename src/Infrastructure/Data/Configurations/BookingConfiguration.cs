using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.BookingId);
        builder.Property(b => b.TenantId).IsRequired();
        builder.Property(b => b.ResourceId).IsRequired();
        builder.Property(b => b.UserId).IsRequired();
        builder.Property(b => b.StartTime).IsRequired();
        builder.Property(b => b.EndTime).IsRequired();
        builder.Property(b => b.Status).IsRequired().HasMaxLength(50);
        builder.HasIndex(b => b.TenantId);
    }
}
