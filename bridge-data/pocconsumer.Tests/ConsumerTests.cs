using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace POCConsumer.Tests;

[Collection(nameof(LocalstackContainer))]
public class ConsumerTests
{
    private static readonly string SecretKey = "ignore";
    private static readonly string AccessKey = "ignore";
    private static readonly string _testB = "test-bucket";
    private static readonly string _testQ = "test-queue";
    private string _localstackServiceUrl;
    private static AmazonS3Client obtainS3Client(string url) => new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonS3Config { ServiceURL = url, UseHttp = true, ForcePathStyle = true, AuthenticationRegion = "us-east-1" });
    private static AmazonSQSClient obtainSqsClient(string url) => new AmazonSQSClient(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonSQSConfig { ServiceURL = url });

    public ConsumerTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_detectPresenceOfQueueforTest()
    {
        var client = ConsumerTests.obtainSqsClient(_localstackServiceUrl);
        var t1 = isQueuePresent(client, _testQ);
        t1.Wait();
        Assert.True(t1.Result, $"Queue {_testQ} is not present");
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
