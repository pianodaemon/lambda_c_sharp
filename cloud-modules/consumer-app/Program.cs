using System;
using System.Text;

class PipeStdinCollector
{
    public static void drain(out string buffer)
    {
        StringBuilder sb = new StringBuilder("");
        string? pivot = null;
        int lcounter = 0;
        while ((pivot = Console.ReadLine()) != null)
        {
            if (lcounter > 0) sb.Append("\n");
            sb.Append(pivot);
            lcounter++;
        }


        if (sb.Length > 0)
        {
            buffer = sb.ToString();
            return;
        }

        throw new Exception("Emptyness could not be piped");
    }
}

class Program
{
    static readonly int SLOT_ARG_UC = 0;

    enum Catalog
    {
        BRIDGE_SECRET_ID_REQ,
        BRIDGE_MAX,
    }

    static void Main(string[] args)
    {
        string buffer;
        int rc = 0;
        try
        {
            if (args.Length == 0)
            {
                throw new Exception("Use case has not been asked");
            }

            PipeStdinCollector.drain(out buffer);
            IConsumerBridge[] useCases = new IConsumerBridge[(int)Catalog.BRIDGE_MAX] {
                    new ConsumerBridgeBuilder<SecretRequest>()
                        .setBuffer(ref buffer)
                        .setParser(SecretRequest.parse)
                        .setConsumer(SecretRequest.consume)
                        .build()
            };

            Catalog uc;
            if (!Enum.TryParse<Catalog>(args[SLOT_ARG_UC], out uc))
            {
                string emsg = "Use case " + args[SLOT_ARG_UC] + " is unsupported nowadays";
                throw new Exception(emsg);
            }

            // It stands for the OS exit code
            rc = useCases[(int)uc].engage();
        }
        catch(Exception e)
        {
            Console.WriteLine("Exception caught: {0}", e);
            rc = -1;
        }
        finally
        {
            // It stands for the OS exit code
            Environment.Exit(rc);
        }
    }
}
