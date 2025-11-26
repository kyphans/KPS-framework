namespace Platform.Core.Tenancy;

public class TenantInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public List<string> EnabledModules { get; set; } = new();
}
