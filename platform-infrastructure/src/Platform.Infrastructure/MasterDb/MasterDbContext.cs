using Microsoft.EntityFrameworkCore;
using Platform.Infrastructure.MasterDb.Entities;

namespace Platform.Infrastructure.MasterDb;

public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<TenantEntity> Tenants { get; set; }
    public DbSet<HookConfigEntity> HookConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantEntity>().HasKey(x => x.Id);
        modelBuilder.Entity<HookConfigEntity>().HasKey(x => x.Id);
    }
}
