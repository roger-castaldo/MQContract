using CoreTesting.Messages;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace CoreTesting.Consumers;

[Consumer(ignoreMessageTypeHeader: true)]
internal class BasicMessageConsumerIgnoringMessageType(List<IReceivedMessage<BasicMessage>> messages, List<Exception> errors) : IPubSubConsumer<BasicMessage>
{
    public BasicMessageConsumerIgnoringMessageType()
        : this([], []) { }

    void IBaseConsumer.ErrorRecieved(Exception error)
        => errors.Add(error);

    void IPubSubConsumer<BasicMessage>.MessageReceived(IReceivedMessage<BasicMessage> message)
        => messages.Add(message);
}
