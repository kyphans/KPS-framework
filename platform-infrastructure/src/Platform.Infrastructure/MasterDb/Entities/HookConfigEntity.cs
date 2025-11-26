namespace Platform.Infrastructure.MasterDb.Entities;

public class HookConfigEntity
{
    public int Id { get; set; }
    public string TenantId { get; set; } = string.Empty; // Empty for global hooks
    public string EventName { get; set; } = string.Empty;
    public string HandlerModule { get; set; } = string.Empty;
    public string HandlerAction { get; set; } = string.Empty;
}
