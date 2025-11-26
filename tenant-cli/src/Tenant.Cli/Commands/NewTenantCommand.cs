using System.CommandLine;
using Tenant.Cli.Services;

namespace Tenant.Cli.Commands;

public static class NewTenantCommand
{
    public static Command Create(TenantDbProvisioner provisioner)
    {
        var command = new Command("new-tenant", "Creates a new tenant");

        var codeOption = new Option<string>(
            name: "--code",
            description: "The tenant code (ID)")
            { IsRequired = true };

        var modulesOption = new Option<string[]>(
            name: "--modules",
            description: "List of enabled modules",
            getDefaultValue: () => Array.Empty<string>());

        command.AddOption(codeOption);
        command.AddOption(modulesOption);

        command.SetHandler(async (string code, string[] modules) =>
        {
            await provisioner.CreateTenantAsync(code, modules);
        }, codeOption, modulesOption);

        return command;
    }
}
