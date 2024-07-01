namespace POCConsumer;

using Amazon;

class Program
{
    static async Task Main(string[] args)
    {
        HashSet<string> overwritePermissibleDirectories = new HashSet<string>
        {
            "/path/to/dir2"
        };

        string queueName = "my-queue";
        string sourceBucket = "my-bucket";
        Console.WriteLine($"Starting to consume messages from SQS queue: {queueName} and source bucket: {sourceBucket}...");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            MassTransitHelper.setupService(builder.Services, "secretKey", "accessKey",
                              RegionEndpoint.USEast1 , queueName, sourceBucket,
                              overwritePermissibleDirectories, StorageHelper.SaveOnPersistence);

            var app = builder.Build();
            //app.MapDefaultEndpoints();
            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Application terminated unexpectedly.\n{ex.ToString()}");
        }
        finally
        {
            Console.WriteLine("Application is shutting down...");
        }
    }
}
