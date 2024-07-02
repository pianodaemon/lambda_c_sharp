using Microsoft.Extensions.Hosting;

namespace POCConsumer;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var builder = MassTransitHelper.CreateHostBuilder(args);
            var app = builder.Build();
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
