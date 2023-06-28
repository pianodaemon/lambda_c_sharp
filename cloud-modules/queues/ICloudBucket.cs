namespace CloudModules;

public interface ICloudBucket
{
  public Task upload(string cType, string fileName, Stream inputStream);
}
