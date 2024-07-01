namespace POCConsumer;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using MassTransit;

public record BridgePartialData(string FileKey, string TargetPath);
public delegate Task FileSaver(AmazonS3Client s3Client, string sourceBucket, HashSet<string> nonRestrictedDirs, BridgePartialData bridgePartialData);

internal static class MassTransitHelper
{
    public static IServiceCollection setupService(IServiceCollection services, string secretKey, string accessKey,
                                        RegionEndpoint region, string queueName,
                                        string sourceBucket, HashSet<string> nonRestrictedDirs,
				       	FileSaver fileSaver)
    {
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
                    e.ConfigureConsumeTopology = false;
                    e.Handler<BridgePartialData>(context =>
                    {
                        return Task.Run(async () =>
                        {
                            var s3Client = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), region);
                            await fileSaver(s3Client, sourceBucket, nonRestrictedDirs, context.Message);
                        });
                    });
                });
            });

            mt.AddBus(provider => Bus.Factory.CreateUsingAmazonSqs(cfg => cfg.ConfigureEndpoints(provider)));
        });

        return services;
    }
}
