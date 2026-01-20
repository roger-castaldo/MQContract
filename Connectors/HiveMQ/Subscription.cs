using HiveMQtt.Client;
using HiveMQtt.Client.Options;
using HiveMQtt.MQTT5.Types;
using MQContract.Interfaces.Service;

namespace MQContract.HiveMQ
{
    internal class Subscription(HiveMQClientOptions clientOptions, Func<MQTT5PublishMessage, ValueTask> messageReceived, string channel, string? group) : IServiceSubscription, IAsyncDisposable
    {
        private readonly HiveMQClient client = new(CloneOptions(clientOptions, channel));
        private bool isOpen = false;

        private static HiveMQClientOptions CloneOptions(HiveMQClientOptions clientOptions, string channel)
        {
            var result = new HiveMQClientOptions();
            foreach (var prop in typeof(HiveMQClientOptions).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => !Equals(p.Name, nameof(clientOptions.ClientId))))
                prop.SetValue(result, prop.GetValue(clientOptions, []));
            result.ClientId=$"{clientOptions.ClientId}.{channel}.{Guid.NewGuid()}";
            return result;
        }

        private string Topic => $"{(group==null ? "" : $"$share/{group}/")}{channel}";

        public async ValueTask EstablishAsync()
        {
            client.OnMessageReceived += async (sender, args)
                => await messageReceived(args.PublishMessage).ConfigureAwait(false);
            var connectResult = await client.ConnectAsync();
            if (connectResult.ReasonCode != HiveMQtt.MQTT5.ReasonCodes.ConnAckReasonCode.Success)
                throw new ConnectionFailedException(connectResult.ReasonString);
            isOpen = true;
            _ = await client.SubscribeAsync(Topic, HiveMQtt.MQTT5.Types.QualityOfService.AtLeastOnceDelivery);
        }

        async ValueTask IServiceSubscription.EndAsync()
        {
            if (isOpen)
            {
                isOpen = false;
                await client.UnsubscribeAsync(Topic);
                await client.DisconnectAsync();
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await ((IServiceSubscription)this).EndAsync();
            client.Dispose();
        }
    }
}
