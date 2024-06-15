namespace POCConsumer;

using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;

public record BridgePartialData(string FileKey, string TargetPath);

public delegate BridgePartialData MessageBodyDecoder(string messageBody);
public delegate Task FileSaver(AmazonS3Client s3Client, string sourceBucket, BridgePartialData bridgePartialData);


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

    private async Task StartConsuming(int delayMilliseconds)
    {
        while (true)
        {
            await ExtractMessages(MessageHelper.DecodeMessage, StorageHelper.SaveOnPersistence);

            Console.WriteLine("Waiting for new messages...");
            await Task.Delay(delayMilliseconds);
        }
    }

    public static async Task StartConsumingLoop(string queueUrl, string sourceBucket, AmazonSQSClient sqsClient, AmazonS3Client s3Client, int delayMilliseconds)
    {
        var consumer = new Consumer(queueUrl, sourceBucket, sqsClient, s3Client);
        await consumer.StartConsuming(delayMilliseconds);
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
                    try
                    {
                        bridgePartialData = messageBodyDecoder(message.Body);
                        await fileSaver(s3Client, sourceBucket, bridgePartialData);
                    }
                    catch (ArgumentNullException ex)
                    {
                        Console.WriteLine($"Error decoding message body: {ex.Message}");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                        throw;
                    }

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