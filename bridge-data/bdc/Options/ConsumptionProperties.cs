namespace BridgeDataConsumer.Console.Options;

public class ConsumptionProperties
{
    public const string SectionName = "ConsumptionProperties";

    public string BucketName { get; init; } = "";
    public string QueueName { get; init; } = "";
    public HashSet<string> DeferredQueryDirs { get; init; } = new HashSet<string>();
    public HashSet<string> NonRestrictedDirs { get; init; } = new HashSet<string>();
}
