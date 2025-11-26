using Microsoft.AspNetCore.Mvc;
using Platform.Core.Docs;
using Platform.Core.Tenancy;
using Platform.Shared.Contracts.Docs;
using System.Text.Json;

namespace WebHost.Controllers;

[ApiController]
[Route("api/{moduleKey}/docs/{docType}")]
public class DocsController : ControllerBase
{
    private readonly IDocEngine _docEngine;
    private readonly ITenantAccessor _tenantAccessor;

    public DocsController(IDocEngine docEngine, ITenantAccessor tenantAccessor)
    {
        _docEngine = docEngine;
        _tenantAccessor = tenantAccessor;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string moduleKey, string docType, [FromBody] JsonElement body)
    {
        if (_tenantAccessor.CurrentTenant == null)
            return Unauthorized("Tenant not identified");

        // Convert JsonElement to Dictionary
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(body.GetRawText());

        var result = await _docEngine.CreateAsync(moduleKey, docType, data ?? new());

        if (!result.IsSuccess)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string moduleKey, string docType, string id)
    {
         if (_tenantAccessor.CurrentTenant == null)
            return Unauthorized("Tenant not identified");
            
        var result = await _docEngine.GetAsync(id);
        if (!result.IsSuccess)
            return NotFound(result.Error);
            
        return Ok(result.Value);
    }
}
