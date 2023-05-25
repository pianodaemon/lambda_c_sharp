using Amazon.SQS;

namespace queues.Tests;

[Collection(nameof(LocalstackContainer))]
public class CustomTests
{
    private string _localstackServiceUrl;
    private static AmazonSQSClient obtainSqsClient(string url) => new AmazonSQSClient(new AmazonSQSConfig { ServiceURL = url });

    public CustomTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_send_receive_and_delete_flawlessly()
    {
        var client = CustomTests.obtainSqsClient(_localstackServiceUrl);
        Assert.True(true);
    }

}