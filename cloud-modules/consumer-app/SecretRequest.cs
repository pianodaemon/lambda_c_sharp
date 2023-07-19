using System;
using System.Text;
using Amazon.SecretsManager;
using CloudModules;

class SecretRequest
{
    public string Buffer { get; set; }

    private SecretRequest(ref string buffer)
    {
        Buffer = buffer;
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
            var t0 = ism.FetchSecretStr(obj.Buffer);
            t0.Wait();
            Console.Write(t0.Result);
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
