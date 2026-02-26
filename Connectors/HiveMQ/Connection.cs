using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.HiveMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using HiveMQ
    /// </summary>
    public sealed class Connection : IInboxQueryableMessageServiceConnection, IPingableMessageServiceConnection, IAsyncDisposable
    {
        private readonly record struct MessageInstance(string ID, MQTT5PublishMessage Message);
        private readonly HiveMQClientOptions clientOptions;
        private readonly Guid connectionID = Guid.NewGuid();
        private long lastPingTimestamp = long.MinValue;
        private TimeSpan lastPingDuration = TimeSpan.MaxValue;
        private readonly BatchedMessageStream<MessageInstance> batchedMessageStream;

        /// <summary>
        /// Houses the underlying HiveMQ client that is being used by the connection
        /// </summary>
        public HiveMQClient Client { get; private init; }

        /// <summary>
        /// Default constructor that requires the HiveMQ client options settings to be provided
        /// </summary>
        /// <param name="clientOptions">The required client options to connect to the HiveMQ instance</param>
        public Connection(HiveMQClientOptions clientOptions)
        {
            this.clientOptions = clientOptions;
            Client = new(clientOptions);
            var connectTask = Client.ConnectAsync();
            connectTask.Wait();
            if (connectTask.Result.ReasonCode!=HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                throw new ConnectionFailedException(connectTask.Result.ReasonString);
            Client.OnPingReqSent += (obj, e) =>
            {
                lastPingTimestamp = Stopwatch.GetTimestamp();
            };
            Client.OnPingRespReceived += (obj, e) =>
            {
                lastPingDuration = Stopwatch.GetElapsedTime(lastPingTimestamp);
            };
            batchedMessageStream = new(
                async (serviceMessage, cancellationToken) => new(serviceMessage.ID, ConvertMessage(serviceMessage)),
                async (message, cancellationToken) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return ProduceFromException(message.ID, new OperationCanceledException("Transmission cancelled"));
                    try
                    {
                        _ = await Client.PublishAsync(message.Message, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        return ProduceFromException(message.ID, e);
                    }
                    return new(message.ID);
                }
            );
        }

        uint? IMessageServiceConnection.MaxMessageBodySize => (uint?)clientOptions.ClientMaximumPacketSize;

        /// <summary>
        /// The default timeout to allow for a Query Response call to execute, defaults to 1 minute
        /// </summary>
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromMinutes(1);

        async ValueTask IMessageServiceConnection.CloseAsync()
        {
            await batchedMessageStream.DisposeAsync();
            await Client.DisconnectAsync();
        }

        private const string MessageID = "_ID";
        private const string MessageTypeID = "_MessageTypeID";
        private const string ResponseID = "_MessageResponseID";

        private static MQTT5PublishMessage ConvertMessage(ServiceMessage message, string? responseTopic = null, Guid? responseID = null, string? respondToTopic = null)
            => new()
            {
                Topic=respondToTopic??message.Channel,
                QoS=QualityOfService.AtLeastOnceDelivery,
                Payload=message.Data.ToArray(),
                ResponseTopic=responseTopic,
                UserProperties=new Dictionary<string, string>(
                    message.Header.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value))
                    .Concat([
                        new(MessageID,message.ID),
                        new(MessageTypeID,message.MessageTypeID)
                    ])
                    .Concat(responseID!=null ? [new(ResponseID, responseID.Value.ToString())] : [])
                )
            };

        private static ReceivedServiceMessage ConvertMessage(MQTT5PublishMessage message, out string? responseID)
        {
            message.UserProperties.TryGetValue(ResponseID, out responseID);
            return new(
                message.UserProperties[MessageID],
                message.UserProperties[MessageTypeID],
                message.Topic!,
                new(message.UserProperties.AsEnumerable()
                    .Where(pair => !Equals(pair.Key, MessageID)&&!Equals(pair.Key, MessageTypeID)&&!Equals(pair.Key, ResponseID))
                    .Select(pair=>new KeyValuePair<string, string?>(pair.Key, pair.Value))
                ),
                message.Payload
            );
        }

        private static TransmissionResult ProduceFromException(string messageID, Exception e)
            => new(messageID, Error: new(e));

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(message, cancellationToken);
        ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(messages, cancellationToken);

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var result = new Subscription(
                clientOptions,
                async (msg) =>
                {
                    try
                    {
                        await messageReceived(ConvertMessage(msg, out _));
                    }
                    catch (Exception e)
                    {
                        errorReceived(e);
                    }
                }
                , channel, group);
            await result.EstablishAsync();
            return result;
        }

        private string InboxChannel => $"_inbox/{connectionID}";

        async ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Func<ReceivedInboxServiceMessage, ValueTask> messageReceived, CancellationToken cancellationToken)
        {
            var result = new Subscription(
                clientOptions,
                async (msg) =>
                {
                    var incomingMessage = ConvertMessage(msg, out var responseID);
                    if (responseID!=null && Guid.TryParse(responseID, out var responseGuid))
                    {
                        await messageReceived(new(
                            incomingMessage.ID,
                            incomingMessage.MessageTypeID,
                            InboxChannel,
                            incomingMessage.Header,
                            responseGuid,
                            incomingMessage.Data,
                            incomingMessage.Acknowledge
                        ));
                    }
                },
                InboxChannel,
                null
            );
            await result.EstablishAsync();
            return result;
        }

        async ValueTask<TransmissionResult> IInboxQueryableMessageServiceConnection.QueryAsync(ServiceMessage message, Guid correlationID, CancellationToken cancellationToken)
        {
            try
            {
                _ = await Client.PublishAsync(ConvertMessage(message, responseTopic: InboxChannel, responseID: correlationID), cancellationToken);
            }
            catch (Exception e)
            {
                return ProduceFromException(message.ID, e);
            }
            return new(message.ID);
        }

        async ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage?>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var result = new Subscription(
                clientOptions,
                async (msg) =>
                {
                    try
                    {
                        var result = await messageReceived(ConvertMessage(msg, out var responseID));
                        if (result!=null)
                            _ = await Client.PublishAsync(ConvertMessage(result!, responseID: new Guid(responseID!), respondToTopic: msg.ResponseTopic), cancellationToken);
                    }
                    catch (Exception e)
                    {
                        errorReceived(e);
                    }
                },
                channel,
                group
            );
            await result.EstablishAsync();
            return result;
        }

        ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
            => ValueTask.FromResult<PingResult>(new(clientOptions.Host, string.Empty, lastPingDuration));

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await batchedMessageStream.DisposeAsync();
            ((IDisposable)Client).Dispose();
        }
    }
}
