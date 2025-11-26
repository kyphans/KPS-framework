using System.CommandLine;
using Microsoft.EntityFrameworkCore;
using Platform.Infrastructure.MasterDb;
using Tenant.Cli.Commands;
using Tenant.Cli.Services;

// Setup DbContext
var connectionString = @"Server=(localdb)\mssqllocaldb;Database=PlatformMasterDb;Trusted_Connection=True;MultipleActiveResultSets=true";
var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
optionsBuilder.UseSqlServer(connectionString);

using var dbContext = new MasterDbContext(optionsBuilder.Options);
dbContext.Database.EnsureCreated(); // Ensure MasterDb exists

// Services
var provisioner = new TenantDbProvisioner(dbContext);

// CLI Root
var rootCommand = new RootCommand("Tenant CLI Tool");
rootCommand.AddCommand(NewTenantCommand.Create(provisioner));

await rootCommand.InvokeAsync(args);