using CoreTesting.Messages;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace CoreTesting.Consumers;

internal class BasicMessageConsumer : IPubSubConsumer<BasicMessage>
{
    private static readonly List<IReceivedMessage<BasicMessage>> messages = [];

    public static List<IReceivedMessage<BasicMessage>> Messages => messages;

    public BasicMessageConsumer()
    {
        messages.Clear();
    }

    void IBaseConsumer.ErrorRecieved(Exception error)
    { }

    void IPubSubConsumer<BasicMessage>.MessageReceived(IReceivedMessage<BasicMessage> message)
        => messages.Add(message);
}
