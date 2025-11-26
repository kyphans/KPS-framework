using Microsoft.EntityFrameworkCore;
using Platform.Core.Tenancy;
using User.Domain.Entities;

namespace User.Infrastructure.TenantDb;

public class UserDbContext : DbContext
{
    private readonly ITenantAccessor _tenantAccessor;

    public UserDbContext(DbContextOptions<UserDbContext> options, ITenantAccessor tenantAccessor) : base(options)
    {
        _tenantAccessor = tenantAccessor;
    }

    public DbSet<Domain.Entities.User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var tenant = _tenantAccessor.CurrentTenant;
        if (tenant != null && !string.IsNullOrEmpty(tenant.ConnectionString))
        {
            optionsBuilder.UseSqlite(tenant.ConnectionString);
        }
        else
        {
            throw new InvalidOperationException("Tenant context is not resolved or connection string is missing. Cannot configure UserDbContext.");
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.User>().HasKey(x => x.Id);
        modelBuilder.Entity<Domain.Entities.User>().Property(x => x.Username).IsRequired().HasMaxLength(100);
        modelBuilder.Entity<Domain.Entities.User>().Property(x => x.Email).IsRequired().HasMaxLength(200);
    }
}
