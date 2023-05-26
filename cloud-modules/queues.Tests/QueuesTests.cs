using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace queues.Tests;

[Collection(nameof(LocalstackContainer))]
public class QueuesTests
{
    private static readonly string SecretKey = "ignore";
    private static readonly string AccessKey = "ignore";
    private static readonly string _testQ = "test-queue";
    private string _localstackServiceUrl;
    private static AmazonSQSClient obtainSqsClient(string url) => new AmazonSQSClient(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonSQSConfig { ServiceURL = url });

    public QueuesTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_detectPresenceOfTestQueue()
    {
        var client = QueuesTests.obtainSqsClient(_localstackServiceUrl);

        var t0 = isAbscentOfQueues(client);
        t0.Wait();
        Assert.False(t0.Result, "No queues at all");

        var t1 = isQueuePresent(client, _testQ);
        t1.Wait();
        Assert.True(t1.Result, $"Queue {_testQ} is not present");
    }

    private static async Task<bool> isQueuePresent(IAmazonSQS sqsClient, string queueName)
    {
        var res = await sqsClient.GetQueueUrlAsync(queueName);
        return res.QueueUrl.EndsWith(_testQ);
    }

    private static async Task<bool> isAbscentOfQueues(IAmazonSQS sqsClient)
    {
        ListQueuesRequest req = new ListQueuesRequest
        {
            QueueNamePrefix = ""
        };
        ListQueuesResponse responseList = await sqsClient.ListQueuesAsync(req);
        return responseList.QueueUrls.Count == 0;
    }
}
