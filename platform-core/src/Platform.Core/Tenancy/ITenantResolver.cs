namespace Platform.Core.Tenancy;

public interface ITenantResolver
{
    Task<TenantInfo?> ResolveAsync(object context); // context could be HttpContext or generic
}
