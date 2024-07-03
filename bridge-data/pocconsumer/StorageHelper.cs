namespace POCConsumer;

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

public static class StorageHelper
{
    private enum Strategy
    {
        Deferral,
        Create,
        Overwrite,
        Versionate
    }

    public static async Task SaveOnPersistence(ILogger logger, AmazonS3Client s3Client, string sourceBucket, HashSet<string> nonRestrictedDirs, BridgePartialData bridgePartialData)
    {
        try
        {
            string targetPathDownload = $"{bridgePartialData.TargetPath}.download";
            Directory.CreateDirectory(Path.GetDirectoryName(targetPathDownload) ?? throw new InvalidOperationException("Target path is null or invalid."));

            await DownloadFileAsync(s3Client, sourceBucket, bridgePartialData.FileKey.TrimStart('/'), targetPathDownload);

            var strategy = DetermineStrategy(bridgePartialData.TargetPath, nonRestrictedDirs);
            ApplyStrategy(targetPathDownload, bridgePartialData.TargetPath, strategy);

            logger.LogInformation($"File downloaded to {bridgePartialData.TargetPath}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error downloading or saving file: {ex.Message}");
            throw;
        }
    }

    private static async Task DownloadFileAsync(AmazonS3Client s3Client, string bucketName, string key, string downloadPath)
    {
        var getObjectRequest = new GetObjectRequest { BucketName = bucketName, Key = key };
        using var response = await s3Client.GetObjectAsync(getObjectRequest);
        await response.WriteResponseStreamToFileAsync(downloadPath, false, default);
    }

    private static void ApplyStrategy(string sourcePath, string targetPath, Strategy strategy)
    {
        switch (strategy)
        {
            case Strategy.Deferral:
                FSUtilHelper.MoveQuery(sourcePath, Path.GetDirectoryName(targetPath));
                break;
            case Strategy.Create:
            case Strategy.Overwrite:
                File.Move(sourcePath, targetPath, true);
                break;
            case Strategy.Versionate:
                FSUtilHelper.MoveFileUnique(sourcePath, targetPath);
                break;
        }
    }

    private static Strategy DetermineStrategy(string targetPath, HashSet<string> nonRestrictedDirs)
    {
        if (!File.Exists(targetPath)) return Strategy.Create;

        string directory = Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid.");
        return nonRestrictedDirs != null && nonRestrictedDirs.Contains(directory)
            ? Strategy.Overwrite
            : Strategy.Versionate;
    }
}
