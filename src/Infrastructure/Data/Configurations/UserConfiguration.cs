using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.TenantId).IsRequired();
        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.Role).IsRequired().HasMaxLength(50);
        builder.Property(u => u.InviteToken).HasMaxLength(200);
        builder.Property(u => u.InviteTokenExpiresAt);
        builder.Property(u => u.IsActive).HasDefaultValue(false);
        builder.HasIndex(u => u.InviteToken).IsUnique().HasFilter("[InviteToken] IS NOT NULL");
        builder.HasIndex(u => u.TenantId);
    }
}
