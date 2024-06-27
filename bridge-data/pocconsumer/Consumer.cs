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


public record BridgePartialData(string FileKey, string TargetPath);

public delegate Task FileSaver(AmazonS3Client s3Client, string sourceBucket, HashSet<string> nonRestrictedDirs, BridgePartialData bridgePartialData);


public class Consumer
{
    private readonly string secretKey;
    private readonly string accessKey;
    private readonly string queueName;
    private readonly string sourceBucket;
    private readonly HashSet<string> nonRestrictedDirs;
    private readonly RegionEndpoint region;

    public Consumer(string secretKey, string accessKey, RegionEndpoint region, string queueName, string sourceBucket, HashSet<string> nonRestrictedDirs)
    {
       	this.secretKey = secretKey;
       	this.accessKey = accessKey;
       	this.queueName = queueName;
        this.sourceBucket = sourceBucket;
        this.nonRestrictedDirs = nonRestrictedDirs;
        this.region = region;
    }

    public static Task StartConsumingLoop(string secretKey, string accessKey, RegionEndpoint region, string queueName, string sourceBucket, HashSet<string> nonRestrictedDirs)
    {
        Consumer consumer = new Consumer(secretKey, accessKey, region, queueName, sourceBucket, nonRestrictedDirs);
        return consumer.ExtractMessagesMassivily(StorageHelper.SaveOnPersistence);
    }

    public async Task ExtractMessagesMassivily(FileSaver fileSaver)
    {
        var s3Client = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), region);
        var busControl = Bus.Factory.CreateUsingAmazonSqs(cfg =>
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
                        await fileSaver(s3Client, sourceBucket, nonRestrictedDirs, context.Message);
                    });
                });
            });
        });

        await busControl.StartAsync();
        try
        {
            Console.WriteLine("Press any key to exit");
            await Task.Run(() => Console.ReadKey());
        }
        finally
        {
            await busControl.StopAsync();
        }
    }
}
