namespace POCConsumer;

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

public static class StorageHelper
{
    enum Strategy
    {
        Create,
        Overwrite,
        Versionate
    }

    public static async Task SaveOnPersistence(AmazonS3Client s3Client, string sourceBucket, HashSet<string> nonRestrictedDirs, BridgePartialData bridgePartialData)
    {
        try
        {
            GetObjectRequest getObjectRequest = new()
            {
                BucketName = sourceBucket,
                Key = bridgePartialData.FileKey
            };

            string targetPath = $"{bridgePartialData.TargetPath}.download";
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid."));
            using GetObjectResponse response = await s3Client.GetObjectAsync(getObjectRequest);
            await response.WriteResponseStreamToFileAsync(targetPath, false, default);
            switch(DetermineStrategy(bridgePartialData.TargetPath, nonRestrictedDirs))
            {
                case Strategy.Create:
                case Strategy.Overwrite:
                    File.Move(targetPath, bridgePartialData.TargetPath, true);
                    break;
                case Strategy.Versionate:
                    FSUtilHelper.MoveFileUnique(targetPath, bridgePartialData.TargetPath);
                    break;
            }
            Console.WriteLine($"File downloaded to {bridgePartialData.TargetPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading or saving file: {ex.Message}");
            throw;
        }
    }

    private static Strategy DetermineStrategy(string targetPath, HashSet<string> nonRestrictedDirs)
    {
        if (!File.Exists(targetPath)) return Strategy.Create;

        string directory = Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid.");
        if (nonRestrictedDirs != null && nonRestrictedDirs.Contains(directory))
        {
            Console.WriteLine($"File {targetPath} already exists in a non-restricted directory. It'll be Overwritten.");
            return Strategy.Overwrite;
        }

        Console.WriteLine($"File {targetPath} already exists. It'll be versionated.");
        return Strategy.Versionate;
    }
}
