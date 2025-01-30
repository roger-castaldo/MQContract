using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class QueryResponseSubscription<T>(
        Func<ReceivedServiceMessage, string, ValueTask<ServiceMessage>> processMessage,
        Action<Exception> errorReceived,
        Func<string, ValueTask<string>> mapChannel,
        string? channel = null, string? group = null,
        bool synchronous = false, ILogger? logger = null)
        : SubscriptionBase<T>(mapChannel, channel, synchronous, logger)
    {
        private ManualResetEventSlim? manualResetEvent = new(true);
        private CancellationTokenSource? token = new();

        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogInformation("Establishing underlying service subscription for QueryResponse subscription.");

            try
            {
                if (connection is IQueryableMessageServiceConnection queryableMessageServiceConnection)
                {
                    Logger?.LogDebug("Establishing underlying QueryResponse service subscription.");
                    serviceSubscription = await queryableMessageServiceConnection.SubscribeQueryAsync(
                        serviceMessage => ProcessServiceMessageAsync(serviceMessage, string.Empty),
                        error => errorReceived(error),
                        MessageChannel,
                        group: group,
                        cancellationToken: cancellationToken
                    );
                    Logger?.LogInformation("Successfully established QueryResponse subscription.");
                }
                else
                {
                    Logger?.LogInformation("Establishing underlying PubSub service subscription to listen for incoming queries.");
                    serviceSubscription = await connection.SubscribeAsync(
                        async (serviceMessage) =>
                        {
                            if (!QueryResponseHelper.IsValidMessage(serviceMessage))
                            {
                                Logger?.LogWarning("Received invalid query response message.");
                                errorReceived(new InvalidQueryResponseMessageReceivedException());
                            }
                            else
                            {
                                Logger?.LogDebug("Processing received service message.");
                                var result = await ProcessServiceMessageAsync(
                                    new(
                                        serviceMessage.ID,
                                        serviceMessage.MessageTypeID,
                                        serviceMessage.Channel,
                                        QueryResponseHelper.StripHeaders(serviceMessage, out var queryClientID, out var replyID, out var replyChannel),
                                        serviceMessage.Data
                                    ),
                                    replyChannel!
                                );
                                await connection.PublishAsync(QueryResponseHelper.EncodeMessage(result, queryClientID, replyID, null, replyChannel), cancellationToken);
                            }
                        },
                        error => errorReceived(error),
                        MessageChannel,
                        cancellationToken: cancellationToken
                    );
                    Logger?.LogInformation("Successfully established PubSub subscription.");
                }

                return serviceSubscription != null;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Error occurred while establishing the subscription.");
                throw;
            }
        }

        private async ValueTask<ServiceMessage> ProcessServiceMessageAsync(ReceivedServiceMessage message, string replyChannel)
        {
            using var scope = SetScope();
            if (Synchronous && !(token?.IsCancellationRequested ?? false))
            {
                Logger?.LogDebug("Waiting for manual reset event to complete synchronous operation.");
                manualResetEvent!.Wait(cancellationToken: token!.Token);
            }

            Exception? error = null;
            ServiceMessage? response = null;

            try
            {
                Logger?.LogDebug("Processing service message with ID: {MessageID}", message.ID);
                response = await processMessage(message, replyChannel);
                if (message.Acknowledge != null)
                {
                    Logger?.LogDebug("Acknowledging service message with ID: {MessageID}", message.ID);
                    await message.Acknowledge();
                }
            }
            catch (Exception e)
            {
                Logger?.LogError(e, "Error occurred while processing service message with ID: {MessageID}", message.ID);
                errorReceived(e);
                error = e;
            }

            if (Synchronous)
            {
                Logger?.LogDebug("Setting manual reset event for synchronous operation.");
                manualResetEvent!.Set();
            }

            if (error != null)
            {
                Logger?.LogWarning("Returning error response for message with ID: {MessageID}", message.ID);
                return ErrorServiceMessage.Produce(replyChannel, error);
            }

            Logger?.LogInformation("Returning valid service response for message with ID: {MessageID}", message.ID);
            return response ?? ErrorServiceMessage.Produce(replyChannel, new NullReferenceException());
        }

        protected override void InternalDispose()
        {
            Logger?.LogInformation("Disposing resources for QueryResponseSubscription.");

            if (token != null)
            {
                Logger?.LogDebug("Cancelling token for QueryResponseSubscription.");
                token.Cancel();
                manualResetEvent?.Dispose();
                token.Dispose();
                token = null;
                manualResetEvent = null;
            }

            Logger?.LogInformation("Resources for QueryResponseSubscription have been disposed.");
        }
    }
}
