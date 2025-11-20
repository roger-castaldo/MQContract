using Microsoft.Extensions.Logging;
using MQContract.Extensions;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class PubSubSubscription<TMessage>(Func<ReceivedServiceMessage, ValueTask<bool>> messageReceived, Action<Exception> errorReceived,
        Func<string, ValueTask<string>> mapChannel,
        string? channel = null, string? group = null, bool synchronous = false, ILogger? logger = null)
        : SubscriptionBase<TMessage>(mapChannel, channel, synchronous, logger)
    {
        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogInformationChecked("Establishing underlying service subscription for PubSub subscription.");
            serviceSubscription = await connection.SubscribeAsync(
                async serviceMessage => await ProcessMessage(serviceMessage),
                error => errorReceived(error),
                MessageChannel,
                group: group,
                cancellationToken: cancellationToken
            );
            if (serviceSubscription==null)
                return false;
            Logger?.LogInformationChecked("Successfully established PubSub subscription.");
            return true;
        }

        private async ValueTask ProcessMessage(ReceivedServiceMessage serviceMessage)
        {
            using var scope = SetScope();
            try
            {
                Logger?.LogDebugChecked("Processing service message with ID: {MessageID}", serviceMessage.ID);
                var tsk = messageReceived(serviceMessage);
                var ack = await tsk.ConfigureAwait(!Synchronous);
                if (serviceMessage.Acknowledge!=null && ack)
                {
                    Logger?.LogDebugChecked("Acknowledging service message with ID: {MessageID}", serviceMessage.ID);
                    await serviceMessage.Acknowledge();
                }
            }
            catch (Exception e)
            {
                Logger?.LogErrorChecked(e, "Error occurred while processing service message with ID: {MessageID}", serviceMessage.ID);
                errorReceived(e);
            }
        }
    }
}
