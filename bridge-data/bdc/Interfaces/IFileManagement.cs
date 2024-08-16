namespace BridgeDataConsumer.Console.Interfaces;

public interface IFileManagement
{
    void MoveFileUnique(string sourcePath, string destinationPath);
    void MoveQuery(string tmpFileName, string pendingDir);
    void MoveWithOverwrite(string tmpFileName, string pendingDir);
    void ApplyStrategy(string sourcePath, string targetPath);
}
