using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using MassTransit;
using System.Net.Mime;

namespace POCConsumer;

internal static class MassTransitHelper
{
    private static void setupService(IServiceCollection services, string secretKey, string accessKey,
                                     RegionEndpoint region, string queueName, string sourceBucket)
    {
        services.AddMassTransit(mt =>
        {
            mt.AddConsumer<LogConsumer>();
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
                    e.ConfigureConsumer<LogConsumer>(context);
                });
            });
        });

        services.AddSingleton<IAmazonS3>(sp =>
        {
            return new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), region);
        });

        services.AddLogging(configure => configure.AddConsole());
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args).ConfigureServices((_, services) =>
        {
            RegionEndpoint region = RegionEndpoint.USEast2;
            string queueName = "my-queue";
            string sourceBucket = "my-bucket-000";

            setupService(services, "secretKey", "accessKey",
                         region, queueName, sourceBucket);

            Console.WriteLine($"Starting to consume messages from SQS queue: {queueName} and source bucket: {sourceBucket}...");
        });
    }
}
