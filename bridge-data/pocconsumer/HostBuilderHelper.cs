namespace POCConsumer;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using MassTransit;
using MassTransit.Clients;
using MassTransit.AmazonSqsTransport;
using Microsoft.Extensions.Hosting;

public static class HostBuilderHelper
{
    public static IHostBuilder setupBuilder(string secretKey, string accessKey,
                                        RegionEndpoint region, string queueName,
                                        string sourceBucket, HashSet<string> nonRestrictedDirs,
				       	FileSaver fileSaver)
    {
        return Host.CreateDefaultBuilder().ConfigureServices((hostContext, services) =>
        {
            services.AddMassTransit(x =>
            {
                x.UsingAmazonSqs((context, cfg) =>
                {

                    cfg.Host(region.SystemName, h =>
                    {
                        h.AccessKey(accessKey);
                        h.SecretKey(secretKey);
                    });

                    cfg.ReceiveEndpoint(queueName, e =>
                    {
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

                x.AddBus(provider => Bus.Factory.CreateUsingAmazonSqs(cfg => cfg.ConfigureEndpoints(provider)));
            });

            services.AddMassTransitHostedService(true);
        });
    }
}
