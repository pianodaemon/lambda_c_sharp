using Amazon.S3;
using MassTransit;
using Serilog;
using TestPublisherService.Extensions;
using BridgeDataConsumer.Console.Models;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try {
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();

    builder.Services.AddAWSService<IAmazonS3>(builder.Configuration.GetAWSOptions<AmazonS3Config>("AWS"));
    builder.Services.AddMassTransitServices(builder.Configuration);

    var app = builder.Build();

    app.MapGet("/publish-test-message", async(IPublishEndpoint publisher) => {
        var message = new MovedToBridgeData{FileKey = "/host", TargetPath = "/tmp/hosts_copy.txt"};
        await publisher.Publish(message, x => { x.SetGroupId("myMessageGroup"); });
        return Results.Ok("Test message sent successfully");
    });

    app.Run();
} catch (Exception ex) {
    Log.Fatal(ex, "Application terminated unexpectedly");
} finally {
    Log.CloseAndFlush();
}
