using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace CloudModules;

public class BasicQueue
{
    private static int NUMBER_OF_MESSAGES_REQUESTED = 1;

    private string _queueUrl;
    private AmazonSQSClient _sqsClient;

    public BasicQueue(string queueUrl, AmazonSQSClient sqsClient)
    {
        _queueUrl = queueUrl;
        _sqsClient = sqsClient;
    }

    public BasicQueue(string queueUrl, string accessKeyId, string secretAccessKey): this(queueUrl, new AmazonSQSClient(accessKeyId, secretAccessKey))
    {

    }

    public async Task<string> send(string messageBody)
    {
        SendMessageResponse responseSendMsg = await _sqsClient.SendMessageAsync(_queueUrl, messageBody);
        return responseSendMsg.MessageId;
    }

    public async Task<string> receive(Action <string> onReceive, short delay)
    {
        var req = new ReceiveMessageRequest {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = NUMBER_OF_MESSAGES_REQUESTED,
            WaitTimeSeconds = delay,
        };

        var res = await _sqsClient.ReceiveMessageAsync(req);
        if (res.Messages.Count == 0)
       	{
            throw new CloudModuleException("No messages to receive yet", ErrorCodes.NO_MESSAGES_FOUND_IN_QUEUE);
        }

        if (res.Messages.Count != NUMBER_OF_MESSAGES_REQUESTED)
       	{
            throw new CloudModuleException("It were received more messages than expected", ErrorCodes.UNKNOWN_FAILURE);
        }

        int slot = NUMBER_OF_MESSAGES_REQUESTED - 1;
        await Task.Run(() => onReceive.Invoke(res.Messages[slot].Body));
        return res.Messages[slot].ReceiptHandle;
    }

    public async Task delete(string receipt)
    {
        await _sqsClient.DeleteMessageAsync(_queueUrl, receipt);
    }

    public async Task purge()
    {
        PurgeQueueResponse res = await _sqsClient.PurgeQueueAsync(_queueUrl);
    }
}
