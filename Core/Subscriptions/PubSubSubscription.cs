using Microsoft.Extensions.Logging;
using MQContract.Interfaces;
using MQContract.Interfaces.Factories;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Threading.Channels;

namespace MQContract.Subscriptions
{
    internal sealed class PubSubSubscription<T>(IMessageFactory<T> messageFactory, Func<IMessage<T>,Task> messageRecieved, Action<Exception> errorRecieved, string? channel = null, string? group = null, 
        bool synchronous=false,IServiceChannelOptions? options = null,ILogger? logger=null)
        : SubscriptionBase<T>(channel,synchronous)
        where T : class
    {
        private readonly Channel<IRecievedServiceMessage> dataChannel = Channel.CreateUnbounded<IRecievedServiceMessage>(new UnboundedChannelOptions()
        {
            SingleReader=true,
            SingleWriter=true
        });

        public async Task<bool> EstablishSubscriptionAsync(IMessageServiceConnection connection, CancellationToken cancellationToken = new CancellationToken())
        {
            SyncToken(cancellationToken);
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
                        var tsk = Task.Run(async () =>
                        {
                            try
                            {
                                var taskMessage = messageFactory.ConvertMessage(logger, message)
                                    ??throw new InvalidCastException($"Unable to convert incoming message {message.MessageTypeID} to {typeof(T).FullName}");
                                await messageRecieved(new RecievedMessage<T>(message.ID,taskMessage!,message.Header,message.RecievedTimestamp,DateTime.Now));
                            }
                            catch (Exception e)
                            {
                                errorRecieved(e);
                            }
                        });
                        if (Synchronous)
                            await tsk;
                    }
                }
            });
        }

        protected override void InternalDispose()
            => dataChannel.Writer.Complete();
    }
}
