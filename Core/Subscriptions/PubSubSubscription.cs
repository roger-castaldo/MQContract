using Microsoft.Extensions.Logging;
using MQContract.Attributes;
using MQContract.Interfaces;
using MQContract.Interfaces.Factories;
using MQContract.Messages;
using System.Reflection;
using System.Threading.Channels;

namespace MQContract.Subscriptions
{
    internal sealed class PubSubSubscription<T>(IMessageFactory<T> messageFactory, Action<IMessage<T>> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, 
        bool synchronous=false,IServiceChannelOptions? options = null,ILogger? logger=null)
        : ISubscription
        where T : class
    {
        private IServiceSubscription? serviceSubscription;
        private readonly Channel<IServiceMessage> dataChannel = Channel.CreateUnbounded<IServiceMessage>(new UnboundedChannelOptions()
        {
            SingleReader=true,
            SingleWriter=true
        });
        private readonly string MessageChannel = channel??typeof(T).GetCustomAttribute<MessageChannelAttribute>(false)?.Name??throw new ArgumentNullException(nameof(channel));
        private readonly CancellationTokenSource token = new();
        private bool disposedValue;

        public async Task<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.Register(() => token.Cancel());
            serviceSubscription = await connection.SubscribeAsync(
                async serviceMessage=>await dataChannel.Writer.WriteAsync(serviceMessage,token.Token),
                error=>errorRecieved(error),
                MessageChannel,
                group??Guid.NewGuid().ToString(),
                options:options,
                cancellationToken:token.Token
            );
            if (serviceSubscription==null)
                return false;
            EstablishReader();
            return true;
        }

        private void EstablishReader()
        {
            Task.Run(async () =>
            {
                while (await dataChannel.Reader.WaitToReadAsync(token.Token))
                {
                    while (dataChannel.Reader.TryRead(out var message))
                    {
                        var tsk = Task.Run(() =>
                        {
                            try
                            {
                                var taskMessage = messageFactory.ConvertMessage(logger, message)
                                    ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(T).FullName}");
                                messageRecieved(new RecievedMessage<T>(message.ID,taskMessage!,message.Header));
                            }
                            catch (Exception e)
                            {
                                errorRecieved(e);
                            }
                        });
                        if (synchronous)
                            await tsk;
                    }
                }
            });
        }

        public async Task EndAsync()
        {
            if (serviceSubscription!=null)
                await serviceSubscription.EndAsync();
            await token.CancelAsync();
        }

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    token.Cancel();
                    serviceSubscription?.Dispose();
                    dataChannel.Writer.Complete();
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
