using NexusResourceEngine.Application.Interfaces;

namespace NexusResourceEngine.Presentation.Endpoints;

public static class BookingEndpoints
{
    public static void MapBookingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/bookings");

        group.MapPost("/", async (HttpContext context, NexusResourceEngine.Application.DTOs.Bookings.CreateBookingDto dto) =>
        {
            var service = context.RequestServices.GetRequiredService<IBookingService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var userId = Guid.NewGuid();
            var result = await service.CreateAsync(dto, userId, tenantId);
            return Results.Created($"/bookings/{result.BookingId}", result);
        });

        group.MapPatch("/{bookingId:guid}/status", async (Guid bookingId, HttpContext context, NexusResourceEngine.Application.DTOs.Bookings.UpdateBookingStatusDto dto) =>
        {
            var service = context.RequestServices.GetRequiredService<IBookingService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var result = await service.UpdateStatusAsync(bookingId, dto, tenantId);
            return Results.Ok(result);
        });
    }

    public static void MapAvailabilityEndpoint(this WebApplication app)
    {
        app.MapGet("/resources/{resourceId:guid}/availability", async (Guid resourceId, HttpContext context, DateTime start, DateTime end) =>
        {
            var service = context.RequestServices.GetRequiredService<IBookingService>();
            var tenantId = (Guid)context.Items["TenantId"]!;
            var available = await service.CheckAvailabilityAsync(resourceId, start, end, tenantId);
            return Results.Ok(new { available });
        });
    }
}
