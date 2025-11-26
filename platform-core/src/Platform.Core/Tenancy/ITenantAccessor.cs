namespace Platform.Core.Tenancy;

public interface ITenantAccessor
{
    TenantInfo? CurrentTenant { get; }
}
