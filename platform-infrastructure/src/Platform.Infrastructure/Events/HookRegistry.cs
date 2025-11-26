using Microsoft.EntityFrameworkCore;
using Platform.Core.Events;
using Platform.Core.Tenancy;
using Platform.Infrastructure.MasterDb;

namespace Platform.Infrastructure.Events;

public class HookRegistry
{
    private readonly MasterDbContext _masterDbContext;
    private readonly ITenantAccessor _tenantAccessor;

    public HookRegistry(MasterDbContext masterDbContext, ITenantAccessor tenantAccessor)
    {
        _masterDbContext = masterDbContext;
        _tenantAccessor = tenantAccessor;
    }

    public async Task<IEnumerable<HookDefinition>> GetHooksForEventAsync(string eventName)
    {
        var tenantId = _tenantAccessor.CurrentTenant?.Id;
        
        // Fetch global hooks (TenantId is empty) + Tenant specific hooks
        var hooks = await _masterDbContext.HookConfigs
            .Where(h => h.EventName == eventName && (h.TenantId == "" || h.TenantId == tenantId))
            .ToListAsync();

        return hooks.Select(h => new HookDefinition
        {
            EventName = h.EventName,
            HandlerModule = h.HandlerModule,
            HandlerAction = h.HandlerAction
        });
    }
}
