using BridgeDataConsumer.Console.Helpers;
using BridgeDataConsumer.Console.Interfaces;

namespace BridgeDataConsumer.Console;

public class LegacyFileMgmt : IFileMgmt
{
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
}
