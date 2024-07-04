using System;
using System.IO;
using System.Collections.Generic;
using BridgeDataConsumer.Console.Models;
using MassTransit;
using Amazon.S3;
using Amazon.S3.Model;

namespace BridgeDataConsumer.Console.Consumers;

public class MsgConsumer : IConsumer<BridgePartialData>
{
    private enum Strategy
    {
        Deferral,
        Create,
        Overwrite,
        Versionate
    }

    public MsgConsumer()
    {
    }

    public async Task Consume(ConsumeContext<BridgePartialData> context)
    {
        await SaveOnPersistence(context.Message);
    }

    private async Task SaveOnPersistence(BridgePartialData bridgePartialData)
    {

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
                Helpers.FSUtilHelper.MoveQuery(sourcePath, Path.GetDirectoryName(targetPath));
                break;
            case Strategy.Create:
            case Strategy.Overwrite:
                File.Move(sourcePath, targetPath, true);
                break;
            case Strategy.Versionate:
                Helpers.FSUtilHelper.MoveFileUnique(sourcePath, targetPath);
                break;
        }
    }

    private static Strategy DetermineStrategy(string targetPath, HashSet<string> deferredQueryDirs, HashSet<string> nonRestrictedDirs)
    {
        if (deferredQueryDirs.Contains(Path.GetDirectoryName(targetPath))) return Strategy.Deferral;
        if (!File.Exists(targetPath)) return Strategy.Create;

        string directory = Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid.");
        return nonRestrictedDirs != null && nonRestrictedDirs.Contains(directory)
            ? Strategy.Overwrite
            : Strategy.Versionate;
    }
}
