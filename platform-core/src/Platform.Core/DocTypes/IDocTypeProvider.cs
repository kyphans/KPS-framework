namespace Platform.Core.DocTypes;

public interface IDocTypeProvider
{
    Task<DocTypeDefinition?> GetDocTypeAsync(string module, string docTypeName);
    Task<IEnumerable<DocTypeDefinition>> GetAllDocTypesAsync();
}
