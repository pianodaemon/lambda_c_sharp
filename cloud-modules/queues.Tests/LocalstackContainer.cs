using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Configurations;

namespace CloudModules.Tests;
public class LocalstackContainer : IAsyncLifetime
{
    private static readonly string _imageLabel = "localstack/localstack";
    private static readonly int _bindingPort = 4566;
    private static readonly string _provisioning_dir = "./localstack/provisioning";
    private readonly TestcontainersContainer _localstackContainer;

    public int LocalstackPort { get; }
    public string LocalstackUri => $"http://localhost:{LocalstackPort}";
    private static string ToAbsolute(string path) => Path.GetFullPath(path);

    public LocalstackContainer()
    {
        // Randomise the port to prevent port errors
        LocalstackPort = Random.Shared.Next(4000, 5000);
        _localstackContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(_imageLabel)
            .WithCleanUp(true)
            .WithPortBinding(LocalstackPort, _bindingPort)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(_bindingPort)
                .AddCustomWaitStrategy(new LocalstackContainerHealthCheck(LocalstackUri)))
            .WithBindMount(ToAbsolute(_provisioning_dir), "/etc/localstack/init/ready.d", AccessMode.ReadOnly)
            .WithBindMount(ToAbsolute($"{_provisioning_dir}/scripts"), "/scripts", AccessMode.ReadOnly)
            .Build();
    }

    public async Task InitializeAsync()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        await _localstackContainer.StartAsync(cts.Token);
    }

    public async Task DisposeAsync()
    {
        await _localstackContainer.DisposeAsync();
    }
}

