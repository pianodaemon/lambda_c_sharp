using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace CloudModules;

public class SecretManager : ICloudSecretManager
{
    private IAmazonSecretsManager _smClient;

    public SecretManager(string accessKeyId, string secretAccessKey) : this(new AmazonSecretsManagerClient(accessKeyId, secretAccessKey))
    {

    }

    private SecretManager(AmazonSecretsManagerClient smClient)
    {
       _smClient = smClient;
    }

    public async Task<string> FetchSecretStr(string secretId)
    {
        GetSecretValueRequest req = new();
        req.SecretId = secretId;

        GetSecretValueResponse? response = null;
        try
        {
            response = await _smClient.GetSecretValueAsync(req);
        }
        catch (AmazonSecretsManagerException ex)
        {
            throw new CloudModuleException("Failure when obtaining a secret value response", ex, ErrorCodes.SECRET_FAILURE_VAL_RES);
        }

        var secret = DecodeString(response);
        if (string.IsNullOrEmpty(secret))
        {
            throw new CloudModuleException("It was decoded a null or empty secret", ErrorCodes.SECRET_FAILURE_VAL_RES);
        }

        return secret;
    }

    private static string DecodeString(GetSecretValueResponse? res)
    {
        if (res is null)
        {
            throw new CloudModuleException("Secret value response can not be null", ErrorCodes.SECRET_FAILURE_VAL_RES);
        }

        // Decrypts secret using the associated AWS Key Management Service
        // Customer Master Key (CMK.) Depending on whether the secret is a
        // string or binary value, one of these fields will be populated.
        MemoryStream memoryStream = new();

        if (res.SecretString is not null)
        {
            var secret = res.SecretString;
            return secret;
        }
        else if (res.SecretBinary is not null)
        {
            memoryStream = res.SecretBinary;
            StreamReader reader = new StreamReader(memoryStream);
            string decodedBinarySecret = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
            return decodedBinarySecret;
        }
        else
        {
            throw new CloudModuleException("Unexpected secret value response", ErrorCodes.SECRET_FAILURE_VAL_RES);
        }
    }
}
