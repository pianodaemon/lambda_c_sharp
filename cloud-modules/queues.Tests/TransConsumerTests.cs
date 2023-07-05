using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace CloudModules.Tests;

[Collection(nameof(LocalstackContainer))]
public class TransConsumerTests
{
    private static readonly string SecretKey = "ignore";
    private static readonly string AccessKey = "ignore";
    private static readonly string _testQ = "test-transconsumer-queue";
    private string _localstackServiceUrl;
    private static AmazonSQSClient obtainSqsClient(string url) => new AmazonSQSClient(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonSQSConfig { ServiceURL = url });

    public TransConsumerTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_transconsumeCorrectly()
    {
        var tz = isQueuePresent(obtainSqsClient(_localstackServiceUrl), _testQ);
        tz.Wait();
        Assert.True(tz.Result, $"Queue {_testQ} is not present");

        Func<PlainObjectMock, ReturnMock> transHandler = (tpo) =>
        {
            var rmo = new ReturnMock();
            rmo.Text = tpo.Text;
            return rmo;
        };

        ICloudQueue<PlainObjectMock> q = obtainSteadyQueue4Test<PlainObjectMock>(_localstackServiceUrl, _testQ);

        // Let us push a json message
        var obj = new PlainObjectMock();
        {
            obj.Text = "Be here now";
            var t0 = q.sendObjectAsJson(obj);
            t0.Wait();
        }

        var list = TransConsumer.DoConsume<PlainObjectMock, ReturnMock>(transHandler, q);
        Assert.True(obj.Text == list.First().Text, "Unexpected text as result");
    }

    private static JsonifiedQueue<T> obtainSteadyQueue4Test<T>(string lss, string queueName)
    {
         var sqsClient = obtainSqsClient(lss);
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
}

public class PlainObjectMock
{
    public string? Text { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as PlainObjectMock;

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

public class ReturnMock
{
    public string? Text { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as ReturnMock;

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
