namespace CloudModules;

public interface ICloudSecretManager
{
    Task<string> FetchSecretStr(string secretId);
}
