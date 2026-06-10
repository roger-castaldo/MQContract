using MQContract.Interfaces.Service;
using MQContract.Messages;
using NetMQ;
using NetMQ.Monitoring;
using NetMQ.Sockets;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MQContract.ZeroMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using ZeroMQ
    /// </summary>
    public sealed class Connection : IPingableMessageServiceConnection, IInboxQueryableMessageServiceConnection, IAsyncDisposable
    {
        private readonly record struct MessageInstance(string ID, byte[] Frame);

        private static readonly byte[] PingMessage = System.Text.UTF8Encoding.UTF8.GetBytes("PING");
        private static readonly byte[] PongMessage = System.Text.UTF8Encoding.UTF8.GetBytes("PONG");
        private static readonly TimeSpan PongTimeout = TimeSpan.FromMinutes(1);

        private sealed record Subscription : IServiceSubscription
        {
            public Func<(ReceivedInboxServiceMessage message, string? responseAddress), ValueTask> Action { get; private init; }
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
        private readonly ConcurrentDictionary<string, IEnumerable<Subscription>> subscriptions = [];
        private readonly NetMQPoller poller = new();
        private string? inboxAddress = null;
        private readonly List<string> servers = [];
        private readonly ManualResetEventSlim pingResponseEvent = new(false);
        private readonly BatchedMessageStream<MessageInstance> batchedMessageStream;
        private bool disposedValue;

        private static void SendMessageToDestination(byte[] message, string address)
        {
            var connection = new PublisherSocket();
            var connectionMonitor = new NetMQMonitor(connection, $"inproc://{address[(address.IndexOf("//")+2)..]}", SocketEvents.Connected);
            connectionMonitor.Connected += (s, e) =>
            {
                Thread.Sleep(5);
                connection.SendFrame(message);
                connection.Disconnect(address);
                connection.Close();
                connectionMonitor.Stop();
            };
            connectionMonitor.StartAsync();
            connection.Connect(address);
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Connection()
        {
            subscriberConnection.SubscribeToAnyTopic();
            batchedMessageStream = new(
                async (serviceMessage, _) => new(serviceMessage.ID, MessageMapper.Map(serviceMessage)),
                async (message, cancellationToken) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return new TransmissionResult(message.ID, new(new OperationCanceledException("Transmission cancelled"), true));
                    return new TransmissionResult(message.ID, await PublishMessageAsync(message.Frame));
                }
            );
        }

        private void SetupPoller()
        {
            if (!poller.IsRunning)
            {
                subscriberConnection.ReceiveReady += async (s, e) =>
                {
                    var bytes = e.Socket.ReceiveFrameBytes();
                    if (PongMessage.SequenceEqual(bytes))
                        pingResponseEvent.Set();
                    else if (bytes.Length>PingMessage.Length && PingMessage.SequenceEqual(bytes.Take(PingMessage.Length)))
                        SendMessageToDestination(PongMessage, System.Text.UTF8Encoding.UTF8.GetString([.. bytes.Skip(PingMessage.Length)]));
                    else
                    {
                        var mappedMessage = MessageMapper.Map(bytes);
                        subscriptions.TryGetValue(mappedMessage.recievedMessage.Channel, out IEnumerable<Subscription>? subs);
                        if (subs!=null)
                            await Task.WhenAll(
                                subs.Select(s => s.Action(mappedMessage).AsTask())
                            );
                    }
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
        {
            publishConnection.Connect(address);
            servers.Add(address);
        }

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
            var result = new Subscription((pars) => messageReceived(pars), (id) =>
            {
                if (subscriptions.TryGetValue(channel, out var subs))
                {
                    var newSubs = subs.Where(s => !Equals(s.ID, id)).ToArray();
                    if (newSubs.Any())
                        subscriptions.TryUpdate(channel, newSubs, subs);
                    else
                        subscriptions.TryRemove(channel, out _);
                }
            });
            if (subscriptions.TryGetValue(channel, out IEnumerable<Subscription>? subs))
                subscriptions.TryUpdate(channel, subs.Append(result), subs);
            else
                subscriptions.TryAdd(channel, [result]);
            return result;
        }

        private async ValueTask<ErrorMessage?> PublishMessageAsync(byte[] frame)
        {
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
            return error;
        }

        async ValueTask IMessageServiceConnection.CloseAsync()
        {
            await batchedMessageStream.DisposeAsync().ConfigureAwait(true);
            poller.Stop();
            publishConnection.Close();
            subscriberConnection.Close();
        }

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            if (servers.Count>0)
            {
                UndefinedInboxException.ThrowIfNullOrWhiteSpace(inboxAddress);
                var start = Stopwatch.GetTimestamp();
                pingResponseEvent.Reset();
                publishConnection.SendFrame([.. PingMessage, .. System.Text.UTF8Encoding.UTF8.GetBytes(inboxAddress!)]);
                try
                {
                    if (pingResponseEvent.Wait(PongTimeout))
                        return new(string.Join(',', servers), typeof(NetMQPoller).Assembly.GetName().Version?.ToString()??string.Empty, Stopwatch.GetElapsedTime(start));
                }
                finally
                {
                    pingResponseEvent.Reset();
                }
            }
            else if (poller.IsRunning)
                return new("self", typeof(NetMQPoller).Assembly.GetName().Version?.ToString()??string.Empty, TimeSpan.Zero);
            throw new PingFailedException("Unable to ping due to lack of connections and no subscribers");
        }

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(message, cancellationToken);

        ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(messages, cancellationToken);

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => ValueTask.FromResult<IServiceSubscription?>(RegisterSubscription(
                async (msg) => await messageReceived((ReceivedServiceMessage)msg.message).ConfigureAwait(false),
                channel
            ));

        ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Func<ReceivedInboxServiceMessage, ValueTask> messageReceived, CancellationToken cancellationToken)
        {
            UndefinedInboxException.ThrowIfNullOrWhiteSpace(inboxAddress);
            return ValueTask.FromResult<IServiceSubscription>(RegisterSubscription(
                (msg) => messageReceived(msg.message),
                INBOX_CHANNEL
            ));
        }

        async ValueTask<TransmissionResult> IInboxQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, Guid correlationID, CancellationToken cancellationToken)
        {
            UndefinedInboxException.ThrowIfNullOrWhiteSpace(inboxAddress);
            return new(message.ID, await PublishMessageAsync(MessageMapper.Map(message, correlationID, inboxAddress)));
        }

        ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
            => ValueTask.FromResult<IServiceSubscription?>(RegisterSubscription(
                async (msg) =>
                {
                    var response = await messageReceived(msg.message);
                    if (response!=null)
                        SendMessageToDestination(MessageMapper.Map(response, msg.message.CorrelationID, msg.responseAddress, INBOX_CHANNEL), msg.responseAddress!);
                },
                channel
            ));

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await batchedMessageStream.DisposeAsync().ConfigureAwait(true);
                if (poller.IsRunning)
                    poller.Stop();
                if (!publishConnection.IsDisposed)
                    publishConnection.Dispose();
                if (!subscriberConnection.IsDisposed)
                    subscriberConnection.Dispose();
                subscriptions.Clear();
            }
            GC.SuppressFinalize(this);
        }
    }
}
