namespace BridgeDataConsumer.Console.Options;

public class ConsumptionProperties
{
    public const string SectionName = "ConsumptionProperties";

    public string BucketName { get; init; } = "";
    public string QueueName { get; init; } = "";
    public HashSet<string> DeferredQueryDirs { get; set; } = new HashSet<string>();
    public HashSet<string> NonRestrictedDirs { get; set; } = new HashSet<string>();
}
