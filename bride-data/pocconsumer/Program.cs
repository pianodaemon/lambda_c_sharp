namespace POCConsumer;

using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json.Linq;

public static class MessageHelper
{
    public const string FileKeyField = "fileKey";
    public const string TargetPathField = "targetPath";

    public static record struct BridgePartialData(string FileKey, string TargetPath);

    public static BridgePartialData DecodeMessage(string messageBody)
    {
        JObject json = JObject.Parse(messageBody);
        string fileKey = json[FileKeyField]?.ToString() ?? throw new ArgumentNullException(FileKeyField, $"{FileKeyField} is required and cannot be null or empty.");
        string targetPath = json[TargetPathField]?.ToString() ?? throw new ArgumentNullException(TargetPathField, $"{TargetPathField} is required and cannot be null or empty.");
        return new BridgePartialData(fileKey, targetPath);
    }
}

public static class StorageHelper
{
    public static async Task SaveOnPersistence(AmazonS3Client s3Client, string sourceBucket, MessageHelper.BridgePartialData bridgePartialData)
    {
        try
        {
            ValidateTargetPath(bridgePartialData.TargetPath);

            Directory.CreateDirectory(Path.GetDirectoryName(bridgePartialData.TargetPath) ?? throw new InvalidOperationException("Target path is null or invalid."));

            await Fetch(s3Client, sourceBucket, bridgePartialData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading or saving file: {ex.Message}");
            throw;
        }
    }

    private static async Task Fetch(AmazonS3Client s3Client, string sourceBucket, MessageHelper.BridgePartialData bridgePartialData)
    {
        try
        {
            GetObjectRequest getObjectRequest = new()
            {
                BucketName = sourceBucket,
                Key = bridgePartialData.FileKey
            };

            using GetObjectResponse response = await s3Client.GetObjectAsync(getObjectRequest);
            await response.WriteResponseStreamToFileAsync(bridgePartialData.TargetPath, false, default);
            Console.WriteLine($"File downloaded to {bridgePartialData.TargetPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file from S3: {ex.Message}");
            throw;
        }
    }

    private static void ValidateTargetPath(string targetPath)
    {
        if (string.IsNullOrEmpty(targetPath))
        {
            throw new ArgumentNullException(nameof(targetPath), "Target path (filename) is not specified.");
        }

        if (File.Exists(targetPath))
        {
            throw new InvalidOperationException($"File {targetPath} already exists. Skipping download.");
        }
    }
}

public class Consumer
{
    private readonly string queueUrl;
    private readonly string sourceBucket;
    private readonly AmazonSQSClient sqsClient;
    private readonly AmazonS3Client s3Client;

    private Consumer(string queueUrl, string sourceBucket, AmazonSQSClient sqsClient, AmazonS3Client s3Client)
    {
        this.queueUrl = queueUrl;
        this.sourceBucket = sourceBucket;
        this.sqsClient = sqsClient;
        this.s3Client = s3Client;
    }

    public static Consumer Create(string queueUrl, string sourceBucket, AmazonSQSClient sqsClient, AmazonS3Client s3Client)
    {
        return new(queueUrl, sourceBucket, sqsClient, s3Client);
    }

    public async Task ExtractMessages(MessageHelper.MessageBodyDecoder messageBodyDecoder, StorageHelper.FileSaver fileSaver)
    {
        ReceiveMessageRequest receiveMessageRequest = new()
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 10
        };

        try
        {
            ReceiveMessageResponse receiveMessageResponse = await sqsClient.ReceiveMessageAsync(receiveMessageRequest);

            if (receiveMessageResponse.Messages.Count > 0)
            {
                foreach (var message in receiveMessageResponse.Messages)
                {
                    Console.WriteLine($"Message received: {message.Body}");

                    MessageHelper.BridgePartialData bridgePartialData;
                    try
                    {
                        bridgePartialData = messageBodyDecoder(message.Body);
                    }
                    catch (ArgumentNullException ex)
                    {
                        Console.WriteLine($"Error decoding message body: {ex.Message}");
                        continue;
                    }

                    try
                    {
                        StorageHelper.ValidateTargetPath(bridgePartialData.TargetPath);
                        await fileSaver(s3Client, sourceBucket, bridgePartialData);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }

                    DeleteMessageRequest deleteMessageRequest = new()
                    {
                        QueueUrl = queueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };
                    await sqsClient.DeleteMessageAsync(deleteMessageRequest);
                    Console.WriteLine("Message deleted from the queue.");
                }
            }
            else
            {
                Console.WriteLine("No messages to process.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }
}

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
