using Microsoft.EntityFrameworkCore;
using NexusResourceEngine.Infrastructure.Data;
using NexusResourceEngine.Presentation.Middleware;
using NexusResourceEngine.Presentation.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NexusResourceEngineContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandler>();
app.UseMiddleware<TenantMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapAuthEndpoints();
app.MapStateEndpoints();
app.MapResourceEndpoints();
app.MapBookingEndpoints();
app.MapAvailabilityEndpoint();

app.Run();
