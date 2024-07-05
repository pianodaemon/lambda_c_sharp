using System;
using System.IO;
using System.Collections.Generic;
using BridgeDataConsumer.Console.Models;
using MassTransit;
using Amazon.S3;
using Amazon.S3.Model;
using BridgeDataConsumer.Console.Interfaces;

namespace BridgeDataConsumer.Console.Consumers;

public class MsgConsumer : IConsumer<BridgePartialData>
{
    private readonly ILogger<MsgConsumer> logger;
    private readonly IFileRepository fileRepository;

    public MsgConsumer(ILogger<MsgConsumer> logger, IFileRepository fileRepository)
    {
        this.logger = logger;
        this.fileRepository = fileRepository;
    }

    public async Task Consume(ConsumeContext<BridgePartialData> ctx)
    {
        fileRepository.DownloadAsync(ctx.Message);
    }
}
