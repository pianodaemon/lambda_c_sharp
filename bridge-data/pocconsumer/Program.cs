namespace POCConsumer;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SQS;

class Program
{
    static async Task Main(string[] args)
    {
        HashSet<string> overwritePermissibleDirectories = new HashSet<string>
        {
            "/path/to/dir2"
        };

        string queueName = "my-queue";
        string sourceBucket = "my-bucket";
        Console.WriteLine($"Starting to consume messages from SQS queue: {queueName} and source bucket: {sourceBucket}...");

        try
        {
            await Consumer.StartConsumingLoop("secretKey", "accessKey", RegionEndpoint.USEast1 , queueName, sourceBucket, overwritePermissibleDirectories);

            //await Consumer.StartConsumingLoop(queueUrl, sourceBucket, overwritePermissibleDirectories, sqsClient, s3Client, cts.Token, 5000);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Consuming loop cancelled.");
        }
        finally
        {
            Console.WriteLine("Application is shutting down...");
        }
    }
}
