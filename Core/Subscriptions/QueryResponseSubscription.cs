using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.Subscriptions
{
    internal sealed class QueryResponseSubscription<Q,R>(IMessageFactory<Q> queryMessageFactory,IMessageFactory<R> responseMessageFactory, Func<IMessage<Q>, Task<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, 
        bool synchronous=false,IServiceChannelOptions? options = null,ILogger? logger=null)
        : SubscriptionBase<Q>(channel,synchronous),ISubscription
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

        private async Task<IServiceMessage> ProcessServiceMessageAsync(IRecievedServiceMessage message)
        {
            if (Synchronous)
                manualResetEvent.Wait(cancellationToken:token.Token);
            Exception? error = null;
            IServiceMessage? response = null;
            try
            {
                var taskMessage = queryMessageFactory.ConvertMessage(logger, message)
                                        ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(Q).FullName}");
                var result = await messageRecieved(new RecievedMessage<Q>(message.ID, taskMessage,message.Header,message.RecievedTimestamp,DateTime.Now));
                response = responseMessageFactory.ConvertMessage(result.Message, message.Channel, new MessageHeader(newData:result.Headers));
            }catch(Exception e)
            {
                errorRecieved(e);
                error=e;
            }
            if (Synchronous)
                manualResetEvent.Set();
            if (error!=null)
                return new ErrorServiceMessage(Guid.NewGuid(),string.Empty,message.Channel,new MessageHeader(),error);
            return response??new ErrorServiceMessage(Guid.NewGuid(), string.Empty, message.Channel, new MessageHeader(), new NullReferenceException());
        }

        protected override void InternalDispose()
            =>manualResetEvent.Dispose();
    }
}
