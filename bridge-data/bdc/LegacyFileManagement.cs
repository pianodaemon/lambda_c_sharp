using BridgeDataConsumer.Console.Helpers;
using BridgeDataConsumer.Console.Interfaces;

namespace BridgeDataConsumer.Console;

public class LegacyFileManagement(ILogger<LegacyFileManagement> logger, HashSet<string> deferredQueryDirs, HashSet<string> nonRestrictedDirs): IFileManagement
{
    private enum Strategy
    {
        Deferral,
        Create,
        Overwrite,
        Versionate
    }

    public void MoveFileUnique(string sourcePath, string destinationPath)
    {
        FSUtilHelper.MoveFileUnique(sourcePath, destinationPath);
    }

    public void MoveQuery(string tmpFileName, string pendingDir)
    {
        FSUtilHelper.MoveQuery(tmpFileName, pendingDir);
    }

    public void MoveWithOverwrite(string sourcePath, string destinationPath)
    {
        File.Move(sourcePath, destinationPath, true);
    }

    public void ApplyStrategy(string sourcePath, string targetPath)
    {
        var strategy = DetermineStrategy(targetPath);
        logger.LogInformation("Applying a file placement featuring {strategy}", strategy);
        switch (strategy)
        {
            case Strategy.Deferral:
                MoveQuery(sourcePath, Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid."));
                break;
            case Strategy.Create:
            case Strategy.Overwrite:
                MoveWithOverwrite(sourcePath, targetPath);
                break;
            case Strategy.Versionate:
                MoveFileUnique(sourcePath, targetPath);
                break;
        }
    }

    private Strategy DetermineStrategy(string targetPath)
    {
        if (deferredQueryDirs.Contains(Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid.")))
        {
            return Strategy.Deferral;
        }

        if (!File.Exists(targetPath))
        {
            return Strategy.Create;
        }

        string directory = Path.GetDirectoryName(targetPath) ?? throw new InvalidOperationException("Target path is null or invalid.");
        return nonRestrictedDirs != null && nonRestrictedDirs.Contains(directory)
            ? Strategy.Overwrite
            : Strategy.Versionate;
    }
}
