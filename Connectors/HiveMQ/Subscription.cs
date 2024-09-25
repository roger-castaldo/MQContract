using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.Client.Results;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.HiveMQ
{
    internal class Subscription(HiveMQClientOptions clientOptions, Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, 
        string channel, string? group) : IServiceSubscription,IDisposable
    {
        private readonly HiveMQClient client = new(CloneOptions(clientOptions,channel));

        private static HiveMQClientOptions CloneOptions(HiveMQClientOptions clientOptions,string channel)
        {
            var result = new HiveMQClientOptions();
            foreach (var prop in typeof(HiveMQClientOptions).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => !Equals(p.Name, nameof(clientOptions.ClientId))))
                prop.SetValue(result, prop.GetValue(clientOptions, []));
            result.ClientId=$"{clientOptions.ClientId}.{channel}.{Guid.NewGuid()}";
            return result;
        }

        private bool disposedValue;

        private string Topic => $"{(group==null ? "" : $"$share/{group}/")}{channel}";

        public async ValueTask EstablishAsync()
        {
            client.OnMessageReceived += (sender, args) =>
            {
                try
                {
                    messageReceived(Connection.ConvertMessage(args.PublishMessage));
                }catch(Exception e)
                {
                    errorReceived(e);
                }
            };
            var connectResult = await client.ConnectAsync();
            if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                throw new Exception($"Failed to connect: {connectResult.ReasonString}");
            _ = await client.SubscribeAsync(Topic, HiveMQtt.MQTT5.Types.QualityOfService.AtLeastOnceDelivery);
        }

        async ValueTask IServiceSubscription.EndAsync()
        {
            await client.UnsubscribeAsync(Topic);
            await client.DisconnectAsync();
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
