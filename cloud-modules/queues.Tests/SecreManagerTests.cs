using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;

namespace CloudModules.Tests;

[Collection(nameof(LocalstackContainer))]
public class SecretManagerTests
{
    private static readonly string SecretKey = "ignore";
    private static readonly string AccessKey = "ignore";
    private static readonly string _testS = "sheldon-cooper-says";
    private string _localstackServiceUrl;
    private static AmazonSecretsManagerClient obtainSMClient(string url) => new AmazonSecretsManagerClient(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonSecretsManagerConfig { ServiceURL = url, UseHttp = true, AuthenticationRegion = "us-east-1" });

    public SecretManagerTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_verifyBasicFunctionallity()
    {
        var client = SecretManagerTests.obtainSMClient(_localstackServiceUrl);
        Assert.True(client is not null, "secret manager client incorrectly set");

	ICloudSecretManager ism = new SecretManager(client);
	var t0 = ism.FetchSecretStr(_testS);
	t0.Wait();
	Assert.True(t0.Result.Equals("bazinga"), "You do not really know Sheldon");
    }
}
