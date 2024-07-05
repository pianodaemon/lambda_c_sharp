using System;
using System.IO;
using System.Collections.Generic;
using BridgeDataConsumer.Console.Models;
using MassTransit;
using Amazon.S3;
using Amazon.S3.Model;

namespace BridgeDataConsumer.Console.Consumers;

public class MsgConsumer : IConsumer<BridgePartialData>
{

    public MsgConsumer()
    {
    }

    public async Task Consume(ConsumeContext<BridgePartialData> context)
    {
    }
}
