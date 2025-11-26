using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Platform.Core.Tenancy;
using User.Application.Services;

namespace User.Module.Endpoints;

public static class UserEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("User CRUD");

        group.MapPost("/", async (CreateUserRequest req, [FromServices] IUserService service, [FromServices] User.Infrastructure.TenantDb.UserDbContext dbContext, [FromServices] ITenantAccessor tenantAccessor) =>
        {
            if (tenantAccessor.CurrentTenant == null) return Results.BadRequest("Tenant header 'X-Tenant-Code' is missing or invalid.");

            // DEV HACK: Ensure table exists for this tenant
            await dbContext.Database.EnsureCreatedAsync();

            var id = await service.CreateUserAsync(req.Username, req.Email, req.Role);
            return Results.Created($"/api/users/{id}", new { Id = id });
        });

        group.MapGet("/{id:guid}", async (Guid id, [FromServices] IUserService service, [FromServices] ITenantAccessor tenantAccessor) =>
        {
            if (tenantAccessor.CurrentTenant == null) return Results.BadRequest("Tenant header 'X-Tenant-Code' is missing or invalid.");
            var user = await service.GetUserAsync(id);
            return user is not null ? Results.Ok(user) : Results.NotFound();
        });

        group.MapGet("/", async ([FromServices] IUserService service, [FromServices] ITenantAccessor tenantAccessor) =>
        {
            if (tenantAccessor.CurrentTenant == null) return Results.BadRequest("Tenant header 'X-Tenant-Code' is missing or invalid.");
            var users = await service.GetAllUsersAsync();
            return Results.Ok(users);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest req, [FromServices] IUserService service, [FromServices] ITenantAccessor tenantAccessor) =>
        {
            if (tenantAccessor.CurrentTenant == null) return Results.BadRequest("Tenant header 'X-Tenant-Code' is missing or invalid.");
            await service.UpdateUserAsync(id, req.Username, req.Email, req.Role);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, [FromServices] IUserService service, [FromServices] ITenantAccessor tenantAccessor) =>
        {
            if (tenantAccessor.CurrentTenant == null) return Results.BadRequest("Tenant header 'X-Tenant-Code' is missing or invalid.");
            await service.DeleteUserAsync(id);
            return Results.NoContent();
        });
    }
}

public record CreateUserRequest(string Username, string Email, string Role);
public record UpdateUserRequest(string Username, string Email, string Role);
