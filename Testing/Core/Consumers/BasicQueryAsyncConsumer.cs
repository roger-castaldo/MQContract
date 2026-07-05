using CoreTesting.Messages;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace CoreTesting.Consumers
{
    [Consumer(channel: "AsyncBasicQueryMessage", group: "AsyncBasicQueryMessageGroup")]
    internal class BasicQueryAsyncConsumer : IQueryResponseAsyncConsumer<BasicQueryMessage, BasicResponseMessage>
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
        { }

        ValueTask<QueryResponseMessage<BasicResponseMessage>> IQueryResponseAsyncConsumer<BasicQueryMessage, BasicResponseMessage>.MessageReceivedAsync(IReceivedMessage<BasicQueryMessage> message)
        => ValueTask.FromResult<QueryResponseMessage<BasicResponseMessage>>(new(new(message.Message.TypeName)));
    }
}
