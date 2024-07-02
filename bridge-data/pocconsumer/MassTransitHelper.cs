namespace POCConsumer;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using MassTransit;
using System.Net.Mime;

using Serilog;

public record BridgePartialData(string FileKey, string TargetPath);
public delegate Task FileSaver(ILogger<BridgePartialData> logger,
                               AmazonS3Client s3Client,
                               string sourceBucket,
                               HashSet<string> nonRestrictedDirs,
                               BridgePartialData bridgePartialData);

internal static class MassTransitHelper
{
    private static void setup(IServiceCollection services, string secretKey, string accessKey,
                              RegionEndpoint region, string queueName, string sourceBucket,
                              HashSet<string> nonRestrictedDirs, FileSaver fileSaver)
    {
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<BridgePartialData>>();
        var s3Client = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), region);

        logger.LogInformation($"Starting to consume messages from SQS queue: {queueName} and source bucket: {sourceBucket}");

        services.AddMassTransit(mt =>
        {
            mt.AddConsumers(typeof(BridgePartialData).Assembly);
            mt.UsingAmazonSqs((context, cfg) =>
            {
                cfg.Host(region.SystemName, h =>
                {
                    h.AccessKey(accessKey);
                    h.SecretKey(secretKey);
                });

                cfg.ReceiveEndpoint(queueName, e =>
                {
                    e.DefaultContentType = new ContentType("application/json");
                    e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                    e.ConfigureConsumeTopology = false;
                    e.Handler<BridgePartialData>(context =>
                    {
                        return Task.Run(async () =>
                        {
                            await fileSaver(logger, s3Client, sourceBucket, nonRestrictedDirs, context.Message);
                        });
                    });
                });
            });
        });
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args).UseSerilog().ConfigureServices((hostContext, services) =>
        {

            HashSet<string> nonRestrictedDirs = new HashSet<string>
            {
                "/path/to/dir2"
            };

            RegionEndpoint region = RegionEndpoint.USEast2;
            string queueName = "my-queue";
            string sourceBucket = "my-bucket-000";

            setup(services,
                  "SECRET_KEY_HERE",
                  "ACCESS_KEY_HERE",
                  region, queueName, sourceBucket,
                  nonRestrictedDirs, StorageHelper.SaveOnPersistence);
        });
    }
}
