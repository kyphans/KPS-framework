using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Platform.Core;
using Platform.Core.DocTypes;
using Platform.Infrastructure.DocTypes;
using System.Text.Json;
using System.Reflection;

namespace Billing.Module;

public class BillingModule : IModule
{
    public string Key => "billing";

    public void ConfigureServices(IServiceCollection services)
    {
        // Seed DocTypes
        // In a real app, we might use a Seeder service, but here we do it on startup for simplicity
        var baseDir = AppContext.BaseDirectory;
        var docPath = Path.Combine(baseDir, "BillingDocTypes", "Invoice.doc.json");
        
        if (File.Exists(docPath))
        {
            var json = File.ReadAllText(docPath);
            var def = JsonSerializer.Deserialize<DocTypeDefinition>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (def != null)
            {
                // Explicitly force module name to ensure consistency
                def.Module = Key; 
                MasterDocTypeProvider.Register(def);
            }
        }
    }

    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Module specific endpoints
    }
}
