using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using System.Text.Json.Nodes;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace queues.Tests;
public class LocalstackContainerHealthCheck : IWaitUntil
{
    private readonly string _readiness = "/_localstack/init/ready";
    private readonly string _endpoint;

    public LocalstackContainerHealthCheck(string endpoint)
    {
        _endpoint = endpoint;
    }

    public async Task<bool> UntilAsync(IContainer container)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(_endpoint) };
        JsonNode? result;
        try
        {
            result = await httpClient.GetFromJsonAsync<JsonNode>(_readiness);
        }
        catch
        {
            return false;
        }

        if (result is null)
            return false;

        var scripts = result["scripts"];
        if (scripts is null)
            return false;

        foreach (var script in scripts.Deserialize<IEnumerable<Script>>() ?? Enumerable.Empty<Script>())
        {
            if (!"READY".Equals(script.Stage, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!"init.sh".Equals(script.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            return "SUCCESSFUL".Equals(script.State, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public record Script(
        [property: JsonPropertyName("stage")] string Stage,
        [property: JsonPropertyName("state")] string State,
        [property: JsonPropertyName("name")] string Name
        );
}
