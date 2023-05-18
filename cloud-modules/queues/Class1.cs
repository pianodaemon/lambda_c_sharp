using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Lombok.NET;

namespace queues;

[AllArgsConstructor]
public partial class Class1
{
	private string _queueUrl;
	private IAmazonSQS _sqsClient;

	public Class1(string queueUrl, string accessKeyId, string secretAccessKey) : this(queueUrl, new AmazonSQSClient(accessKeyId, secretAccessKey))
	{

	}

	public async Task<string> send(string messageBody)
	{
		SendMessageResponse responseSendMsg = await _sqsClient.SendMessageAsync(_queueUrl, messageBody);
		return responseSendMsg.MessageId;
	}

	public async Task<string> receive(Action<string> onReceive)
	{
		string body = "this is the body";
		await  Task.Run(() => onReceive.Invoke(body));

		return "A message Identifier";
	}
}
