namespace POCConsumer;

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

public static class StorageHelper
{
    public static async Task SaveOnPersistence(AmazonS3Client s3Client, string sourceBucket, HashSet<string> nonRestrictedDirs, BridgePartialData bridgePartialData)
    {
        try
        {
            ValidateTargetPath(bridgePartialData.TargetPath, nonRestrictedDirs);
            Directory.CreateDirectory(Path.GetDirectoryName(bridgePartialData.TargetPath) ?? throw new InvalidOperationException("Target path is null or invalid."));
            await Fetch(s3Client, sourceBucket, bridgePartialData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading or saving file: {ex.Message}");
            throw;
        }
    }

    private static async Task Fetch(AmazonS3Client s3Client, string sourceBucket, BridgePartialData bridgePartialData)
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

    private static void ValidateTargetPath(string targetPath, HashSet<string> nonRestrictedDirs)
    {
        if (File.Exists(targetPath))
        {
            string directory = Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid.");
            if (nonRestrictedDirs != null && nonRestrictedDirs.Contains(directory))
            {
                Console.WriteLine($"File {targetPath} already exists in a permissible directory. Overwriting allowed.");
                return;
            }

            throw new InvalidOperationException($"File {targetPath} already exists. Skipping download.");
        }
    }

    private static void RenameFiles(string filename)
    {
        if (!File.Exists(filename))
        {
            return;
        }

        string prefix;
        int digits;
        var regex = new Regex(@"^(.+)\.(\d+)$");
        var match = regex.Match(filename);

        if (match.Success)
        {
            prefix = match.Groups[1].Value;
            digits = int.Parse(match.Groups[2].Value);
        }
        else
        {
            prefix = filename;
            digits = 0;
        }

        digits++;
        string newname = $"{prefix}.{digits}";

        if (File.Exists(newname))
        {
            RenameFiles(newname);
        }

        File.Move(filename, newname);
    }
}
