using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace CloudModules.Tests;

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
    public void should_detectPresenceOfQueueforTest()
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
    public void should_abideWithTheHigherExpectations()
    {
        ICloudQueue<TextPlainObj> q = obtainSteadyQueue4Test<TextPlainObj>(_localstackServiceUrl, _testQ);

        // Expecting purge mechanism function correctly
        {
            const short element2incept = 100;
            Random rnd = new Random();
            for (int j = 0; j < element2incept; j++)
            {
                var t1 = q.send(rnd.Next().ToString());
                t1.Wait();
            }
            q.purge().Wait();
        }

        // Expecting to find nothing at the queue for test
        {
            Action<string> actOnReceiveHandler = (payload) =>
            {
                Assert.Fail("Why have we reached this execution point ??");
            };

            try
            {
                q.receive(actOnReceiveHandler).Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle((ex) =>
                {
                    if (ex is CloudModuleException) // This we know how to handle.
                    {
                        //It must be handled as expected
                        return true;
                    }
                    return false;
               });
            }
        }

        // Expecting to move a json message back and forth and deletion
        {
            var obj = new TextPlainObj();
            obj.Text = "Welcome to the jungle";
            var t0 = q.sendAsJson(obj);
            t0.Wait();
            Action<TextPlainObj> actOnReceiveHandler = (tpo) =>
            {
                Assert.True(obj.Equals(tpo), "How did we not receive what we sent ??");
            };
            const short delay2receive = 1;
            var t1 = q.receiveAsJson(actOnReceiveHandler, delay2receive);
            t1.Wait();
            q.delete(t1.Result).Wait();
        }
    }

    private static JsonifiedQueue<T> obtainSteadyQueue4Test<T>(string lss, string queueName)
    {
         var sqsClient = QueueTests.obtainSqsClient(lss);
         var t0 = turnIntoQueueUrl(sqsClient, queueName);
         t0.Wait();
         return new JsonifiedQueue<T>(t0.Result, sqsClient);
    }

    private static async Task<string> turnIntoQueueUrl(IAmazonSQS sqsClient, string queueName)
    {
        var res = await sqsClient.GetQueueUrlAsync(queueName);
        return res.QueueUrl;
    }

    private static async Task<bool> isQueuePresent(IAmazonSQS sqsClient, string queueName)
    {
        var res = await turnIntoQueueUrl(sqsClient, queueName);
        return res.EndsWith(_testQ);
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

public class TextPlainObj
{
	public string? Text { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as TextPlainObj;

        if (item == null || this.Text == null)
        {
            return false;
        }

        return this.Text.Equals(item.Text);
    }

    public override int GetHashCode()
    {
        return this.Text != null ? this.Text.GetHashCode() : 0;
    }
}