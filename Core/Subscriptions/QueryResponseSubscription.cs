using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class QueryResponseSubscription<Q,R>(IMessageFactory<Q> queryMessageFactory,IMessageFactory<R> responseMessageFactory, 
        Func<IRecievedMessage<Q>, Task<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved,
        Func<string, Task<string>> mapChannel,
        string? channel = null, string? group = null, 
        bool synchronous=false,IServiceChannelOptions? options = null,ILogger? logger=null)
        : SubscriptionBase<Q>(mapChannel,channel,synchronous),ISubscription
        where Q : class
        where R : class
    {
        private readonly ManualResetEventSlim manualResetEvent = new(true);

        public async Task<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken = new CancellationToken())
        {
            SyncToken(cancellationToken);
            serviceSubscription = await connection.SubscribeQueryAsync(
                serviceMessage => ProcessServiceMessageAsync(serviceMessage),
                error => errorRecieved(error),
                MessageChannel,
                group??Guid.NewGuid().ToString(),
                options: options,
                cancellationToken: token.Token
            );
            return serviceSubscription!=null;
        }

        private async Task<ServiceMessage> ProcessServiceMessageAsync(RecievedServiceMessage message)
        {
            if (Synchronous)
                manualResetEvent.Wait(cancellationToken:token.Token);
            Exception? error = null;
            ServiceMessage? response = null;
            try
            {
                var taskMessage = queryMessageFactory.ConvertMessage(logger, message)
                                        ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(Q).FullName}");
                var result = await messageRecieved(new RecievedMessage<Q>(message.ID, taskMessage,message.Header,message.RecievedTimestamp,DateTime.Now));
                response = await responseMessageFactory.ConvertMessageAsync(result.Message, message.Channel, new MessageHeader(result.Headers));
            }catch(Exception e)
            {
                errorRecieved(e);
                error=e;
            }
            if (Synchronous)
                manualResetEvent.Set();
            if (error!=null)
                return ErrorServiceMessage.Produce(message.Channel,error);
            return response??ErrorServiceMessage.Produce(message.Channel, new NullReferenceException());
        }

        protected override void InternalDispose()
            =>manualResetEvent.Dispose();
    }
}
