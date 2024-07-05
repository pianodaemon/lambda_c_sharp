namespace BridgeDataConsumer.Console.Options;

public class ConsumptionSources
{
    public const string SectionName = "ConsumptionSources";

    public string BucketName { get; init; } = "";
    public string QueueName { get; init; } = "";
}
