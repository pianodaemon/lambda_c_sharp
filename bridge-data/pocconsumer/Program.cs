using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = POCConsumer.MassTransitHelper.CreateHostBuilder(args);
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
