using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace CloudModules;

public class JsonifiedQueue<T>: BasicQueue, ICloudQueue<T>
{
    public JsonifiedQueue(string queueUrl, AmazonSQSClient sqsClient): base(queueUrl, sqsClient)
    {

    }

    public async Task<string> sendObjectAsJson(T obj)
    {
        return await send(JsonSerializer.Serialize(obj));
    }

    public async Task<string> receiveJsonAsObject(Action<T> onReceive, short delay)
    {
        Action<string> onReceiveWrapper = (jsonMsg) => {
            if (jsonMsg is null)
            {
                throw new CloudModuleException("Json Message received is null", ErrorCodes.JSON_MESSAGE_IS_NULL);
            }
            onReceive(JsonSerializer.Deserialize<T>(jsonMsg));
        };
        return await receive(onReceiveWrapper, delay);
    }
}
