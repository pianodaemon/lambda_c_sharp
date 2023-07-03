using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.S3.Model;
using System.IO;

namespace CloudModules.Tests;

[Collection(nameof(LocalstackContainer))]
public class BucketTests
{
    private static readonly string SecretKey = "ignore";
    private static readonly string AccessKey = "ignore";
    private static readonly string _testB = "test-bucket";
    private string _localstackServiceUrl;
    private static AmazonS3Client obtainS3Client(string url) => new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonS3Config { ServiceURL = url, UseHttp = true, ForcePathStyle = true, AuthenticationRegion = "us-east-1" });

    public BucketTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_verifyBasicFunctionallity()
    {
        var client = BucketTests.obtainS3Client(_localstackServiceUrl);
        Assert.True(client is not null, "s3 client incorrectly set");
        var b = new Bucket(_testB, client);
        FileStream fs = new FileStream("/etc/services", FileMode.Open, FileAccess.Read);
        b.upload("text/plain","/etc/services_copy.txt", fs);
        var t0 = b.searchItems("etc/ser");
        t0.Wait();
        Assert.True(t0.Result.Count() > 0, "No entries found after uploading file");
        using Stream streamToWriteTo = File.Open("/tmp/services_copy.txt", FileMode.Create);
        var t1 = b.download("/etc/services_copy.txt");
        t1.Wait();
        CopyStream(t1.Result, streamToWriteTo);
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
}
