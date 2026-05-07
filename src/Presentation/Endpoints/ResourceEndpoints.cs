using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class ResourceEndpoints
{
    public static void MapResourceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/resources");

        group.MapGet("/", async (HttpContext context, Guid? stateId) =>
        {
            var service = context.RequestServices.GetRequiredService<IResourceService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.GetAllAsync(tenantId, stateId);
            return Results.Ok(result);
        });

        group.MapPost("/", async (HttpContext context, NexusResourceEngine.Application.DTOs.Resources.CreateResourceDto dto) =>
        {
            var service = context.RequestServices.GetRequiredService<IResourceService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.CreateAsync(dto, tenantId);
            return Results.Created($"/resources/{result.ResourceId}", result);
        });

        group.MapPatch("/{resourceId:guid}/state", async (Guid resourceId, HttpContext context, NexusResourceEngine.Application.DTOs.Resources.UpdateResourceStateDto dto) =>
        {
            var service = context.RequestServices.GetRequiredService<IResourceService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.ChangeStateAsync(resourceId, dto, tenantId);
            return Results.Ok(result);
        });
    }
}
