using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace queues.Tests;

[Collection(nameof(LocalstackContainer))]
public class QueueTests
{
    private static readonly string SecretKey = "ignore";
    private static readonly string AccessKey = "ignore";
    private static readonly string _testQ = "test-queue";
    private string _localstackServiceUrl;
    private static AmazonSQSClient obtainSqsClient(string url) => new AmazonSQSClient(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonSQSConfig { ServiceURL = url });

    public QueueTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_detectPresenceOfTestQueue()
    {
        var client = QueueTests.obtainSqsClient(_localstackServiceUrl);

        var t0 = isAbscentOfQueues(client);
        t0.Wait();
        Assert.False(t0.Result, "No queues at all");

        var t1 = isQueuePresent(client, _testQ);
        t1.Wait();
        Assert.True(t1.Result, $"Queue {_testQ} is not present");
    }

    [Fact]
    public void should_sendAndReceive()
    {
         var q = obtainSteadyQueue4Test();
         string msgBody = "Hello world";
         var t1 = q.send(msgBody);
         t1.Wait();
         Action<string> actOnReceiveHandler = (payload) =>
         {
            Assert.True(msgBody.Equals(payload), $"How did we not receive what we sent ??");
         };
         q.receive(actOnReceiveHandler).Wait();
    }

    private Queue obtainSteadyQueue4Test()
    {
         var sqsClient = QueueTests.obtainSqsClient(_localstackServiceUrl);
         var t0 = turnIntoQueueUrl(sqsClient, _testQ);
         t0.Wait();
         return new Queue(t0.Result, sqsClient);
    }

    private static async Task<string> turnIntoQueueUrl(IAmazonSQS sqsClient, string queueName)
    {
        var res = await sqsClient.GetQueueUrlAsync(queueName);
        return res.QueueUrl;
    }

    private static async Task<bool> isQueuePresent(IAmazonSQS sqsClient, string queueName)
    {
        return turnIntoQueueUrl(sqsClient, queueName).Result.EndsWith(_testQ);
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