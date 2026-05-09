using NexusResourceEngine.Presentation.Endpoints;
using NexusResourceEngine.Presentation.Middleware;
using Scalar.AspNetCore;
using Serilog;

namespace NexusResourceEngine.Presentation.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApplicationPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseMiddleware<GlobalExceptionHandler>();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<TenantMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        return app;
    }

    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapAuthEndpoints();
        app.MapUserEndpoints();
        app.MapStateEndpoints();
        app.MapResourceEndpoints();
        app.MapBookingEndpoints();
        app.MapAvailabilityEndpoint();
        app.MapDevEndpoints();

        return app;
    }
}
