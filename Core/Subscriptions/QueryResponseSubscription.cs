using Microsoft.Extensions.Logging;
using MQContract.Connections;
using MQContract.Extensions;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using MQContract.Middleware;
using System.Diagnostics;

namespace MQContract.Subscriptions
{
    internal sealed class QueryResponseSubscription<TMessage>(
        Func<ReceivedServiceMessage, string, ValueTask<FilteredServiceMessage>> processMessage,
        Action<Exception> errorReceived,
        Func<string, ValueTask<string>> mapChannel,
        MessageContext context,
        string? channel = null, string? group = null,
        bool synchronous = false, ILogger? logger = null)
        : SubscriptionBase<TMessage>(mapChannel, context, channel, synchronous, logger)
    {
        private ManualResetEventSlim? manualResetEvent = new(true);
        private CancellationTokenSource? token = new();

        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, string? serviceConnectionName, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logger?.LogInformationChecked("Establishing underlying service subscription for QueryResponse subscription.");

            try
            {
                if (connection is IQueryableMessageServiceConnection queryableMessageServiceConnection)
                {
                    Logger?.LogDebugChecked("Establishing underlying QueryResponse service subscription.");
                    serviceSubscription = await queryableMessageServiceConnection.SubscribeQueryAsync(
                        async serviceMessage =>
                        {
                            (var responseMessage, _) = await ProcessServiceMessageAsync(serviceMessage, string.Empty);
                            return responseMessage;
                        },
                        error => errorReceived(error),
                        MessageChannel,
                        group: group,
                        cancellationToken: cancellationToken
                    );
                    Logger?.LogInformationChecked("Successfully established QueryResponse subscription.");
                }
                else
                {
                    Logger?.LogInformationChecked("Establishing underlying PubSub service subscription to listen for incoming queries.");
                    serviceSubscription = await connection.SubscribeAsync(
                        async (serviceMessage) =>
                        {
                            if (!QueryResponseHelper.IsValidMessage(serviceMessage))
                            {
                                Logger?.LogWarningChecked("Received invalid query response message.");
                                errorReceived(new InvalidQueryResponseMessageReceivedException());
                            }
                            else
                            {
                                Logger?.LogDebugChecked("Processing received service message.");
                                (var resultMessage, var activity) = await ProcessServiceMessageAsync(
                                    new(
                                        serviceMessage.ID,
                                        serviceMessage.MessageTypeID,
                                        serviceMessage.Channel,
                                        QueryResponseHelper.StripHeaders(serviceMessage, out var queryClientID, out var replyID, out var replyChannel),
                                        serviceMessage.Data,
                                        serviceMessage.Acknowledge
                                    ),
                                    replyChannel!
                                );
                                if (resultMessage!=null)
                                {
                                    var res = await connection.PublishAsync(QueryResponseHelper.EncodeMessage(resultMessage!, queryClientID, replyID, null, replyChannel), cancellationToken);
                                    OpenTelemetryMiddleware.AddMessagePublishedEvent(activity, resultMessage!, res, connection, serviceConnectionName);
                                }
                            }
                        },
                        error => errorReceived(error),
                        MessageChannel,
                        cancellationToken: cancellationToken
                    );
                    Logger?.LogInformationChecked("Successfully established PubSub subscription.");
                }

                return serviceSubscription != null;
            }
            catch (Exception ex)
            {
                Logger?.LogErrorChecked(ex, "Error occurred while establishing the subscription.");
                throw new SubscriptionFailedException(ex);
            }
        }

        private readonly record struct ProcessedServiceResponse(ServiceMessage? Response, Activity? Activity);

        private async ValueTask<ProcessedServiceResponse> ProcessServiceMessageAsync(ReceivedServiceMessage message, string replyChannel)
        {
            using var scope = SetScope();
            if (Synchronous && !(token?.IsCancellationRequested ?? false))
            {
                Logger?.LogDebugChecked("Waiting for manual reset event to complete synchronous operation.");
                manualResetEvent!.Wait(cancellationToken: token!.Token);
            }

            Exception? error = null;
            FilteredServiceMessage? response = null;

            try
            {
                Logger?.LogDebugChecked("Processing service message with ID: {MessageID}", message.ID);
                response = await processMessage(message, replyChannel);
                if (message.Acknowledge != null && !Equals(response?.FilterResult, MessageFilterResult.DropAndDontAcknowledge))
                {
                    Logger?.LogDebugChecked("Acknowledging service message with ID: {MessageID}", message.ID);
                    await message.Acknowledge();
                }
            }
            catch (Exception e)
            {
                Logger?.LogErrorChecked(e, "Error occurred while processing service message with ID: {MessageID}", message.ID);
                errorReceived(e);
                error = e;
            }

            if (Synchronous)
            {
                Logger?.LogDebugChecked("Setting manual reset event for synchronous operation.");
                manualResetEvent!.Set();
            }

            if (error != null)
            {
                Logger?.LogWarningChecked("Returning error response for message with ID: {MessageID}", message.ID);
                return new(ErrorServiceMessage.Produce(replyChannel, error), response?.Activity);
            }

            Logger?.LogInformationChecked("Returning valid service response for message with ID: {MessageID}", message.ID);
            return new(response?.ServiceMessage ?? (Equals(response?.FilterResult, MessageFilterResult.Allow) ? ErrorServiceMessage.Produce(replyChannel, new NullReferenceException()) : null), response?.Activity);
        }

        protected override void InternalDispose()
        {
            Logger?.LogInformationChecked("Disposing resources for QueryResponseSubscription.");

            if (token != null)
            {
                Logger?.LogDebugChecked("Cancelling token for QueryResponseSubscription.");
                token.Cancel();
                manualResetEvent?.Dispose();
                token.Dispose();
                token = null;
                manualResetEvent = null;
            }

            Logger?.LogInformationChecked("Resources for QueryResponseSubscription have been disposed.");
        }
    }
}
