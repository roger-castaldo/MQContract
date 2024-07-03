using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Factories;
using MQContract.Messages;
using System.Reflection;

namespace MQContract.Subscriptions
{
    internal sealed class QueryResponseSubscription<Q,R>(IMessageFactory<Q> queryMessageFactory,IMessageFactory<R> responseMessageFactory, Func<IMessage<Q>, Task<QueryResponseMessage<R>>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, 
        bool synchronous=false,IServiceChannelOptions? options = null,ILogger? logger=null)
        : ISubscription
        where Q : class
        where R : class
    {
        private IServiceSubscription? serviceSubscription;
        private readonly string MessageChannel = channel??typeof(Q).GetCustomAttribute<MessageChannelAttribute>(false)?.Name??throw new ArgumentNullException(nameof(channel));
        private readonly CancellationTokenSource token = new();
        private readonly ManualResetEventSlim manualResetEvent = new(true);
        private bool disposedValue;

        public async Task<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.Register(() => token.Cancel());
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

        private async Task<IServiceMessage> ProcessServiceMessageAsync(IServiceMessage message)
        {
            if (synchronous)
                manualResetEvent.Wait(cancellationToken:token.Token);
            Exception? error = null;
            IServiceMessage? response = null;
            try
            {
                var taskMessage = queryMessageFactory.ConvertMessage(logger, message)
                                        ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(Q).FullName}");
                var result = await messageRecieved(new RecievedMessage<Q>(message.ID, taskMessage,message.Header));
                response = responseMessageFactory.ConvertMessage(result.Message, null, new MessageHeader(newData:result.Headers));
            }catch(Exception e)
            {
                errorRecieved(e);
                error=e;
            }
            if (synchronous)
                manualResetEvent.Set();
            if (error!=null)
                throw error;
            return response??throw new NullReferenceException();
        }

        public async Task EndAsync()
        {
            if (serviceSubscription!=null)
                await serviceSubscription.EndAsync();
            await token.CancelAsync();
        }

#pragma warning disable CS0628 // New protected member declared in sealed type
        protected void Dispose(bool disposing)
#pragma warning restore CS0628 // New protected member declared in sealed type
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    token.Cancel();
                    serviceSubscription?.Dispose();
                    manualResetEvent.Dispose();
                    token.Dispose();
                }
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
