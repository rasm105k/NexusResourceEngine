using NexusResourceEngine.Presentation.Extensions;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services
        .AddInfrastructure(builder.Configuration)
        .AddAuth(builder.Configuration)
        .AddApplicationServices()
        .AddOpenApi();

    var app = builder.Build();

    app
        .UseApplicationPipeline()
        .MapApiEndpoints();

    Log.Information("NexusResourceEngine is running");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
