using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace CloudModules;

public class Bucket : ICloudBucket
{
    IAmazonS3 _s3Client;
    string _target;

    public Bucket(string target, string accessKeyId, string secretAccessKey) : this(target, new AmazonS3Client(accessKeyId, secretAccessKey))
    {

    }

    public Bucket(string target, IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
        _target = target;
    }

    public async Task upload(string cType, string fileName, Stream inputStream)
    {
        var objRequest = new PutObjectRequest()
        {
            BucketName = _target,
            Key = fileName,
            ContentType = cType,
            InputStream = inputStream,
        };

        var res = await _s3Client.PutObjectAsync(objRequest);
    }
}
