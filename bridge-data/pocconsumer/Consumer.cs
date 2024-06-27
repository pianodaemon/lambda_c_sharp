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
public delegate IBusControl SetupBusDelegate(string secretKey, string accessKey, RegionEndpoint region, string queueName, string sourceBucket, HashSet<string> nonRestrictedDirs, FileSaver fileSaver);

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
        return consumer.ExtractMessagesMassivily(BusHelper.setupBus, StorageHelper.SaveOnPersistence);
    }

    public async Task ExtractMessagesMassivily(SetupBusDelegate setupBus, FileSaver fileSaver)
    {
        var busControl = setupBus(secretKey, accessKey, region, queueName, sourceBucket, nonRestrictedDirs, fileSaver);
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
