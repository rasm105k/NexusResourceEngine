using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class StateEndpoints
{
    public static void MapStateEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/states");

        group.MapGet("/", async (HttpContext context) =>
        {
            var service = context.RequestServices.GetRequiredService<IResourceStateService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.GetAllAsync(tenantId);
            return Results.Ok(result);
        });

        group.MapPost("/", async (HttpContext context, NexusResourceEngine.Application.DTOs.States.CreateResourceStateDto dto) =>
        {
            var service = context.RequestServices.GetRequiredService<IResourceStateService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.CreateAsync(dto, tenantId);
            return Results.Created($"/states/{result.StateId}", result);
        });

        var transitionGroup = app.MapGroup("/transitions");

        transitionGroup.MapPost("/", async (HttpContext context, NexusResourceEngine.Application.DTOs.Transitions.CreateStateTransitionDto dto) =>
        {
            var service = context.RequestServices.GetRequiredService<IStateTransitionService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.CreateAsync(dto, tenantId);
            return Results.Created($"/transitions/{result.TransitionId}", result);
        });
    }
}
