using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Platform.Core.Tenancy;
using Platform.Infrastructure.MasterDb;
using System.Text.Json;

namespace Platform.Infrastructure.Tenancy;

public class HttpTenantResolver : ITenantResolver
{
    private readonly MasterDbContext _dbContext;

    public HttpTenantResolver(MasterDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TenantInfo?> ResolveAsync(object context)
    {
        if (context is HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue("X-Tenant-Code", out var tenantCode))
            {
                var entity = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == tenantCode.ToString());
                if (entity != null)
                {
                    return new TenantInfo
                    {
                        Id = entity.Id,
                        Name = entity.Name,
                        ConnectionString = entity.ConnectionString,
                        EnabledModules = string.IsNullOrEmpty(entity.EnabledModulesJson) 
                            ? new List<string>() 
                            : JsonSerializer.Deserialize<List<string>>(entity.EnabledModulesJson) ?? new List<string>()
                    };
                }
            }
        }
        return null;
    }
}
