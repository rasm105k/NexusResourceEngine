using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Infrastructure.Data.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resources");
        builder.HasKey(r => r.ResourceId);
        builder.Property(r => r.TenantId).IsRequired();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).HasMaxLength(4000);
        builder.Property(r => r.CurrentStateId).IsRequired();
        builder.Property(r => r.Latitude).HasColumnType("decimal(18,6)");
        builder.Property(r => r.Longitude).HasColumnType("decimal(18,6)");
        builder.Property(r => r.Metadata).HasMaxLength(4000);
        builder.HasIndex(r => r.TenantId);
    }
}
