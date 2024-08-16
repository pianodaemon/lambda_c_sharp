using BridgeDataConsumer.Console.Models;
using MassTransit;
using BridgeDataConsumer.Console.Interfaces;

namespace BridgeDataConsumer.Console.Consumers;

public class MsgConsumer(ILogger<MsgConsumer> logger, IFileRepository repo, IFileManagement fileMgmt) : IConsumer<MovedToBridgeData>
{
    public async Task Consume(ConsumeContext<MovedToBridgeData> ctx)
    {
        logger.LogInformation("Consuming message");
        string targetPathDownload = await repo.DoPlacementAsync(ctx.Message);
        fileMgmt.ApplyStrategy(targetPathDownload, ctx.Message.TargetPath);
    }
}
