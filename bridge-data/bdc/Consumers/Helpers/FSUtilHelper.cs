using System.Text.RegularExpressions;

namespace BridgeDataConsumer.Console.Consumers.Helpers;

static class FSUtilHelper
{
    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new InvalidOperationException($"File path {filePath} is not valid.");
        }
    }

    public static void MoveFileUnique(string sourcePath, string destinationPath)
    {
        RenameFiles(destinationPath);
        if (File.Exists(sourcePath)) File.Move(sourcePath, destinationPath);
    }

    private static void RenameFiles(string filePath)
    {
        ValidateFilePath(filePath);

        string prefix;
        int digits;
        Regex regex = new Regex(@"^(.+)\.(\d+)$");
        Match match = regex.Match(filePath);

        if (match.Success)
        {
            prefix = match.Groups[1].Value;
            digits = int.Parse(match.Groups[2].Value);
        }
        else
        {
            prefix = filePath;
            digits = 0;
        }

        digits++;
        string newname = $"{prefix}.{digits}";

        if (File.Exists(newname))RenameFiles(newname);
        File.Move(filePath, newname);
    }

    public static void MoveQuery(string tmpFileName, string? pendingDir)
    {
        if (string.IsNullOrEmpty(tmpFileName))
        {
            throw new ArgumentException("Temporary file name must not be null or empty.", nameof(tmpFileName));
        }

        if (string.IsNullOrEmpty(pendingDir))
        {
            throw new ArgumentException("Pending directory must not be null or empty.", nameof(pendingDir));
        }

        string queryStr = "query";
        string fileName = Path.GetFileName(tmpFileName);
        if (!fileName.StartsWith($"{queryStr}."))
        {
            throw new InvalidOperationException("The file name must start with 'query.'");
        }

        string downloadExtension = ".download";
        if (fileName.EndsWith(downloadExtension)) {
            fileName = fileName.Substring(0, fileName.Length - downloadExtension.Length);
        }

        string destFileName = Path.Combine(pendingDir, fileName);

        int counter = 0;
    retry:
        if (File.Exists(destFileName))
        {
            counter++;
            destFileName = Path.Combine(pendingDir, $"{fileName}.{counter}");
            goto retry;
        }

        File.Move(tmpFileName, destFileName, true);
    }
}
