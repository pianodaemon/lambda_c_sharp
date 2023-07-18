using System;
using System.Text;
using Amazon.SecretsManager;
using CloudModules;

class SecretRequest
{
    public string SecretId { get; set; }

    private SecretRequest(ref string buffer)
    {
        SecretId = buffer;
    }

    public static SecretRequest parse(string buffer)
    {
        return new SecretRequest(ref buffer);
    }

    public static int consume(SecretRequest obj)
    {
        ICloudSecretManager ism = SecretManagerHelper.InceptFromEnv();

        try
        {
            var t0 = ism.FetchSecretStr(obj.SecretId);
            t0.Wait();
            Console.WriteLine(t0.Result);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ICloudSecretManager error:\n {0}", ex.Message);
            Console.WriteLine("ICloudSecretManager all the exception-related info:\n {0}", ex.ToString());

            // it stands for catching all for general errors
            return 1;
        }

        return 0;
    }
}