using Platform.Core.DocTypes;
using System.Collections.Concurrent;

namespace Platform.Infrastructure.DocTypes;

public class MasterDocTypeProvider : IDocTypeProvider
{
    // Temporary in-memory storage. In real app, read from DB or cache.
    private static readonly ConcurrentDictionary<string, DocTypeDefinition> _docTypes = new();

    public Task<DocTypeDefinition?> GetDocTypeAsync(string module, string docTypeName)
    {
        var key = $"{module.ToLower()}:{docTypeName.ToLower()}";
        _docTypes.TryGetValue(key, out var definition);
        return Task.FromResult(definition);
    }

    public Task<IEnumerable<DocTypeDefinition>> GetAllDocTypesAsync()
    {
        return Task.FromResult(_docTypes.Values.AsEnumerable());
    }

    public static void Register(DocTypeDefinition definition)
    {
        var key = $"{definition.Module.ToLower()}:{definition.Name.ToLower()}";
        _docTypes[key] = definition;
    }
}
