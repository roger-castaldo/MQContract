using AutomatedTesting.Messages;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace AutomatedTesting.Consumers
{
    [ConsumerMessageChannel("AsyncBasicMessage")]
    [ConsumerGroup("AsyncBasicMessageGroup")]
    internal class BasicMessageAsyncConsumer : IPubSubAsyncConsumer<BasicMessage>
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
        { }

        ValueTask IPubSubAsyncConsumer<BasicMessage>.MessageReceivedAsync(IReceivedMessage<BasicMessage> message)
        => ValueTask.CompletedTask;
    }
}
