using MassTransit;
using BridgeDataConsumer.Console.Options;
using BridgeDataConsumer.Console.Models;

namespace BridgeDataConsumer.Test.Extensions;

public static class ServiceCollectionExtensions {
  public static IServiceCollection AddMassTransitServices(this IServiceCollection services, IConfiguration configuration) {
    var messageBusOptions = configuration.GetSection(MessageBus.SectionName).Get<ConsumptionProperties> () ??
      throw new InvalidOperationException("Missing MessageBus configuration");

    services.AddMassTransit(mt => {
      mt.UsingAmazonSqs((context, cfg) => {
        cfg.UseDefaultHost();
        cfg.Message < MovedToBridgeData > (x => x.SetEntityName(messageBusOptions.QueueName));
        cfg.Publish < MovedToBridgeData > ();
      });
    });

    return services;
  }
}
