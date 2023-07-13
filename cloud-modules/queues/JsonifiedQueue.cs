using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace CloudModules;

public class JsonifiedQueue<T>: BasicQueue, ICloudQueue<T>
{
    public JsonifiedQueue(string queueUrl, AmazonSQSClient sqsClient): base(queueUrl, sqsClient)
    {

    }

    public async Task<string> SendObjectAsJson(T obj)
    {
        return await Send(JsonSerializer.Serialize(obj));
    }

    public async Task<string> ReceiveJsonAsObject(Action<T> onReceive, short delay)
    {
        Action<string> onReceiveWrapper = (jsonMsg) => {
            if (jsonMsg is null)
            {
                throw new CloudModuleException("Json Message received is null", ErrorCodes.JSON_MESSAGE_IS_NULL);
            }

            var obj = JsonSerializer.Deserialize<T>(jsonMsg);
            if (obj is null)
            {
                throw new CloudModuleException("It seems the Json Message was not correctly deserialized", ErrorCodes.JSON_MESSAGE_WAS_NOT_DES);
            }
            onReceive(obj);
        };
        return await Receive(onReceiveWrapper, delay);
    }
}
