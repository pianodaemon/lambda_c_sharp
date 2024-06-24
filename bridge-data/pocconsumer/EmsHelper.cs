using System;
using System.IO;

namespace POCConsumer;

public static class EmsHelper
{
    private const string QueryStr = "query";
 
    static string GetUniqueQueryName()
    {
        string tempDir = Path.GetTempPath();
        string baseFileName = $"{QueryStr}.{Guid.NewGuid().ToString()}";
        string outFile = Path.Combine(tempDir, baseFileName);
        int counter = 0;

    retry:
        if (File.Exists(outFile))
        {
            counter++;
            outFile = Path.Combine(tempDir, $"{baseFileName}.{counter}");
            goto retry;
        }

        return outFile;
    }

    static void MoveQuery(string tmpFileName, string pendingDir)
    {
        if (string.IsNullOrEmpty(tmpFileName) || string.IsNullOrEmpty(pendingDir))
        {
            return;
        }

        string fileName = Path.GetFileName(tmpFileName);
        if (!fileName.StartsWith($"{QueryStr}."))
        {
            return;
        }

        // Ensure the pending directory exists
        Directory.CreateDirectory(pendingDir);
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
