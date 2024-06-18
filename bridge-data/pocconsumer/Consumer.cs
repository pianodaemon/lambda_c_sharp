namespace POCConsumer;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;

public record BridgePartialData(string FileKey, string TargetPath);

public delegate BridgePartialData MessageBodyDecoder(string messageBody);
public delegate Task FileSaver(AmazonS3Client s3Client, string sourceBucket, HashSet<string> nonRestrictedDirs, BridgePartialData bridgePartialData);


public class Consumer
{
    private readonly string queueUrl;
    private readonly string sourceBucket;
    private readonly AmazonSQSClient sqsClient;
    private readonly AmazonS3Client s3Client;
    private readonly HashSet<string> nonRestrictedDirs;

    private Consumer(string queueUrl, string sourceBucket, HashSet<string> nonRestrictedDirs, AmazonSQSClient sqsClient, AmazonS3Client s3Client)
    {
        this.queueUrl = queueUrl;
        this.sourceBucket = sourceBucket;
        this.sqsClient = sqsClient;
        this.s3Client = s3Client;
        this.nonRestrictedDirs = nonRestrictedDirs;
    }

    public static async Task StartConsumingLoop(string queueUrl, string sourceBucket, HashSet<string> nonRestrictedDirs, AmazonSQSClient sqsClient, AmazonS3Client s3Client, int delayMilliseconds)
    {
        Consumer consumer = new Consumer(queueUrl, sourceBucket, nonRestrictedDirs, sqsClient, s3Client);
        while (true)
        {
            await consumer.ExtractMessages(MessageHelper.DecodeMessage, StorageHelper.SaveOnPersistence);

            Console.WriteLine("Waiting for new messages...");
            await Task.Delay(delayMilliseconds);
        }
    }

    private async Task ExtractMessages(MessageBodyDecoder messageBodyDecoder, FileSaver fileSaver)
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

                    BridgePartialData bridgePartialData;
                    bridgePartialData = messageBodyDecoder(message.Body);
                    await fileSaver(s3Client, sourceBucket, nonRestrictedDirs, bridgePartialData);

                    DeleteMessageRequest deleteMessageRequest = new()
                    {
                        QueueUrl = queueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };
                    await sqsClient.DeleteMessageAsync(deleteMessageRequest);
                    Console.WriteLine("Message deleted from the queue.");
                }
                return;
            }

            Console.WriteLine("No messages to process.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }
}
