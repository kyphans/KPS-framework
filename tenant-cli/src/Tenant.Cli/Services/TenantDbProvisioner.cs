using Microsoft.EntityFrameworkCore;
using Platform.Core.Tenancy;
using Platform.Infrastructure.MasterDb;
using Platform.Infrastructure.MasterDb.Entities;
using Platform.Infrastructure.Tenancy;
using System.Text.Json;

namespace Tenant.Cli.Services;

public class TenantDbProvisioner
{
    private readonly MasterDbContext _dbContext;

    public TenantDbProvisioner(MasterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateTenantAsync(string code, string[] modules)
    {
        if (_dbContext.Tenants.Any(t => t.Id == code))
        {
            Console.WriteLine($"Tenant {code} already exists.");
            return;
        }

        // For simplicity, use a localdb connection string based on tenant code
        var connectionString = $"Data Source=Platform_Tenant_{code}.sqlite";

        var tenant = new TenantEntity
        {
            Id = code,
            Name = $"Tenant {code}",
            ConnectionString = connectionString,
            EnabledModulesJson = JsonSerializer.Serialize(modules)
        };

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"Tenant {code} created in MasterDb.");
        Console.WriteLine($"Connection String: {connectionString}");

        // Create actual DB
        Console.WriteLine("Provisioning Tenant DB...");
        var optionsBuilder = new DbContextOptionsBuilder<PlatformTenantDbContext>();
        optionsBuilder.UseSqlite(connectionString);
        
        var dummyAccessor = new DummyTenantAccessor(connectionString);
        using var tenantDb = new PlatformTenantDbContext(optionsBuilder.Options, dummyAccessor);
        tenantDb.Database.EnsureCreated();
        Console.WriteLine("Tenant DB Provisioned.");
    }

    private class DummyTenantAccessor : ITenantAccessor
    {
        public TenantInfo? CurrentTenant { get; }
        public DummyTenantAccessor(string connString)
        {
            CurrentTenant = new TenantInfo { ConnectionString = connString };
        }
    }
}
