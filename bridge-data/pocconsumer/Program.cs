namespace POCConsumer;

using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.SQS;

class Program
{
    static async Task Main(string[] args)
    {
        string queueUrl = "https://sqs.us-east-1.amazonaws.com/123456789012/MyQueue";
        string sourceBucket = "my-bucket";
        Console.WriteLine($"Starting to consume messages from SQS with queue URL: {queueUrl} and source bucket: {sourceBucket}...");

        AmazonSQSClient sqsClient = new(RegionEndpoint.USEast1);
        AmazonS3Client s3Client = new(RegionEndpoint.USEast1);
        Consumer consumer = Consumer.Create(queueUrl, sourceBucket, sqsClient, s3Client);
        await StartConsuming(consumer, 5000);
    }

    static async Task StartConsuming(Consumer consumer, int delayMilliseconds)
    {
        while (true)
        {
            await consumer.ExtractMessages(MessageHelper.DecodeMessage, StorageHelper.SaveOnPersistence);

            Console.WriteLine("Waiting for new messages...");
            await Task.Delay(delayMilliseconds);
        }
    }
}
