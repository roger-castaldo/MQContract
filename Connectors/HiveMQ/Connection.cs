using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.HiveMQ
{
    public class Connection : IMessageServiceConnection,IDisposable
    {
        private readonly HiveMQClientOptions clientOptions;
        private readonly HiveMQClient client;
        private bool disposedValue;

        public Connection(HiveMQClientOptions clientOptions)
        {
            this.clientOptions = clientOptions;
            client = new(clientOptions);
            var connectTask = client.ConnectAsync();
            connectTask.Wait();
            if (connectTask.Result.ReasonCode!=HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                throw new Exception($"Failed to connect: {connectTask.Result.ReasonString}");
        }

        uint? IMessageServiceConnection.MaxMessageBodySize => (uint?)clientOptions.ClientMaximumPacketSize;

        async ValueTask IMessageServiceConnection.CloseAsync()
            =>await client.DisconnectAsync();

        private const string MessageID = "_ID";
        private const string MessageTypeID = "_MessageTypeID";

        internal static MQTT5PublishMessage ConvertMessage(ServiceMessage message)
            => new()
            {
                Topic=message.Channel,
                QoS=QualityOfService.AtLeastOnceDelivery,
                Payload=message.Data.ToArray(),
                UserProperties=new Dictionary<string, string>(
                    message.Header.Keys
                    .Select(k=>new KeyValuePair<string, string>(k,message.Header[k]!))
                    .Concat([
                        new(MessageID,message.ID),
                        new(MessageTypeID,message.MessageTypeID)
                    ])
                )
            };

        internal static ReceivedServiceMessage ConvertMessage(MQTT5PublishMessage message)
            => new(
                message.UserProperties[MessageID],
                message.UserProperties[MessageTypeID],
                message.Topic!,
                new(message.UserProperties.AsEnumerable().Where(pair => !Equals(pair.Key, MessageID)&&!Equals(pair.Key, MessageTypeID))),
                message.Payload
            );

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
            var result = new Subscription(clientOptions,messageReceived,errorReceived,channel,group);
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
