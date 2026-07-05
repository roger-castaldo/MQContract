using CoreTesting.Messages;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace CoreTesting.Consumers
{
    internal class BasicQueryConsumer : IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
        { }

        QueryResponseMessage<BasicResponseMessage> IQueryResponseConsumer<BasicQueryMessage, BasicResponseMessage>.MessageReceived(IReceivedMessage<BasicQueryMessage> message)
        => new(new(message.Message.TypeName));
    }
}
