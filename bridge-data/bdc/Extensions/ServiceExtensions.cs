using Amazon.S3;
using MassTransit;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using BridgeDataConsumer.Console.Consumers;
using BridgeDataConsumer.Console.Interfaces;
using BridgeDataConsumer.Console.Options;

namespace BridgeDataConsumer.Console.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddMassTransit();
        builder.Services.AddAWSService<IAmazonS3>(builder.Configuration.GetAWSOptions<AmazonS3Config>("AWS"));
        builder.Services.Configure<ConsumptionProperties>(builder.Configuration.GetSection(ConsumptionProperties.SectionName));
        builder.Services.AddSingleton<IFileRepository>(sp => new S3Repository(
            sp.GetRequiredService<ILogger<S3Repository>>(),
            sp.GetRequiredService<IAmazonS3>(),
            sp.GetRequiredService<IOptions<ConsumptionProperties>>().Value.BucketName,
            sp.GetRequiredService<IOptions<ConsumptionProperties>>().Value.DeferredQueryDirs,
            sp.GetRequiredService<IOptions<ConsumptionProperties>>().Value.NonRestrictedDirs
        ));

        return builder.Services;
    }

    public static IServiceCollection AddMassTransit(this WebApplicationBuilder builder)
    {
        var csrcs = builder.Configuration.GetSection(ConsumptionProperties.SectionName).Get<ConsumptionProperties>()
                    ?? throw new InvalidOperationException("Missing sources of consumption in configuration");

        builder.Services.AddMassTransit(mt =>
        {
            mt.AddConsumer<MsgConsumer>();
            mt.UsingAmazonSqs((ctx, cfg) =>
            {
                cfg.UseDefaultHost();
                cfg.ReceiveEndpoint(csrcs.QueueName, e =>
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
