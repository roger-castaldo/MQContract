using AutomatedTesting.Messages;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace AutomatedTesting.Consumers
{
    internal class BasicMessageConsumer : IPubSubConsumer<BasicMessage>
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
        { }

        void IPubSubConsumer<BasicMessage>.MessageReceived(IReceivedMessage<BasicMessage> message)
        { }
    }
}
