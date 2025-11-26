namespace Platform.Core.DocTypes;

public class DocFieldDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string"; // string, number, date, etc.
    public bool IsRequired { get; set; }
}
