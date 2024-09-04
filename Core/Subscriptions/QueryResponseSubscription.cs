using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class QueryResponseSubscription<Q,R>(IMessageFactory<Q> queryMessageFactory,IMessageFactory<R> responseMessageFactory, 
        Func<IRecievedMessage<Q>, ValueTask<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved,
        Func<string, ValueTask<string>> mapChannel,
        string? channel = null, string? group = null, 
        bool synchronous=false,IServiceChannelOptions? options = null,ILogger? logger=null)
        : SubscriptionBase<Q>(mapChannel,channel,synchronous)
        where Q : class
        where R : class
    {
        private ManualResetEventSlim? manualResetEvent = new(true);
        private CancellationTokenSource? token = new();

        public async ValueTask<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken)
        {
            if (connection is IQueryableMessageServiceConnection queryableMessageServiceConnection)
                serviceSubscription = await queryableMessageServiceConnection.SubscribeQueryAsync(
                    serviceMessage => ProcessServiceMessageAsync(serviceMessage),
                    error => errorRecieved(error),
                    MessageChannel,
                    group??Guid.NewGuid().ToString(),
                    options: options,
                    cancellationToken: cancellationToken
                );
            else
            {
                serviceSubscription = await connection.SubscribeAsync(
                    async (serviceMessage) =>
                    {
                        if (!QueryResponseHelper.IsValidMessage(serviceMessage))
                            errorRecieved(new InvalidQueryResponseMessageRecieved());
                        else
                        {
                            var result = await ProcessServiceMessageAsync(
                                new(
                                    serviceMessage.ID,
                                    serviceMessage.MessageTypeID,
                                    serviceMessage.Channel,
                                    QueryResponseHelper.StripHeaders(serviceMessage, out var queryClientID, out var replyID, out var replyChannel),
                                    serviceMessage.Data
                                )
                            );
                            await connection.PublishAsync(QueryResponseHelper.EncodeMessage(result, queryClientID, replyID, null,replyChannel), null, cancellationToken);
                        }
                    },
                    error=> errorRecieved(error),
                    MessageChannel,
                    group??Guid.NewGuid().ToString(),
                    options:options,
                    cancellationToken:cancellationToken
                );
            }
            return serviceSubscription!=null;
        }

        private async ValueTask<ServiceMessage> ProcessServiceMessageAsync(RecievedServiceMessage message)
        {
            if (Synchronous&&!(token?.IsCancellationRequested??false))
                manualResetEvent!.Wait(cancellationToken:token!.Token);
            Exception? error = null;
            ServiceMessage? response = null;
            try
            {
                var taskMessage = await queryMessageFactory.ConvertMessageAsync(logger, message)
                                        ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(Q).FullName}");
                var result = await messageRecieved(new RecievedMessage<Q>(message.ID, taskMessage,message.Header,message.RecievedTimestamp,DateTime.Now));
                response = await responseMessageFactory.ConvertMessageAsync(result.Message, message.Channel, new MessageHeader(result.Headers));
            }catch(Exception e)
            {
                errorRecieved(e);
                error=e;
            }
            if (Synchronous)
                manualResetEvent!.Set();
            if (error!=null)
                return ErrorServiceMessage.Produce(message.Channel,error);
            return response??ErrorServiceMessage.Produce(message.Channel, new NullReferenceException());
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
