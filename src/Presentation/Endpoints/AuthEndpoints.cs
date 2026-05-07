using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (HttpContext context, NexusResourceEngine.Application.DTOs.Auth.RegisterRequestDto request) =>
        {
            var authService = context.RequestServices.GetRequiredService<IAuthService>();
            var tenantId = Guid.NewGuid();
            var result = await authService.RegisterAsync(request, tenantId);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (HttpContext context, NexusResourceEngine.Application.DTOs.Auth.LoginRequestDto request) =>
        {
            var authService = context.RequestServices.GetRequiredService<IAuthService>();
            var result = await authService.LoginAsync(request);
            return Results.Ok(result);
        });
    }
}
