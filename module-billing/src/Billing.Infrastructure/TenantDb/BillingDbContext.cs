using Microsoft.EntityFrameworkCore;
using Platform.Core.Tenancy;
using Billing.Domain.Entities;

namespace Billing.Infrastructure.TenantDb;

public class BillingDbContext : DbContext
{
    private readonly ITenantAccessor _tenantAccessor;

    public BillingDbContext(DbContextOptions<BillingDbContext> options, ITenantAccessor tenantAccessor) : base(options)
    {
        _tenantAccessor = tenantAccessor;
    }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var tenant = _tenantAccessor.CurrentTenant;
        if (tenant != null && !string.IsNullOrEmpty(tenant.ConnectionString))
        {
            optionsBuilder.UseSqlite(tenant.ConnectionString);
        }
        else
        {
            throw new InvalidOperationException("Tenant context is not resolved or connection string is missing. Cannot configure BillingDbContext.");
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>().HasKey(x => x.Id);
        modelBuilder.Entity<Payment>().HasKey(x => x.Id);
    }
}
