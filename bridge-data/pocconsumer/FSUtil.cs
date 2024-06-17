namespace POCConsumer;

using System.Text.RegularExpressions;

class FSUtilHelper
{
    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new InvalidOperationException($"File path {filePath} is not valid.");
        }
    }

    public static void RenameFileIfExists(string filePath)
    {
        ValidateFilePath(filePath);

        try
        {
            string directoryPath = Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("Directory path is invalid.");
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            // Logic to rename the file if already exists with postfix .1, .2, .3 and so on
            // For ex: If test.txt already exists then rename it to test.1.txt
            int digit;
            string prefix;

            var match = Regex.Match(fileNameWithoutExtension, @"^(.+)\.(\d+)$");
            if (match.Success)
            {
                prefix = match.Groups[1].Value;
                digit = int.Parse(match.Groups[2].Value) + 1;

            }
            else
            {
                prefix = fileNameWithoutExtension;
                digit = 0;
            }

            digit++;

            string newFileName = $"{prefix}.{digit}{fileExtension}";
            if (File.Exists(Path.Combine(directoryPath, newFileName)))
            {
                RenameFileIfExists(filePath);
            }
            // Rename the file       
            File.Move(filePath, Path.Combine(directoryPath, newFileName));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error renaming file: {ex.Message}");
            throw;
        }
    }
}
