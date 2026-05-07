using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Infrastructure.Data.Configurations;

public class StateTransitionConfiguration : IEntityTypeConfiguration<StateTransition>
{
    public void Configure(EntityTypeBuilder<StateTransition> builder)
    {
        builder.ToTable("StateTransitions");
        builder.HasKey(st => st.TransitionId);
        builder.Property(st => st.TenantId).IsRequired();
        builder.Property(st => st.FromStateId).IsRequired();
        builder.Property(st => st.ToStateId).IsRequired();
        builder.Property(st => st.RequiredRole).HasMaxLength(50);
        builder.HasIndex(st => st.TenantId);
    }
}
