namespace BridgeDataConsumer.Console.Options;

public class MessageBus
{
    public const string SectionName = "MessageBus";

    public string BucketName { get; init; } = string.Empty;
    public string QueueName { get; init; } = string.Empty;
    public HashSet<string> DeferredQueryDirs { get; init; } = [];
    public HashSet<string> NonRestrictedDirs { get; init; } = [];
}
