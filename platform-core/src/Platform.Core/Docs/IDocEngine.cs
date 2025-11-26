using Platform.Core.Common;
using Platform.Shared.Contracts.Docs;

namespace Platform.Core.Docs;

public interface IDocEngine
{
    Task<Result<DocDto>> CreateAsync(string module, string docType, Dictionary<string, object> data);
    Task<Result<DocDto>> GetAsync(string id);
    // Add Update/Delete later
}
