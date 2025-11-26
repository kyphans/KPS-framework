using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Platform.Infrastructure.MasterDb;
using Tenant.Cli.Commands;
using Tenant.Cli.Services;

// Setup DbContext
var connectionString = "Data Source=PlatformMasterDb.sqlite";
var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
optionsBuilder.UseSqlite(connectionString);

using var dbContext = new MasterDbContext(optionsBuilder.Options);
dbContext.Database.EnsureCreated(); // Ensure MasterDb exists

// Services
var provisioner = new TenantDbProvisioner(dbContext);

// CLI Root
var rootCommand = new RootCommand("Tenant CLI Tool");
rootCommand.AddCommand(NewTenantCommand.Create(provisioner));

await rootCommand.InvokeAsync(args);