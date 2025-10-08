using MQContract.Interfaces.Service;
using MQContract.Messages;
using NetMQ;
using NetMQ.Sockets;

namespace MQContract.ZeroMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using ZeroMQ
    /// </summary>
    public class Connection : IPingableMessageServiceConnection, IInboxQueryableMessageServiceConnection, IAsyncDisposable
    {
        private sealed record Subscription : IServiceSubscription
        {
            public Func<(ReceivedInboxServiceMessage message,string? responseAddress),ValueTask> Action { get; private init; }
            public Guid ID { get; private init; }
            private readonly Action<Guid> removeSubscription;

            public Subscription(Func<(ReceivedInboxServiceMessage message, string? responseAddress), ValueTask> action, Action<Guid> removeSubscription)
            {
                Action=action;
                ID = Guid.NewGuid();
                this.removeSubscription = removeSubscription;
            }

            ValueTask IServiceSubscription.EndAsync()
            {
                removeSubscription(ID);
                return ValueTask.CompletedTask;
            }
        }

        private const string INBOX_CHANNEL = "_INBOX";

        private readonly PublisherSocket publishConnection = new();
        private readonly SubscriberSocket subscriberConnection = new();
        private readonly Dictionary<string, IEnumerable<Subscription>> subscriptions = new();
        private readonly NetMQPoller poller = new();
        private readonly SemaphoreSlim locker = new(1);
        private readonly ReaderWriterLockSlim subLocker = new();
        private string? inboxAddress = null;
        private bool disposedValue;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Connection()
        {
            subscriberConnection.SubscribeToAnyTopic();
        }

        private void SetupPoller()
        {
            if (!poller.IsRunning)
            {
                subscriberConnection.ReceiveReady += async (s, e) =>
                {
                    var bytes = e.Socket.ReceiveFrameBytes();
                    var mappedMessage = MessageMapper.Map(bytes);
                    IEnumerable<Subscription>? subs = null;
                    subLocker.EnterReadLock();
                    subscriptions.TryGetValue(mappedMessage.recievedMessage.Channel, out subs);
                    subLocker.ExitReadLock();
                    if (subs!=null)
                        await Task.WhenAll(
                            subs.Select(s=>s.Action(mappedMessage).AsTask())
                        );
                };
                poller.Add(subscriberConnection);
                poller.RunAsync();
            }
        }

        /// <summary>
        /// Called to establish a connection to a listening server
        /// </summary>
        /// <param name="address">The address string for the server to connect to</param>
        public void ConnectToServer(string address)
            => publishConnection.Connect(address);

        /// <summary>
        /// Called to establish a binding for this instance to act as a server
        /// </summary>
        /// <param name="address">The address string to bind to</param>
        public void BindAsServer(string address)
        {
            subscriberConnection.Bind(address);
            SetupPoller();
            inboxAddress ??= address;
        }

        /// <summary>
        /// Called to establish the binding for the inbox address, used for query response
        /// </summary>
        /// <param name="address">The address string to bind to</param>
        public void BindInboxAddress(string address)
        {
            inboxAddress=address;
            BindAsServer(address);
        }


        /// <summary>
        /// The maximum message body size allowed, defaults to 4MB
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 1024*1024*4;

        TimeSpan IQueryableMessageServiceConnection.DefaultTimeout => TimeSpan.FromMinutes(1);

        private Subscription RegisterSubscription(Func<(ReceivedInboxServiceMessage message, string? responseAddress), ValueTask> messageReceived, string channel)
        {
            subLocker.EnterWriteLock();
            var result = new Subscription((pars) => messageReceived(pars), (id) =>
            {
                subLocker.EnterWriteLock();
                if (subscriptions.TryGetValue(channel, out var subs))
                {
                    subs = subs.Where(s => !Equals(s.ID, id));
                    subscriptions.Remove(channel);
                    if (subs.Any())
                        subscriptions.Add(channel, subs);
                }
                subLocker.ExitWriteLock();
            });
            if (subscriptions.TryGetValue(channel, out IEnumerable<Subscription>? subs))
                subscriptions.Remove(channel);
            subscriptions.Add(channel, (subs?? []).Append(result));
            subLocker.ExitWriteLock();
            return result;
        }

        private async ValueTask<ErrorMessage?> PublishMessageAsync(byte[] frame, CancellationToken cancellationToken)
        {
            await locker.WaitAsync(cancellationToken);
            ErrorMessage? error = null;
            try
            {
                publishConnection.SendFrame(frame);
            }
            catch (Exception ex)
            {
                error = new(ex, ex switch
                {
                    ObjectDisposedException => true,
                    TerminatingException => true,
                    _ => false
                });
            }
            locker.Release();
            return error;
        }

        ValueTask IMessageServiceConnection.CloseAsync()
        {
            poller.Stop();
            publishConnection.Close();
            subscriberConnection.Close();
            return ValueTask.CompletedTask;
        }

        ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
            => ValueTask.FromResult<PingResult>(new(string.Empty, string.Empty, TimeSpan.Zero));

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => new(message.ID,await PublishMessageAsync(MessageMapper.Map(message), cancellationToken));

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            =>ValueTask.FromResult<IServiceSubscription?>(RegisterSubscription(
                (msg)=> {
                    messageReceived((ReceivedServiceMessage)msg.message);
                    return ValueTask.CompletedTask;
                },
                channel
            ));

        ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Action<ReceivedInboxServiceMessage> messageReceived, CancellationToken cancellationToken)
        {
            UndefinedInboxException.ThrowIfNullOrWhiteSpace(inboxAddress);
            return ValueTask.FromResult<IServiceSubscription>(RegisterSubscription(
                (msg) =>
                {
                    messageReceived(msg.message);
                    return ValueTask.CompletedTask;
                },
                INBOX_CHANNEL
            ));
        }

        async ValueTask<TransmissionResult> IInboxQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, Guid correlationID, CancellationToken cancellationToken)
        {
            UndefinedInboxException.ThrowIfNullOrWhiteSpace(inboxAddress);
            return new(message.ID, await PublishMessageAsync(MessageMapper.Map(message, correlationID, inboxAddress), cancellationToken));
        }

        ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => ValueTask.FromResult<IServiceSubscription?>(RegisterSubscription(
                async (msg) => {
                    var response = await messageReceived(msg.message);
                    await locker.WaitAsync();
                    publishConnection.Connect(msg.responseAddress!);
                    publishConnection.SendFrame(MessageMapper.Map(response, msg.message.CorrelationID, msg.responseAddress, INBOX_CHANNEL));
                    publishConnection.Disconnect(msg.responseAddress!);
                    locker.Release();
                },
                channel
            ));

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                await locker.WaitAsync();
                if (poller.IsRunning)
                    poller.Stop();
                if (!publishConnection.IsDisposed)
                    publishConnection.Dispose();
                if (!subscriberConnection.IsDisposed)
                    subscriberConnection.Dispose();
                locker.Release();
                locker.Dispose();
                subLocker.EnterWriteLock();
                subscriptions.Clear();
                subLocker.ExitWriteLock();
                subLocker.Dispose();
                disposedValue=true;
            }
        }
    }
}
