using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Collections.Generic;

namespace CloudModules;

public class Bucket : ICloudBucket
{
    private IAmazonS3 _s3Client;
    private readonly string _target;

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

    public async Task<Stream> download(string fileName)
    {
        var res = await _s3Client.GetObjectAsync(_target, fileName);
        return res.ResponseStream;
    }

    public async Task<LinkedList<string>> searchItems(string sPattern)
    {
        LinkedList<string> itemsFound = new LinkedList<string>();

        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _target,
                Prefix = sPattern,
            };

            ListObjectsV2Response response;
            do
            {
                response = await _s3Client.ListObjectsV2Async(request);

                response.S3Objects
                        .ForEach(obj => itemsFound.AddLast($"{obj.Key,-35}"));

                // If the response is truncated, set the request ContinuationToken
                // from the NextContinuationToken property of the response.
                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' getting list of objects.");
        }

        return itemsFound;
    }
}
