using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;

namespace CloudModules;

public class SecretManagerHelper
{
    private static readonly string AWS_ACCESS_KEY_ID = "AWS_ACCESS_KEY_ID";
    private static readonly string AWS_SECRET_ACCESS_KEY = "AWS_SECRET_ACCESS_KEY";

    private static AmazonSecretsManagerClient ObtainSMClient(string url, string accessKeyId, string secretAccesskey)
    {
        return new AmazonSecretsManagerClient(
			new BasicAWSCredentials(accessKeyId, secretAccesskey),
		       	new AmazonSecretsManagerConfig { ServiceURL = url, UseHttp = true, AuthenticationRegion = "us-east-1" });
    }

    public static ICloudSecretManager InceptFromEnv()
    {
        string secretAccesskey = Environment.GetEnvironmentVariable(AWS_SECRET_ACCESS_KEY);
        string accessKeyId = Environment.GetEnvironmentVariable(AWS_ACCESS_KEY_ID);
        return new SecretManager(ObtainSMClient("http://localhost:4566", accessKeyId, secretAccesskey));
    }
}
