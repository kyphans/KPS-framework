namespace Platform.Infrastructure.Docs;

public class DocEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Module { get; set; } = string.Empty;
    public string DocType { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
