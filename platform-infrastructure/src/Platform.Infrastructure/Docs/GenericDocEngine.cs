using Platform.Core.Common;
using Platform.Core.Docs;
using Platform.Core.DocTypes;
using Platform.Core.Events;
using Platform.Infrastructure.Docs;
using Platform.Infrastructure.Tenancy;
using Platform.Shared.Contracts.Docs;
using System.Text.Json;

namespace Platform.Infrastructure.Docs;

public class GenericDocEngine : IDocEngine
{
    private readonly PlatformTenantDbContext _dbContext;
    private readonly IDocTypeProvider _docTypeProvider;
    private readonly IEventBus _eventBus;

    public GenericDocEngine(PlatformTenantDbContext dbContext, IDocTypeProvider docTypeProvider, IEventBus eventBus)
    {
        _dbContext = dbContext;
        _docTypeProvider = docTypeProvider;
        _eventBus = eventBus;
    }

    public async Task<Result<DocDto>> CreateAsync(string module, string docType, Dictionary<string, object> data)
    {
        // 1. Get DocType Definition
        var definition = await _docTypeProvider.GetDocTypeAsync(module, docType);
        if (definition == null)
            return Result<DocDto>.Failure(Errors.DocTypeNotFound);

        // 2. Validate (Simple check required fields)
        foreach (var field in definition.Fields.Where(f => f.IsRequired))
        {
            if (!data.ContainsKey(field.Name) || data[field.Name] == null)
                return Result<DocDto>.Failure($"{Errors.ValidationFailed}: Missing required field {field.Name}");
        }

        // 3. Save
        var entity = new DocEntity
        {
            Module = module,
            DocType = docType,
            DataJson = JsonSerializer.Serialize(data),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "User" // TODO: Get from UserContext
        };

        _dbContext.Docs.Add(entity);
        await _dbContext.SaveChangesAsync();

        // 4. Publish Event (TODO)
        // await _eventBus.PublishAsync(new DocCreatedEvent(...));

        return Result<DocDto>.Success(new DocDto
        {
            Id = entity.Id,
            DocType = docType,
            Data = data,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        });
    }

    public async Task<Result<DocDto>> GetAsync(string id)
    {
        var entity = await _dbContext.Docs.FindAsync(id);
        if (entity == null)
            return Result<DocDto>.Failure("DocNotFound");

        return Result<DocDto>.Success(new DocDto
        {
            Id = entity.Id,
            DocType = entity.DocType,
            Data = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.DataJson) ?? new(),
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        });
    }
}
