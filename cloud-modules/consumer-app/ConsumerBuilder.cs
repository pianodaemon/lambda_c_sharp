using System;
using System.Text;

public interface IConsumerBridge {
    public int engage();
}

public class ConsumerBridge<W> : IConsumerBridge
{

    // required parameters
    private string buffer;
    private Func<string, W> parser;
    private Func<W, int> consumer;

    public ConsumerBridge(ConsumerBridgeBuilder<W> builder) {
        this.buffer = builder.buffer;
        this.parser = builder.parser;
        this.consumer = builder.consumer;
    }

    public int engage()
    {
        var tobj = parseInputTextBuffer<W>(ref this.buffer, this.parser);
        return this.consumer(tobj);
    }

    protected static M parseInputTextBuffer<M>(ref string buffer, Func<string, M> parser)
    {
        int buffSize = System.Text.ASCIIEncoding.Unicode.GetByteCount(buffer);
        return parser(buffer);
    }
}

public class ConsumerBridgeBuilder<T>{

    // required parameters
    public string buffer = null!;
    public Func<string, T> parser = null!;
    public Func<T, int> consumer = null!;

    public ConsumerBridgeBuilder<T> setBuffer(ref string buffer) {
        this.buffer = buffer;
        return this;
    }

    public ConsumerBridgeBuilder<T> setParser(Func<string, T> parser) {
        this.parser = parser;
        return this;
    }

    public ConsumerBridgeBuilder<T> setConsumer(Func<T, int> consumer) {
        this.consumer = consumer;
        return this;
    }

    public ConsumerBridge<T> build(){
        return new ConsumerBridge<T>(this);
    }
}
