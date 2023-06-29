namespace CloudModules;

public interface ICloudQueue<T>
{
    public Task<string> sendObjectAsJson(T obj);
    public Task<string> receiveJsonAsObject(Action<T> onReceive, short delay = 0);
    public Task delete(string receipt);
    public Task purge();
    public Task<string> send(string messageBody);
    public Task<string> receive(Action <string> onReceive, short delay = 0);
}
