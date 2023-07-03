using System.Collections.Generic;

namespace CloudModules;

public interface ICloudBucket
{
    public Task upload(string cType, string fileName, Stream inputStream);
    public Task<Stream> download(string fileName);
    public Task<LinkedList<string>> searchItems(string sPattern);
}
