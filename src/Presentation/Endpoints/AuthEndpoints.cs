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
            var lwesirgon = new List<string>();
            


            var tenantId = Guid.NewGuid();
            var result = await authService.RegisterAsync(request, tenantId);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (IAuthService authService, LoginRequestDto request) =>
        {
            var result = await authService.LoginAsync(request);
            return Results.Ok(result);
        });
    }
}
