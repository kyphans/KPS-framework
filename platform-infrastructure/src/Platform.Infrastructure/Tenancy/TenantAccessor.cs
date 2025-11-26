using Microsoft.AspNetCore.Http;
using Platform.Core.Tenancy;

namespace Platform.Infrastructure.Tenancy;

public class TenantAccessor : ITenantAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TenantInfo? CurrentTenant
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.Items.TryGetValue("Tenant", out var tenant) == true)
            {
                return tenant as TenantInfo;
            }
            return null;
        }
    }
}
