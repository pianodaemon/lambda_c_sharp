using Amazon.S3;
using Amazon.S3.Transfer;
using BridgeDataConsumer.Console.Models;
using BridgeDataConsumer.Console.Interfaces;

namespace BridgeDataConsumer.Console;

public class S3Repository(IAmazonS3 s3Client, string sourceBucket) : IFileRepository
{
    public async Task<string> DoPlacementAsync(MovedToBridgeData movedToBridgeData)
    {
        if (string.IsNullOrEmpty(movedToBridgeData.TargetPath))
        {
            throw new InvalidOperationException("Target path is null or invalid.");
        }
        string targetPathDownload = $"{movedToBridgeData.TargetPath}.download";
        Directory.CreateDirectory(Path.GetDirectoryName(targetPathDownload) ?? throw new InvalidOperationException("Target path is null or invalid."));

        if (string.IsNullOrEmpty(movedToBridgeData.FileKey))
        {
            throw new InvalidOperationException("File key is null or invalid.");
        }
        await Fetch(movedToBridgeData.FileKey.TrimStart('/'), targetPathDownload);
        return targetPathDownload;
    }

    private async Task Fetch(string key, string downloadPath)
    {
        var tu = new TransferUtility(s3Client);
        await tu.DownloadAsync(downloadPath, sourceBucket, key);
    }
}
