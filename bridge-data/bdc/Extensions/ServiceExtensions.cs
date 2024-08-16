using Amazon.S3;
using MassTransit;
using Microsoft.Extensions.Options;
using BridgeDataConsumer.Console.Consumers;
using BridgeDataConsumer.Console.Interfaces;
using BridgeDataConsumer.Console.Options;

namespace BridgeDataConsumer.Console.Extensions;

internal static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAWSService<IAmazonS3>(builder.Configuration.GetAWSOptions<AmazonS3Config>("AWS"));
        builder.Services.Configure<MessageBus>(builder.Configuration.GetSection(MessageBus.SectionName));
        builder.Services.AddSingleton<IFileRepository>(sp => new S3Repository(
            sp.GetRequiredService<IAmazonS3>(),
            sp.GetRequiredService<IOptions<MessageBus>>().Value.BucketName
        ));
        builder.Services.AddSingleton<IFileManagement>(sp => new LegacyFileManagement(
            sp.GetRequiredService<ILogger<LegacyFileManagement>>(),
            sp.GetRequiredService<IOptions<MessageBus>>().Value.DeferredQueryDirs,
            sp.GetRequiredService<IOptions<MessageBus>>().Value.NonRestrictedDirs
        )); 
        builder.AddMassTransit();

        return builder.Services;
    }

    public static IServiceCollection AddMassTransit(this WebApplicationBuilder builder)
    {
        var csrcs = builder.Configuration.GetSection(MessageBus.SectionName).Get<MessageBus>()
                    ?? throw new InvalidOperationException("Missing sources of consumption in configuration");

        builder.Services.AddMassTransit(mt =>
        {
            mt.AddConsumer<MsgConsumer>();
            mt.UsingAmazonSqs((ctx, cfg) =>
            {
                cfg.UseDefaultHost();
                cfg.ReceiveEndpoint(csrcs.QueueName, e =>
                {
                    e.ConfigureConsumeTopology = false;
                    e.ThrowOnSkippedMessages();
                    e.RethrowFaultedMessages();
                    e.ConfigureConsumer<MsgConsumer>(ctx);
                });
            });
        });
        return builder.Services;
    }
}
