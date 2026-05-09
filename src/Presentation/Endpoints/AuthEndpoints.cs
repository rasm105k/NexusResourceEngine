using Microsoft.AspNetCore.Authorization;
using NexusResourceEngine.Application.DTOs.Auth;
using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (IAuthService authService, RegisterRequestDto request) =>
        {
            var tenantId = Guid.NewGuid();
            var result = await authService.RegisterAsync(request, tenantId);
            return Results.Ok(result);
        });

        group.MapPost("/register-member", [Authorize(Roles = "Admin")] async (HttpContext context, IAuthService authService, RegisterMemberDto request) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await authService.RegisterMemberAsync(request, tenantId);
            return Results.Ok(result);
        });

        group.MapPost("/invite", [Authorize(Roles = "Admin")] async (HttpContext context, IAuthService authService, InviteRequestDto request) =>
        {
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await authService.InviteAsync(request, tenantId);
            return Results.Ok(result);
        });

        group.MapPost("/accept-invite", async (IAuthService authService, AcceptInviteDto request) =>
        {
            var result = await authService.AcceptInviteAsync(request);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (IAuthService authService, LoginRequestDto request) =>
        {
            var result = await authService.LoginAsync(request);
            return Results.Ok(result);
        });
    }
}
