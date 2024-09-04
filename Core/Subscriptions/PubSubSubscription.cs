using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class PubSubSubscription<T>(IMessageFactory<T> messageFactory, Func<IRecievedMessage<T>, ValueTask> messageRecieved, Action<Exception> errorRecieved,
        Func<string, ValueTask<string>> mapChannel,
        string? channel = null, string? group = null, bool synchronous=false,IServiceChannelOptions? options = null,ILogger? logger=null)
        : SubscriptionBase<T>(mapChannel,channel,synchronous)
        where T : class
    {
        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection,CancellationToken cancellationToken)
        {
            serviceSubscription = await connection.SubscribeAsync(
                async serviceMessage=> await ProcessMessage(serviceMessage),
                error=>errorRecieved(error),
                MessageChannel,
                group??Guid.NewGuid().ToString(),
                options:options,
                cancellationToken:cancellationToken
            );
            if (serviceSubscription==null)
                return false;
            return true;
        }

        private async ValueTask ProcessMessage(RecievedServiceMessage serviceMessage)
        {
            try
            {
                var taskMessage = await messageFactory.ConvertMessageAsync(logger, serviceMessage)
                    ??throw new InvalidCastException($"Unable to convert incoming message {serviceMessage.MessageTypeID} to {typeof(T).FullName}");
                var tsk = messageRecieved(new RecievedMessage<T>(serviceMessage.ID, taskMessage!, serviceMessage.Header, serviceMessage.RecievedTimestamp, DateTime.Now));
                if (Synchronous)
                    await tsk.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                errorRecieved(e);
            }
        }
    }
}
