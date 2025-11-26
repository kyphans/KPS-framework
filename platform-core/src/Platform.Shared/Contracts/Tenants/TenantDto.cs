namespace Platform.Shared.Contracts.Tenants;

public class TenantDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> EnabledModules { get; set; } = new();
}
