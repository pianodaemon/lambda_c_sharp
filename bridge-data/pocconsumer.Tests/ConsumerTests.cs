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
    public void should_verifyStrategiesTest()
    {
        var s3Client = obtainS3Client(_localstackServiceUrl);
        var sqsClient = ConsumerTests.obtainSqsClient(_localstackServiceUrl);

        {      
            string fileKey = "/etc/hosts";
	    string targetPath = "/tmp/hosts_copy.txt";

            placeStuffIntoCloud(fileKey, targetPath, sqsClient, s3Client);

	    var t1 = turnIntoQueueUrl(sqsClient, _testQ);

	    HashSet<string> nonRestrictedDirs = new HashSet<string>
            {
                "/tmp"
            };

	    // Then our expectation is a creation
	    var t2 = startConsuming(t1.Result, _testB, nonRestrictedDirs, sqsClient, s3Client);
            t2.Wait();

	    Assert.True(File.Exists(targetPath), "File could not be created !!");
	}

	{      
            string fileKey = "/etc/services";
	    string targetPath = "/tmp/hosts_copy.txt";

            placeStuffIntoCloud(fileKey, targetPath, sqsClient, s3Client);

	    var t1 = turnIntoQueueUrl(sqsClient, _testQ);

	    HashSet<string> nonRestrictedDirs = new HashSet<string>
            {
                "/tmp"
            };

	    // Then our expectation is a creation
	    var t2 = startConsuming(t1.Result, _testB, nonRestrictedDirs, sqsClient, s3Client);
            t2.Wait();
            string fragment = "Internet style";
	    string text = File.ReadAllText(targetPath);

            // Split the text into lines and get the first line
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            string firstLine = lines.Length > 0 ? lines[0] : string.Empty;

            // Verify the presence of the fragment in the first line
            bool isFragmentPresent = firstLine.Contains(fragment);

	    Assert.True(isFragmentPresent, "Overwrite never occuried !!");
	}

	{      
            string fileKey = "/etc/passwd";
	    string targetPath = "/tmp/hosts_copy.txt";

            placeStuffIntoCloud(fileKey, targetPath, sqsClient, s3Client);

	    var t1 = turnIntoQueueUrl(sqsClient, _testQ);

	    HashSet<string> nonRestrictedDirs = new HashSet<string>
            {
                "/var"
            };

	    // Then our expectation is a versionate
	    var t2 = startConsuming(t1.Result, _testB, nonRestrictedDirs, sqsClient, s3Client);
            t2.Wait();

	   Assert.True(File.Exists($"{targetPath}.1") && File.Exists($"{targetPath}.2"), "Versionate never occuried !!");
	}

    }

    private void placeStuffIntoCloud(string fileKey, string targetPath, AmazonSQSClient sqsClient, AmazonS3Client s3Client)
    {
    	var t = isQueuePresent(sqsClient, _testQ);
        t.Wait();
        Assert.True(t.Result, $"Queue {_testQ} is not present");

 
        FileStream fs = new FileStream(fileKey, FileMode.Open, FileAccess.Read);
        upload(s3Client, _testB, "text/plain", fileKey, fs);

        // Let us push a json message
        var obj = new PartialMsg();
	obj.fileKey = fileKey;
        obj.targetPath = targetPath;
        var t0 = sendObjectAsJson(sqsClient, _testQ, obj);
        t0.Wait();
    }

    private static async Task startConsuming(string queueUrl, string sourceBucket, HashSet<string> nonRestrictedDirs, AmazonSQSClient sqsClient, AmazonS3Client s3Client)
    {
        Consumer consumer = new Consumer(queueUrl, sourceBucket, nonRestrictedDirs, sqsClient, s3Client);
        await consumer.ExtractMessages(MessageHelper.DecodeMessage, StorageHelper.SaveOnPersistence);
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
