using Microsoft.EntityFrameworkCore;
using Platform.Core;
using Platform.Core.Docs;
using Platform.Core.DocTypes;
using Platform.Core.Events;
using Platform.Core.Tenancy;
using Platform.Infrastructure.Docs;
using Platform.Infrastructure.DocTypes;
using Platform.Infrastructure.Events;
using Platform.Infrastructure.MasterDb;
using Platform.Infrastructure.Tenancy;
using Billing.Module;
using User.Module;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KPS Platform API", Version = "v1" });

    // Add Tenant Header support
    c.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme
    {
        Description = "Tenant Code (e.g. demo)",
        Name = "X-Tenant-Code",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Tenant"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Infrastructure Services
// Use a shared path for MasterDb so CLI and WebHost see the same DB
var dbPath = Path.Combine(AppContext.BaseDirectory, "PlatformMasterDb.sqlite");
builder.Services.AddDbContext<MasterDbContext>(options => 
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddDbContext<PlatformTenantDbContext>(); 

builder.Services.AddScoped<ITenantResolver, HttpTenantResolver>();
builder.Services.AddScoped<ITenantAccessor, TenantAccessor>();
builder.Services.AddSingleton<IDocTypeProvider, MasterDocTypeProvider>(); // Singleton for the static cache
builder.Services.AddScoped<IDocEngine, GenericDocEngine>();
builder.Services.AddScoped<IEventBus, InProcessEventBus>();
builder.Services.AddScoped<HookRegistry>();

// Modules
var modules = new List<IModule>
{
    new BillingModule(),
    new UserModule()
};

foreach (var module in modules)
{
    module.ConfigureServices(builder.Services);
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "KPS Platform API v1");
    c.RoutePrefix = "swagger"; // Default
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseMiddleware<TenantMiddleware>();

foreach (var module in modules)
{
    module.ConfigureEndpoints(app);
}

app.MapControllers();

// Database Migration (For Dev convenience - normally handled by CLI)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
    db.Database.EnsureCreated(); // create master db if not exists
    
    // Seed demo tenant if empty
    if (!db.Tenants.Any(t => t.Id == "demo"))
    {
        db.Tenants.Add(new Platform.Infrastructure.MasterDb.Entities.TenantEntity
        {
            Id = "demo",
            Name = "Demo Tenant",
            ConnectionString = "Data Source=Platform_Demo.sqlite",
            EnabledModulesJson = "[\"billing\"]"
        });
        db.SaveChanges();
    }
}

app.MapGet("/api/debug/tenants", async (MasterDbContext db) => 
{
    var tenants = await db.Tenants.ToListAsync();
    return new 
    { 
        DbPath = dbPath,
        Count = tenants.Count,
        Tenants = tenants.Select(t => new { t.Id, t.Name })
    };
}).WithTags("Debug");

app.Run();
