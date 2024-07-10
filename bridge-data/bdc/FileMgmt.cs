using BridgeDataConsumer.Console.Helpers;
using BridgeDataConsumer.Console.Interfaces;

namespace BridgeDataConsumer.Console;

public class FileMgmt : IFileMgmt
{
    public void MoveFileUnique(string sourcePath, string destinationPath)
    {
        FSUtilHelper.MoveFileUnique(sourcePath, destinationPath);
    }

    public void MoveQuery(string tmpFileName, string pendingDir)
    {
        FSUtilHelper.MoveQuery(tmpFileName, pendingDir);
    }
}
