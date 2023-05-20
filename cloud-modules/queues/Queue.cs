using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Lombok.NET;

namespace queues;

[AllArgsConstructor]
public partial class Queue
{
	private static int NUMBER_OF_MESSAGES_REQUESTED = 1;

	private string _queueUrl;
	private IAmazonSQS _sqsClient;

	public Queue(string queueUrl, string accessKeyId, string secretAccessKey) : this(queueUrl, new AmazonSQSClient(accessKeyId, secretAccessKey))
	{

	}

	public async Task<string> send(string messageBody)
	{
		SendMessageResponse responseSendMsg = await _sqsClient.SendMessageAsync(_queueUrl, messageBody);
		return responseSendMsg.MessageId;
	}

	public async Task<string> receive(Action<string> onReceive)
	{
		var req = new ReceiveMessageRequest {
			QueueUrl = _queueUrl,
			MaxNumberOfMessages = NUMBER_OF_MESSAGES_REQUESTED,
		};

		var res = await _sqsClient.ReceiveMessageAsync(req);
		if (res.Messages.Count == 0)
		{
			throw new QueueException("No messages to receive yet", ErrorCodes.NO_MESSAGES_FOUND);
		}

		if (res.Messages.Count != NUMBER_OF_MESSAGES_REQUESTED)
		{
			throw new QueueException("It were received more messages than expected", ErrorCodes.UNKNOWN_FAILURE);
		}

		int slot = NUMBER_OF_MESSAGES_REQUESTED - 1;
		await Task.Run(() => onReceive.Invoke(res.Messages[slot].Body));
		return res.Messages[slot].ReceiptHandle;
	}

	public async Task delete(string receipt)
	{
		await _sqsClient.DeleteMessageAsync(_queueUrl, receipt);
	}
}
