using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.HiveMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using HiveMQ
    /// </summary>
    public class Connection : IInboxQueryableMessageServiceConnection, IDisposable
    {
        private readonly HiveMQClientOptions clientOptions;
        private readonly HiveMQClient client;
        private readonly Guid connectionID = Guid.NewGuid();
        private bool disposedValue;

        /// <summary>
        /// Default constructor that requires the HiveMQ client options settings to be provided
        /// </summary>
        /// <param name="clientOptions">The required client options to connect to the HiveMQ instance</param>
        public Connection(HiveMQClientOptions clientOptions)
        {
            this.clientOptions = clientOptions;
            client = new(clientOptions);
            var connectTask = client.ConnectAsync();
            connectTask.Wait();
            if (connectTask.Result.ReasonCode!=HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                throw new ConnectionFailedException(connectTask.Result.ReasonString);
        }

        uint? IMessageServiceConnection.MaxMessageBodySize => (uint?)clientOptions.ClientMaximumPacketSize;

        /// <summary>
        /// The default timeout to allow for a Query Response call to execute, defaults to 1 minute
        /// </summary>
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromMinutes(1);

        async ValueTask IMessageServiceConnection.CloseAsync()
            => await client.DisconnectAsync();

        private const string MessageID = "_ID";
        private const string MessageTypeID = "_MessageTypeID";
        private const string ResponseID = "_MessageResponseID";

        private static MQTT5PublishMessage ConvertMessage(ServiceMessage message,string? responseTopic=null,Guid? responseID=null,string? respondToTopic=null)
            => new()
            {
                Topic=respondToTopic??message.Channel,
                QoS=QualityOfService.AtLeastOnceDelivery,
                Payload=message.Data.ToArray(),
                ResponseTopic=responseTopic,
                UserProperties=new Dictionary<string, string>(
                    message.Header.Keys
                    .Select(k=>new KeyValuePair<string, string>(k,message.Header[k]!))
                    .Concat([
                        new(MessageID,message.ID),
                        new(MessageTypeID,message.MessageTypeID)
                    ])
                    .Concat(responseID!=null ?[new(ResponseID,responseID.Value.ToString())] : [])
                )
            };

        private static ReceivedServiceMessage ConvertMessage(MQTT5PublishMessage message, out string? responseID)
        { 
            message.UserProperties.TryGetValue(ResponseID, out responseID);
            return new(
                message.UserProperties[MessageID],
                message.UserProperties[MessageTypeID],
                message.Topic!,
                new(message.UserProperties.AsEnumerable().Where(pair => !Equals(pair.Key, MessageID)&&!Equals(pair.Key, MessageTypeID)&&!Equals(pair.Key,ResponseID))),
                message.Payload
            );
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            try
            {
                _ = await client.PublishAsync(ConvertMessage(message), cancellationToken);
            }catch(Exception e)
            {
                return new(message.ID, e.Message);
            }
            return new(message.ID);
        }

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var result = new Subscription(
                clientOptions,
                (msg) =>
                {
                    try
                    {
                        messageReceived(ConvertMessage(msg,out _));
                    }catch(Exception e)
                    {
                        errorReceived(e);
                    }
                }
                ,channel,group);
            await result.EstablishAsync();
            return result;
        }

        private string InboxChannel => $"_inbox/{connectionID}";

        async ValueTask<IServiceSubscription> IInboxQueryableMessageServiceConnection.EstablishInboxSubscriptionAsync(Action<ReceivedInboxServiceMessage> messageReceived, CancellationToken cancellationToken)
        {
            var result = new Subscription(
                clientOptions,
                (msg) =>
                {
                    var incomingMessage = ConvertMessage(msg, out var responseID);
                    if (responseID!=null && Guid.TryParse(responseID, out var responseGuid))
                    {
                        messageReceived(new(
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
                _ = await client.PublishAsync(ConvertMessage(message,responseTopic:InboxChannel,responseID:correlationID), cancellationToken);
            }
            catch (Exception e)
            {
                return new(message.ID, e.Message);
            }
            return new(message.ID);
        }

        async ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var result = new Subscription(
                clientOptions,
                async (msg) =>
                {
                    try
                    {
                        var result = await messageReceived(ConvertMessage(msg, out var responseID));
                        _ = await client.PublishAsync(ConvertMessage(result, responseID: new Guid(responseID!), respondToTopic: msg.ResponseTopic), cancellationToken);
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

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Dispose();
                }
                disposedValue=true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
