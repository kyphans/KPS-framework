namespace Platform.Infrastructure.MasterDb.Entities;

public class TenantEntity
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string EnabledModulesJson { get; set; } = "[]"; // Serialized list
}
