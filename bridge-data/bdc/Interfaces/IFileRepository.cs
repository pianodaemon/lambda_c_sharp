using BridgeDataConsumer.Console.Models;

namespace BridgeDataConsumer.Console.Interfaces;

public interface IFileRepository
{
    Task<string> DoPlacementAsync(MovedToBridgeData movedToBridgeData);
}
