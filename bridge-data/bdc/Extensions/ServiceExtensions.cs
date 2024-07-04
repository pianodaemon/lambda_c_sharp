using MassTransit;
using Microsoft.Extensions.Options;
using System.Net.Mime;

namespace BridgeDataConsumer.Console.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddMassTransit();
        return builder.Services;
    }

    public static IServiceCollection AddMassTransit(this WebApplicationBuilder builder)
    {
        builder.Services.AddMassTransit(mt =>
        {
            mt.AddConsumer<BridgeDataConsumer>();
            mt.UsingAmazonSqs((ctx, cfg) =>
            {


                cfg.ReceiveEndpoint("queueName", e =>
                {
                    e.DefaultContentType = new ContentType("application/json");
                    e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                    e.ConfigureConsumeTopology = false;
                    e.ConfigureConsumer<BridgeDataConsumer>(ctx);
                });
            });
        });
        return builder.Services;
    }
}
