using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace CloudModules;

public class JsonifiedQueue<T>: BasicQueue, ICloudQueue<T>
{
    public JsonifiedQueue(string queueUrl, AmazonSQSClient sqsClient): base(queueUrl, sqsClient)
    {

    }

    public async Task <string> sendAsJson(T obj)
    {
        return await send(JsonSerializer.Serialize(obj));
    }

    public async Task <string> receiveAsJson(Action <T> onReceive, short delay)
    {
        Action <string> onReceiveWrapper = (jsonMsg) => {
            onReceive(JsonSerializer.Deserialize<T>(jsonMsg));
        };
        return await receive(onReceiveWrapper, delay);
    }
}
