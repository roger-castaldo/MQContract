using Microsoft.Extensions.Logging;
using MQContract.Connections;
using MQContract.Interfaces.Service;
using MQContract.Logging;
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
        ILogger logger, string? channel = null,
        string? group = null, bool synchronous = false)
        : SubscriptionBase<TMessage>(mapChannel, context, channel, synchronous, logger)
    {
        private ManualResetEventSlim? manualResetEvent = new(true);
        private CancellationTokenSource? token = new();

        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, string? serviceConnectionName, CancellationToken cancellationToken)
        {
            using var scope = SetScope();
            Logs.Lifetime.EstablishingServiceSubscription(Logger);

            try
            {
                if (connection is IQueryableMessageServiceConnection queryableMessageServiceConnection)
                {
                    Logs.Lifetime.EstablishingQueryResponseServiceSubscription(Logger);
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
                    Logs.Lifetime.EstablishingQueryResponseSubscriptionSuccess(Logger);
                }
                else
                {
                    Logs.Lifetime.EstablishingQueryResponseSubscriptionWithPubSubServiceSubscription(Logger);
                    serviceSubscription = await connection.SubscribeAsync(
                        async (serviceMessage) =>
                        {
                            if (!QueryResponseHelper.IsValidMessage(serviceMessage))
                            {
                                Logs.Pipeline.ReceivedInvalidQueryResponseMessage(Logger);
                                errorReceived(new InvalidQueryResponseMessageReceivedException());
                            }
                            else
                            {
                                Logs.Consuming.ProcessingServiceMessage(Logger, serviceMessage.ID);
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
                    Logs.Lifetime.EstablishingQueryResponseSubscriptionSuccess(Logger);
                }

                return serviceSubscription != null;
            }
            catch (Exception ex)
            {
                Logs.Lifetime.ErrorEstablishingSubscription(Logger, ex);
                throw new SubscriptionFailedException(ex);
            }
        }

        private readonly record struct ProcessedServiceResponse(ServiceMessage? Response, Activity? Activity);

        private async ValueTask<ProcessedServiceResponse> ProcessServiceMessageAsync(ReceivedServiceMessage message, string replyChannel)
        {
            using var scope = SetScope();
            if (Synchronous && !(token?.IsCancellationRequested ?? false))
            {
                Logs.Consuming.WaitingToProcessMessage(Logger);
                manualResetEvent!.Wait(cancellationToken: token!.Token);
            }

            Exception? error = null;
            FilteredServiceMessage? response = null;

            try
            {
                Logs.Consuming.ProcessingServiceMessage(Logger, message.ID);
                response = await processMessage(message, replyChannel);
                if (message.Acknowledge != null && !Equals(response?.FilterResult, MessageFilterResult.DropAndDontAcknowledge))
                {
                    Logs.Consuming.AcknowledgingServiceMessage(Logger, message.ID);
                    await message.Acknowledge();
                }
            }
            catch (Exception e)
            {
                Logs.Consuming.ProcessingServiceMessageError(Logger, e, message.ID);
                errorReceived(e);
                error = e;
            }

            if (Synchronous)
            {
                Logs.Consuming.ReleasingMessageWait(Logger);
                manualResetEvent!.Set();
            }

            if (error != null)
            {
                Logs.Consuming.ReturningErrorMessage(Logger, message.ID);
                return new(ErrorServiceMessage.Produce(replyChannel, error), response?.Activity);
            }

            Logs.Consuming.ReturningValidResponse(Logger, message.ID);
            return new(response?.ServiceMessage ?? (Equals(response?.FilterResult, MessageFilterResult.Allow) ? ErrorServiceMessage.Produce(replyChannel, new NullReferenceException()) : null), response?.Activity);
        }

        protected override void InternalDispose()
        {
            Logs.Lifetime.DisposingQueryResponseSubscription(Logger);

            if (token != null)
            {
                Logs.Consuming.CancellingToken(Logger);
                token.Cancel();
                manualResetEvent?.Dispose();
                token.Dispose();
                token = null;
                manualResetEvent = null;
            }

            Logs.Lifetime.DisposedQueryResponseSubscription(Logger);
        }
    }
}
