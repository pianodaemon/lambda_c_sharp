using BridgeDataConsumer.Console.Extensions;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddApplicationServices();
    builder.Host.UseSerilog((ctx, servs, conf) => conf
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(servs)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter()));

    var app = builder.Build();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly}");
}
finally
{
    Log.CloseAndFlush();
}
