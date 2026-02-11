using Microsoft.Extensions.Logging;
using MQContract.Connections;
using MQContract.Extensions;
using MQContract.Interfaces.Service;
using MQContract.Loggers;
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
            QueryResponseLog.EstablishingServiceSubscription(Logger);

            try
            {
                if (connection is IQueryableMessageServiceConnection queryableMessageServiceConnection)
                {
                    QueryResponseLog.EstablishingQueryResponseServiceSubscription(Logger);
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
                    QueryResponseLog.EstablishingQueryResponseServiceSubscriptionSuccess(Logger);
                }
                else
                {
                    QueryResponseLog.EstablishingPubSubServiceSubscription(Logger);
                    serviceSubscription = await connection.SubscribeAsync(
                        async (serviceMessage) =>
                        {
                            if (!QueryResponseHelper.IsValidMessage(serviceMessage))
                            {
                                QueryResponseLog.ReceivedInvalidQueryResponseMessage(Logger);
                                errorReceived(new InvalidQueryResponseMessageReceivedException());
                            }
                            else
                            {
                                QueryResponseLog.ProcessingRecievedMessage(Logger, serviceMessage.ID);
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
                    QueryResponseLog.EstablishinPubSubServiceSubscriptionSuccess(Logger);
                }

                return serviceSubscription != null;
            }
            catch (Exception ex)
            {
                QueryResponseLog.ErrorEstablishingSubscription(Logger, ex);
                throw new SubscriptionFailedException(ex);
            }
        }

        private readonly record struct ProcessedServiceResponse(ServiceMessage? Response, Activity? Activity);

        private async ValueTask<ProcessedServiceResponse> ProcessServiceMessageAsync(ReceivedServiceMessage message, string replyChannel)
        {
            using var scope = SetScope();
            if (Synchronous && !(token?.IsCancellationRequested ?? false))
            {
                QueryResponseLog.WaitingToProcessMessage(Logger);
                manualResetEvent!.Wait(cancellationToken: token!.Token);
            }

            Exception? error = null;
            FilteredServiceMessage? response = null;

            try
            {
                QueryResponseLog.ProcessingServiceMessage(Logger, message.ID);
                response = await processMessage(message, replyChannel);
                if (message.Acknowledge != null && !Equals(response?.FilterResult, MessageFilterResult.DropAndDontAcknowledge))
                {
                    QueryResponseLog.AcknowledgingServiceMessage(Logger, message.ID);
                    await message.Acknowledge();
                }
            }
            catch (Exception e)
            {
                QueryResponseLog.ErrorProcessingServiceMessage(Logger, e, message.ID);
                errorReceived(e);
                error = e;
            }

            if (Synchronous)
            {
                QueryResponseLog.ReleasingMessageWait(Logger);
                manualResetEvent!.Set();
            }

            if (error != null)
            {
                QueryResponseLog.ReturningErrorMessage(Logger, message.ID);
                return new(ErrorServiceMessage.Produce(replyChannel, error), response?.Activity);
            }

            QueryResponseLog.ReturningValidResponse(Logger, message.ID);
            return new(response?.ServiceMessage ?? (Equals(response?.FilterResult, MessageFilterResult.Allow) ? ErrorServiceMessage.Produce(replyChannel, new NullReferenceException()) : null), response?.Activity);
        }

        protected override void InternalDispose()
        {
            QueryResponseLog.Disposing(Logger);

            if (token != null)
            {
                QueryResponseLog.CancellingToken(Logger);
                token.Cancel();
                manualResetEvent?.Dispose();
                token.Dispose();
                token = null;
                manualResetEvent = null;
            }

            QueryResponseLog.Disposed(Logger);
        }
    }
}
