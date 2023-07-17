using System;
using System.Text;

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
        Console.WriteLine(obj.SecretId);
        return 0;
    }
}
