using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using NexusResourceEngine.Application.DTOs.Users;
using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users").RequireAuthorization();

        group.MapGet("/", async (HttpContext context) =>
        {
            var service = context.RequestServices.GetRequiredService<IUserService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.GetAllAsync(tenantId);
            return Results.Ok(result);
        });

        group.MapPatch("/{userId:guid}/role", [Authorize(Roles = "Admin")] async (Guid userId, HttpContext context, UpdateUserRoleDto dto) =>
        {
            var service = context.RequestServices.GetRequiredService<IUserService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var currentUserId = Guid.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await service.UpdateRoleAsync(userId, dto, tenantId, currentUserId);
            return Results.Ok(result);
        });
    }
}
