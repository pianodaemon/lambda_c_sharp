using Amazon.S3.Model;
using Amazon.S3;
using MassTransit;
using Microsoft.Extensions.Logging;
using POCConsumer.Models;

namespace POCConsumer;

public class LogConsumer : IConsumer<LogMessage>
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<LogConsumer> _logger;

    public LogConsumer(IAmazonS3 s3Client, ILogger<LogConsumer> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LogMessage> context)
    {
        Console.WriteLine("Consume method invoked.");
        _logger.LogInformation("Consume method invoked.");
        var message = context.Message;

        if (message == null)
        {
            Console.WriteLine("Received null message.");
            _logger.LogWarning("Received null message.");
            return;
        }

        if (string.IsNullOrEmpty(message.FileKey) || string.IsNullOrEmpty(message.TargetPath))
        {
            Console.WriteLine("Received message with null or empty FileKey or TargetPath.");
            _logger.LogWarning("Received message with null or empty FileKey or TargetPath.");
            return;
        }

        Console.WriteLine($"Received message: FileKey={message.FileKey}, TargetPath={message.TargetPath}");
        _logger.LogInformation($"Received message: FileKey={message.FileKey}, TargetPath={message.TargetPath}");

        try
        {
            // Get the file from S3
            var request = new GetObjectRequest
            {
                BucketName = "my-bucket-000",
                Key = message.FileKey.TrimStart('/')
            };

            Console.WriteLine($"Fetching file from S3: {request.BucketName}/{request.Key}");
            _logger.LogInformation($"Fetching file from S3: {request.BucketName}/{request.Key}");
            using var response = await _s3Client.GetObjectAsync(request);
            using var responseStream = response.ResponseStream;
            using var reader = new StreamReader(responseStream);
            var fileContent = await reader.ReadToEndAsync();
            Console.WriteLine("File content retrieved from S3");
            _logger.LogInformation("File content retrieved from S3");

            // Determine the local file path
            var localFilePath = Path.Combine("/connect/logs/elf/mef", Path.GetFileName(message.TargetPath));

            // Ensure the directory exists
            var directoryPath = Path.GetDirectoryName(localFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Append to the local file
            Console.WriteLine($"Appending content to local file: {localFilePath}");
            _logger.LogInformation($"Appending content to local file: {localFilePath}");
            await File.AppendAllTextAsync(localFilePath, fileContent);
            Console.WriteLine("File content appended to target path in local filesystem");
            _logger.LogInformation("File content appended to target path in local filesystem");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
            _logger.LogError($"Error processing message: {ex.Message}");
        }
    }
}
