namespace Platform.Core.DocTypes;

public class DocTypeDefinition
{
    public string Name { get; set; } = string.Empty; // e.g., "Invoice"
    public string Module { get; set; } = string.Empty; // e.g., "Billing"
    public List<DocFieldDefinition> Fields { get; set; } = new();
}
