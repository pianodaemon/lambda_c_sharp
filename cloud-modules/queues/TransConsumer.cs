namespace CloudModules;

public class TransConsumer
{
    public static List<R> DoConsume<T, R>(Func<T, R> transHandler, ICloudQueue<T> q)
    {
        var productionList = new List<R>();
        Action<T> actOnReceiveHandler = (tpo) =>
        {
            R elementTransformed = transHandler(tpo);
            if (elementTransformed != null) productionList.Add(elementTransformed);
        };

        short consumptionCounter = 0;
        try
        {
            for(;;)
            {
                var t = q.ReceiveJsonAsObject(actOnReceiveHandler);
                t.Wait();
                q.Delete(t.Result).Wait();
                consumptionCounter++;
            }
        }
        catch (AggregateException ae)
        {
            ae.Handle((ex) =>
            {
                // This is what we expect to handle as
                // the minimal consumption.
                if ((ex is CloudModuleException) && (consumptionCounter > 0) &&
                   (((CloudModuleException) ex).ErrorCode == ErrorCodes.NO_MESSAGES_FOUND_IN_QUEUE))
                {
                    return true;
                }

                return false;
            });
        }

        return productionList;
    }
}
