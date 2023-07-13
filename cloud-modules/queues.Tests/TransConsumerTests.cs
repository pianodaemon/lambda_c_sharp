using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;

namespace CloudModules.Tests;

[Collection(nameof(LocalstackContainer))]
public class TransConsumerTests
{
    private static readonly string SecretKey = "ignore";
    private static readonly string AccessKey = "ignore";
    private static readonly string _testB = "test-transconsumer-bucket";
    private static readonly string _testQ = "test-transconsumer-queue";
    private string _localstackServiceUrl;
    private static AmazonS3Client obtainS3Client(string url) => new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonS3Config { ServiceURL = url, UseHttp = true, ForcePathStyle = true, AuthenticationRegion = "us-east-1" });
    private static AmazonSQSClient obtainSqsClient(string url) => new AmazonSQSClient(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonSQSConfig { ServiceURL = url });

    public TransConsumerTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_transconsumeCorrectly()
    {
        var b = new Bucket(_testB, obtainS3Client(_localstackServiceUrl));
        FileStream fs = new FileStream("/etc/hosts", FileMode.Open, FileAccess.Read);
        b.Upload("text/plain","/etc/hosts_copy.txt", fs);

        var tz = isQueuePresent(obtainSqsClient(_localstackServiceUrl), _testQ);
        tz.Wait();
        Assert.True(tz.Result, $"Queue {_testQ} is not present");

        Func<MetaMsg, ReturnMock?> transHandler = (tpo) =>
        {
            var rmo = new ReturnMock();
            rmo.Text = tpo.BucketObjKey;
            using Stream streamToWriteTo = File.Open("/tmp/hosts_copy.txt", FileMode.Create);
            var td = b.Download("/etc/hosts_copy.txt");
            td.Wait();
            CopyStream(td.Result, streamToWriteTo);
            return rmo;
        };

        ICloudQueue<MetaMsg> q = obtainSteadyQueue4Test<MetaMsg>(_localstackServiceUrl, _testQ);

        // Let us push a json message
        var obj = new MetaMsg();
        {
            obj.BucketObjKey = "/etc/hosts_copy.txt";
            var t0 = q.SendObjectAsJson(obj);
            t0.Wait();
        }

        var list = TransConsumer.DoConsume<MetaMsg, ReturnMock>(transHandler, q);
        Assert.True(obj.BucketObjKey == list.First().Text, "Unexpected text as result");
    }

    private static void CopyStream(Stream input, Stream output)
    {
        byte[] buffer = new byte[1<<12];
        int len;
        while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, len);
        }
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

public class MetaMsg
{
    public string? BucketObjKey { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as MetaMsg;

        if (item == null || this.BucketObjKey == null)
        {
            return false;
        }

        return this.BucketObjKey.Equals(item.BucketObjKey);
    }

    public override int GetHashCode()
    {
        return this.BucketObjKey != null ? this.BucketObjKey.GetHashCode() : 0;
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
