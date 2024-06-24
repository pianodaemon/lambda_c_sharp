using System;
using System.IO;

namespace POCConsumer;

public static class EmsHelper
{
    static string GetUniqueQueryName()
    {
        string tempDir = Path.GetTempPath();
        string baseFileName = $"query.{Guid.NewGuid().ToString()}";
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
}
