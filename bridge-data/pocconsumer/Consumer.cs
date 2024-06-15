namespace POCConsumer;

using System;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;

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
