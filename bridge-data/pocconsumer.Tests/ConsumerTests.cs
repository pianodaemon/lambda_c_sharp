using System.IO;
using System.Text.Json;
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
        var sqsClient = ConsumerTests.obtainSqsClient(_localstackServiceUrl);
        var t = isQueuePresent(sqsClient, _testQ);
        t.Wait();
        Assert.True(t.Result, $"Queue {_testQ} is not present");

       
        string fileKey = "/etc/hosts";
	string targetPath = "/nfs_volume/etc/hosts_copy.txt";


        var s3Client = obtainS3Client(_localstackServiceUrl);
        FileStream fs = new FileStream(fileKey, FileMode.Open, FileAccess.Read);
        upload(s3Client, _testB, "text/plain", fileKey, fs);

        // Let us push a json message
        var obj = new PartialMsg();
	obj.fileKey = fileKey;
        obj.targetPath = targetPath;
        var t0 = sendObjectAsJson(sqsClient, _testQ, obj);
        t0.Wait();

	var t1 = turnIntoQueueUrl(sqsClient, _testQ);
        t1.Wait();
        var req = new ReceiveMessageRequest {
            QueueUrl = t1.Result,
            MaxNumberOfMessages = 10,
            WaitTimeSeconds = 1,
        };

        var t2 = sqsClient.ReceiveMessageAsync(req);
	t2.Wait();
	var res = t2.Result;
	string jsonMsg = res.Messages[0].Body;
	var pm = JsonSerializer.Deserialize<PartialMsg>(jsonMsg);
        Assert.True(pm.fileKey == fileKey, "UPS!!!");
	Assert.True(pm.targetPath == targetPath, "UPS!!!");
    }

    public static async Task upload(IAmazonS3 s3Client, string target, string cType, string fileName, Stream inputStream)
    {
        var objRequest = new PutObjectRequest()
        {
            BucketName = target,
            Key = fileName,
            ContentType = cType,
            InputStream = inputStream,
        };

        var res = await s3Client.PutObjectAsync(objRequest);
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

    public static async Task<string> send(IAmazonSQS sqsClient, string queueName, string messageBody)
    {
        var t = turnIntoQueueUrl(sqsClient, queueName);
        t.Wait();
        SendMessageResponse responseSendMsg = await sqsClient.SendMessageAsync(t.Result, messageBody);
        return responseSendMsg.MessageId;
    }

    public static async Task<string> sendObjectAsJson<T>(IAmazonSQS sqsClient, string queueName, T obj)
    {
        return await send(sqsClient, queueName, JsonSerializer.Serialize(obj));
    }
}

public class PartialMsg
{
    public string? targetPath { get; set; }
    public string? fileKey { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as PartialMsg;

        if (item == null || this.targetPath == null || this.fileKey == null )
        {
            return false;
        }

        return this.fileKey.Equals(item.fileKey) && this.targetPath.Equals(item.targetPath) ;
    }

    public override int GetHashCode()
    {
        return this.targetPath != null && this.fileKey != null ? (this.targetPath + this.fileKey).GetHashCode() : 0;
    }
}
