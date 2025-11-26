using Microsoft.EntityFrameworkCore;
using Platform.Core.Tenancy;
using Platform.Infrastructure.Docs;

namespace Platform.Infrastructure.Tenancy;

public class PlatformTenantDbContext : DbContext
{
    private readonly ITenantAccessor _tenantAccessor;

    public PlatformTenantDbContext(DbContextOptions<PlatformTenantDbContext> options, ITenantAccessor tenantAccessor) : base(options)
    {
        _tenantAccessor = tenantAccessor;
    }

    public DbSet<DocEntity> Docs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var tenant = _tenantAccessor.CurrentTenant;
        if (tenant != null && !string.IsNullOrEmpty(tenant.ConnectionString))
        {
            optionsBuilder.UseSqlServer(tenant.ConnectionString);
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocEntity>().HasKey(x => x.Id);
    }
}
