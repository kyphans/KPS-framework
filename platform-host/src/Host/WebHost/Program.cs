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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Infrastructure Services
builder.Services.AddDbContext<MasterDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDb") ?? @"Server=(localdb)\mssqllocaldb;Database=PlatformMasterDb;Trusted_Connection=True;MultipleActiveResultSets=true"));

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
    new BillingModule()
};

foreach (var module in modules)
{
    module.ConfigureServices(builder.Services);
}

var app = builder.Build();

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
            ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=Platform_Demo;Trusted_Connection=True;MultipleActiveResultSets=true",
            EnabledModulesJson = "[\"billing\"]"
        });
        db.SaveChanges();
    }
}

app.Run();
