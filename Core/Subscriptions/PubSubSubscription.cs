using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class PubSubSubscription<T>(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived,
        Func<string, ValueTask<string>> mapChannel,
        string? channel = null, string? group = null, bool synchronous=false,ILogger? logger=null)
        : SubscriptionBase<T>(mapChannel,channel,synchronous,logger)
        where T : class
    {
        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection,CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogInformation("Establishing underlying service subscription for PubSub subscription.");
            serviceSubscription = await connection.SubscribeAsync(
                async serviceMessage => await ProcessMessage(serviceMessage),
                error => errorReceived(error),
                MessageChannel,
                group:group,
                cancellationToken: cancellationToken
            );
            if (serviceSubscription==null)
                return false;
            Logger?.LogInformation("Successfully established PubSub subscription.");
            return true;
        }

        private async ValueTask ProcessMessage(ReceivedServiceMessage serviceMessage)
        {
            using var scope = SetScope();
            try
            {
                Logger?.LogDebug("Processing service message with ID: {MessageID}", serviceMessage.ID);
                var tsk = messageReceived(serviceMessage);
                await tsk.ConfigureAwait(!Synchronous);
                if (serviceMessage.Acknowledge!=null)
                {
                    Logger?.LogDebug("Acknowledging service message with ID: {MessageID}", serviceMessage.ID);
                    await serviceMessage.Acknowledge();
                }
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Error occurred while processing service message with ID: {MessageID}", serviceMessage.ID);
                errorReceived(e);
            }
        }
    }
}
