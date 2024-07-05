using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using BridgeDataConsumer.Console.Models;
using BridgeDataConsumer.Console.Helpers;
using BridgeDataConsumer.Console.Interfaces;

namespace BridgeDataConsumer.Console;

public class S3Repository : IFileRepository
{
    private enum Strategy
    {
        Deferral,
        Create,
        Overwrite,
        Versionate
    }

    private readonly IAmazonS3 s3Client;
    private readonly string sourceBucket;
    private readonly HashSet<string> deferredQueryDirs;
    private readonly HashSet<string> nonRestrictedDirs;

    S3Repository(IAmazonS3 s3Client, string sourceBucket, HashSet<string> deferredQueryDirs, HashSet<string> nonRestrictedDirs)
    {
        this.s3Client = s3Client;
        this.sourceBucket = sourceBucket;
        this.deferredQueryDirs = deferredQueryDirs;
        this.nonRestrictedDirs = nonRestrictedDirs;
    }

    public async Task DownloadAsync(BridgePartialData bridgePartialData)
    {
        try
        {
            string targetPathDownload = $"{bridgePartialData.TargetPath}.download";
            Directory.CreateDirectory(Path.GetDirectoryName(targetPathDownload) ?? throw new InvalidOperationException("Target path is null or invalid."));

            await Fetch(bridgePartialData.FileKey.TrimStart('/'), targetPathDownload);
            ApplyStrategy(targetPathDownload, bridgePartialData.TargetPath);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task Fetch(string key, string downloadPath)
    {
        var getObjectRequest = new GetObjectRequest { BucketName = sourceBucket, Key = key };
        using var response = await s3Client.GetObjectAsync(getObjectRequest);
        await response.WriteResponseStreamToFileAsync(downloadPath, false, default);
    }

    private void ApplyStrategy(string sourcePath, string targetPath)
    {
        switch (DetermineStrategy(targetPath))
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

    private Strategy DetermineStrategy(string targetPath)
    {
        if (deferredQueryDirs.Contains(Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid."))) return Strategy.Deferral;
        if (!File.Exists(targetPath)) return Strategy.Create;

        string directory = Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid.");
        return nonRestrictedDirs != null && nonRestrictedDirs.Contains(directory)
            ? Strategy.Overwrite
            : Strategy.Versionate;
    }
}