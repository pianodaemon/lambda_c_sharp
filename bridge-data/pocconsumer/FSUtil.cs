namespace POCConsumer;

using System.Text.RegularExpressions;

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
        ValidateFilePath(sourcePath);
        RenameFiles(destinationPath);
        if (File.Exists(sourcePath)) File.Move(sourcePath, destinationPath);
    }

    public static void RenameFiles(string filePath)
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
}
