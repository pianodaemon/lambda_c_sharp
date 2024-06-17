namespace POCConsumer;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.SQS;

class Program
{
    static async Task Main(string[] args)
    {
        HashSet<string> overwritePermissibleDirectories = new HashSet<string>
        {
            "/path/to/dir1",
            "/path/to/dir2"
        };

        string queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/MyQueue";
        string sourceBucket = "my-bucket";
        Console.WriteLine($"Starting to consume messages from SQS with queue URL: {queueUrl} and source bucket: {sourceBucket}...");

        AmazonSQSClient sqsClient = new(RegionEndpoint.USEast1);
        AmazonS3Client s3Client = new(RegionEndpoint.USEast1);
        await Consumer.StartConsumingLoop(queueUrl, sourceBucket, overwritePermissibleDirectories, sqsClient, s3Client, 5000);
    }
}
