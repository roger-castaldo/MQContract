using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class PubSubSubscription<T>(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived,
        Func<string, ValueTask<string>> mapChannel,
        string? channel = null, string? group = null, bool synchronous=false,ILogger? logger=null)
        : SubscriptionBase<T>(mapChannel,channel,synchronous)
        where T : class
    {
        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection,CancellationToken cancellationToken)
        {
            serviceSubscription = await connection.SubscribeAsync(
                async serviceMessage => await ProcessMessage(serviceMessage),
                error => errorReceived(error),
                MessageChannel,
                group:group,
                cancellationToken: cancellationToken
            );
            if (serviceSubscription==null)
                return false;
            return true;
        }

        private async ValueTask ProcessMessage(ReceivedServiceMessage serviceMessage)
        {
            try
            {
                var tsk = messageReceived(serviceMessage);
                await tsk.ConfigureAwait(!Synchronous);
                if (serviceMessage.Acknowledge!=null)
                    await serviceMessage.Acknowledge();
            }
            catch (Exception e)
            {
                errorReceived(e);
            }
        }
    }
}
