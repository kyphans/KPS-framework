namespace Platform.Core.Events;

public class HookDefinition
{
    public string EventName { get; set; } = string.Empty;
    public string HandlerModule { get; set; } = string.Empty;
    public string HandlerAction { get; set; } = string.Empty; // e.g., URL or service key
}
