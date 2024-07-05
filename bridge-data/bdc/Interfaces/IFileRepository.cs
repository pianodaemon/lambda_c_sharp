using BridgeDataConsumer.Console.Models;

namespace BridgeDataConsumer.Console.Interfaces;

public interface IFileRepository
{
    Task DownloadAsync(BridgePartialData bridgePartialData);
}
