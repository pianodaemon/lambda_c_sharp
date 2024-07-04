using MassTransit;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using BridgeDataConsumer.Console.Consumers;

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
            mt.AddConsumer<MsgConsumer>();
            mt.UsingAmazonSqs((ctx, cfg) =>
            {


                cfg.ReceiveEndpoint("queueName", e =>
                {
                    e.DefaultContentType = new ContentType("application/json");
                    e.UseRawJsonSerializer(RawSerializerOptions.AddTransportHeaders | RawSerializerOptions.CopyHeaders);
                    e.ConfigureConsumeTopology = false;
                    e.ConfigureConsumer<MsgConsumer>(ctx);
                });
            });
        });
        return builder.Services;
    }
}
