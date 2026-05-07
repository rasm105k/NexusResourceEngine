using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Infrastructure.Data.Configurations;

public class ResourceStateConfiguration : IEntityTypeConfiguration<ResourceState>
{
    public void Configure(EntityTypeBuilder<ResourceState> builder)
    {
        builder.ToTable("ResourceStates");
        builder.HasKey(rs => rs.StateId);
        builder.Property(rs => rs.TenantId).IsRequired();
        builder.Property(rs => rs.Name).IsRequired().HasMaxLength(100);
        builder.Property(rs => rs.IsBookable).IsRequired();
        builder.Property(rs => rs.ColorCode).HasMaxLength(50);
        builder.Property(rs => rs.SortOrder).IsRequired();
        builder.HasIndex(rs => rs.TenantId);
    }
}
