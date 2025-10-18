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
        private static readonly List<IReceivedMessage<BasicMessage>> messages = [];

        public static List<IReceivedMessage<BasicMessage>> Messages => messages;

        public BasicMessageAsyncConsumer()
        {
            messages.Clear();
        }

        void IBaseConsumer.ErrorRecieved(Exception error)
        { }

        ValueTask IPubSubAsyncConsumer<BasicMessage>.MessageReceivedAsync(IReceivedMessage<BasicMessage> message)
        {
            messages.Add(message);
            return ValueTask.CompletedTask;
        }
    }
}
