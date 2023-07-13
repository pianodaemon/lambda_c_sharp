using System.Collections.Generic;

namespace CloudModules;

public interface ICloudBucket
{
    public Task Upload(string cType, string fileName, Stream inputStream);
    public Task<Stream> Download(string fileName);
    public Task<LinkedList<string>> SearchItems(string sPattern);
}
