using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class QueryResponseSubscription<T>(
        Func<ReceivedServiceMessage,string, ValueTask<ServiceMessage>> processMessage,
        Action<Exception> errorReceived,
        Func<string, ValueTask<string>> mapChannel,
        string? channel = null, string? group = null, 
        bool synchronous=false,ILogger? logger=null)
        : SubscriptionBase<T>(mapChannel,channel,synchronous)
        where T : class
    {
        private ManualResetEventSlim? manualResetEvent = new(true);
        private CancellationTokenSource? token = new();

        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken)
        {
            if (connection is IQueryableMessageServiceConnection queryableMessageServiceConnection)
                serviceSubscription = await queryableMessageServiceConnection.SubscribeQueryAsync(
                    serviceMessage => ProcessServiceMessageAsync(serviceMessage,string.Empty),
                    error => errorReceived(error),
                    MessageChannel,
                    group:group,
                    cancellationToken: cancellationToken
                );
            else
            {
                serviceSubscription = await connection.SubscribeAsync(
                    async (serviceMessage) =>
                    {
                        if (!QueryResponseHelper.IsValidMessage(serviceMessage))
                            errorReceived(new InvalidQueryResponseMessageReceived());
                        else
                        {
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
            }
            return serviceSubscription!=null;
        }

        private async ValueTask<ServiceMessage> ProcessServiceMessageAsync(ReceivedServiceMessage message,string replyChannel)
        {
            if (Synchronous&&!(token?.IsCancellationRequested??false))
                manualResetEvent!.Wait(cancellationToken:token!.Token);
            Exception? error = null;
            ServiceMessage? response = null;
            try
            {
                response = await processMessage(message,replyChannel);
                if (message.Acknowledge!=null)
                    await message.Acknowledge();
            }catch(Exception e)
            {
                errorReceived(e);
                error=e;
            }
            if (Synchronous)
                manualResetEvent!.Set();
            if (error!=null)
                return ErrorServiceMessage.Produce(replyChannel,error);
            return response??ErrorServiceMessage.Produce(replyChannel, new NullReferenceException());
        }

        protected override void InternalDispose()
        {
            if (token!=null)
            {
                token.Cancel();
                manualResetEvent?.Dispose();
                token.Dispose();
                token=null;
                manualResetEvent=null;
            }
        }
    }
}
