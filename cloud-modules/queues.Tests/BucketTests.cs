using Amazon;
using Amazon.S3;
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace CloudModules.Tests;

[Collection(nameof(LocalstackContainer))]
public class BucketTests
{
    private static readonly string SecretKey = "ignore";
    private static readonly string AccessKey = "ignore";
    private static readonly string _testB = "test-bucket";
    private string _localstackServiceUrl;
    private static AmazonS3Client obtainS3Client(string url) => new AmazonS3Client(new BasicAWSCredentials(AccessKey, SecretKey), new AmazonS3Config { ServiceURL = url });

    public BucketTests(LocalstackContainer lsc)
    {
        _localstackServiceUrl = lsc.LocalstackUri;
    }

    [Fact]
    public void should_detectPresenceOfBucketforTest()
    {
        var client = BucketTests.obtainS3Client(_localstackServiceUrl);
        Assert.True(client is not null, "s3 client incorrectly set");
    }
}