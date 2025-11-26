namespace Platform.Shared.Contracts.Docs;

public class DocDto
{
    public string Id { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
