namespace CloudModules;

public interface ICloudQueue<T>
{
    public Task<string> SendObjectAsJson(T obj);
    public Task<string> ReceiveJsonAsObject(Action<T> onReceive, short delay = 0);
    public Task Delete(string receipt);
    public Task Purge();
    public Task<string> Send(string messageBody);
    public Task<string> Receive(Action <string> onReceive, short delay = 0);
}
