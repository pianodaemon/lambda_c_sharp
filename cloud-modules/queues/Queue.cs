using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace queues;

public partial class Queue
{
	private static int NUMBER_OF_MESSAGES_REQUESTED = 1;
	private static int DELAY_SECS_TO_RECEIVE = 1;

	private string _queueUrl;
	private AmazonSQSClient _sqsClient;

	public Queue(string queueUrl, AmazonSQSClient sqsClient)
	{
		_queueUrl = queueUrl;
		_sqsClient = sqsClient;
	}

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
			WaitTimeSeconds = DELAY_SECS_TO_RECEIVE,
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

public partial class QueueJsonified<T> : Queue
{
	public QueueJsonified(string queueUrl, AmazonSQSClient sqsClient) : base(queueUrl, sqsClient)
	{

	}

	public async Task<string> sendAsJson(T obj)
	{
		return await send(JsonSerializer.Serialize(obj));
	}

	public async Task<string> receiveAsJson(Action<T> onReceive)
	{
		Action<string> onReceiveWrapper = (jsonMsg) =>
		{
			onReceive(JsonSerializer.Deserialize<T>(jsonMsg));
		};
		return await receive(onReceiveWrapper);
	}
}