using Microsoft.Extensions.Logging;
using MQContract.Extensions;
using MQContract.Interfaces.Service;
using MQContract.Loggers;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class PubSubSubscription<TMessage>(Func<ReceivedServiceMessage, ValueTask<bool>> messageReceived, Action<Exception> errorReceived,
        Func<string, ValueTask<string>> mapChannel, MessageContext context,
        string? channel = null, string? group = null, bool synchronous = false, ILogger? logger = null)
        : SubscriptionBase<TMessage>(mapChannel, context, channel, synchronous, logger)
    {
        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            PubSubLog.EstablishingPubSubServiceSubscription(Logger);
            serviceSubscription = await connection.SubscribeAsync(
                async serviceMessage => await ProcessMessage(serviceMessage),
                error => errorReceived(error),
                MessageChannel,
                group: group,
                cancellationToken: cancellationToken
            );
            if (serviceSubscription==null)
                return false;
            PubSubLog.PubSubSubscriptionEstablishmentSucceeded(Logger);
            return true;
        }

        private async ValueTask ProcessMessage(ReceivedServiceMessage serviceMessage)
        {
            using var scope = SetScope();
            try
            {
                PubSubLog.ProcessingServiceMessage(Logger, serviceMessage.ID);
                var tsk = messageReceived(serviceMessage);
                var ack = await tsk.ConfigureAwait(!Synchronous);
                if (serviceMessage.Acknowledge!=null && ack)
                {
                    PubSubLog.AcknowledgingServiceMessage(Logger, serviceMessage.ID);
                    await serviceMessage.Acknowledge();
                }
            }
            catch (Exception e)
            {
                PubSubLog.ProcessingServiceMessageError(Logger, e, serviceMessage.ID);
                errorReceived(e);
            }
        }
    }
}
