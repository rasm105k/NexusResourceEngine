using Serilog.Context;

namespace NexusResourceEngine.Presentation.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantIdClaim = context.User?.FindFirst("TenantId")?.Value;

        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            context.Items["TenantId"] = tenantId;

            using (LogContext.PushProperty("TenantId", tenantId))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }
}
