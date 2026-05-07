using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Domain;

namespace NexusResourceEngine.Infrastructure.Data;

public class NexusResourceEngineContext : DbContext
{
    public NexusResourceEngineContext(DbContextOptions<NexusResourceEngineContext> options)
        : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ResourceState> ResourceStates => Set<ResourceState>();
    public DbSet<StateTransition> StateTransitions => Set<StateTransition>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexusResourceEngineContext).Assembly);

        modelBuilder.Entity<Tenant>().HasQueryFilter(t => t.TenantId != Guid.Empty);
        modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId != Guid.Empty);
        modelBuilder.Entity<ResourceState>().HasQueryFilter(rs => rs.TenantId != Guid.Empty);
        modelBuilder.Entity<StateTransition>().HasQueryFilter(st => st.TenantId != Guid.Empty);
        modelBuilder.Entity<Resource>().HasQueryFilter(r => r.TenantId != Guid.Empty);
        modelBuilder.Entity<Booking>().HasQueryFilter(b => b.TenantId != Guid.Empty);
    }
}
