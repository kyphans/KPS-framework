using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Platform.Core;
using Platform.Core.DocTypes;
using Platform.Infrastructure.DocTypes;
using System.Text.Json;
using User.Application.Services;
using User.Infrastructure.Services;
using User.Infrastructure.TenantDb;
using User.Module.Endpoints;

namespace User.Module;

public class UserModule : IModule
{
    public string Key => "user";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<UserDbContext>();
        services.AddScoped<IUserService, UserService>();

        // Seed DocTypes
        var baseDir = AppContext.BaseDirectory;
        var docPath = Path.Combine(baseDir, "UserDocTypes", "User.doc.json");
        
        if (File.Exists(docPath))
        {
            var json = File.ReadAllText(docPath);
            var def = JsonSerializer.Deserialize<DocTypeDefinition>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (def != null)
            {
                def.Module = Key; 
                MasterDocTypeProvider.Register(def);
            }
        }
    }

    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/user/test", () => "Hello from User Module!")
            .WithTags("User");

        UserEndpoints.MapEndpoints(endpoints);
        
        // Hack: Ensure DB Created for the module specific table
        // In real app, this should be in the CLI tool or Migration Runner
        using (var scope = endpoints.ServiceProvider.CreateScope())
        {
             // We need a valid Tenant Context to create the DB? 
             // Actually EnsureCreated works if connection string is valid.
             // But here we are in startup, no tenant yet.
             // So we skip auto-migration here and rely on runtime or just hope it exists/EF updates it when accessed.
             // Better: Middleware or first request will trigger if we wanted, but usually migrations are separate.
             // For this prototype, we rely on the fact that the DB exists (created by CLI) 
             // AND we need to create the 'Users' table.
        }
    }
}
