using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Service;
using MQContract.Logging;
using MQContract.Messages;

namespace MQContract.Subscriptions;

internal sealed class PubSubSubscription<TMessage>(Func<ReceivedServiceMessage, ValueTask<bool>> messageReceived, Action<Exception> errorReceived,
    Func<string, ValueTask<string>> mapChannel, MessageContext context,
    ILogger logger, Action<Guid> remove, string? channel = null, string? group = null, bool synchronous = false)
    : SubscriptionBase<TMessage>(mapChannel, context, channel, synchronous, logger, remove)
{
    public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken)
    {
        using var scope = SetScope();
        Logs.Lifetime.EstablishingPubSubServiceSubscription(Logger);
        serviceSubscription = await connection.SubscribeAsync(
            async serviceMessage => await ProcessMessage(serviceMessage),
            error => errorReceived(error),
            MessageChannel,
            group: group,
            cancellationToken: cancellationToken
        );
        if (serviceSubscription==null)
            return false;
        Logs.Lifetime.PubSubSubscriptionEstablishmentSucceeded(Logger);
        return true;
    }

    private async ValueTask ProcessMessage(ReceivedServiceMessage serviceMessage)
    {
        using var scope = SetScope();
        try
        {
            Logs.Consuming.ProcessingServiceMessage(Logger, serviceMessage.ID);
            var tsk = messageReceived(serviceMessage);
            var ack = await tsk.ConfigureAwait(!Synchronous);
            if (serviceMessage.Acknowledge!=null && ack)
            {
                Logs.Consuming.AcknowledgingServiceMessage(Logger, serviceMessage.ID);
                await serviceMessage.Acknowledge();
            }
        }
        catch (Exception e)
        {
            Logs.Consuming.ProcessingServiceMessageError(Logger, e, serviceMessage.ID);
            errorReceived(e);
        }
    }
}
