using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Infrastructure.Data.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.TenantId);
        builder.Property(t => t.OrganizationName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.CreatedAt).IsRequired();
    }
}
