using Microsoft.AspNetCore.Http;
using Platform.Core.Tenancy;

namespace Platform.Infrastructure.Tenancy;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        var tenant = await tenantResolver.ResolveAsync(context);
        if (tenant != null)
        {
            context.Items["Tenant"] = tenant;
        }
        
        await _next(context);
    }
}
