using Grpc.Core;
using Microsoft.Extensions.Logging;
using MQContract.KubeMQ.Options;
using MQContract.KubeMQ.SDK.Connection;
using MQContract.KubeMQ.SDK.Grpc;
using MQContract.Messages;
using static MQContract.KubeMQ.SDK.Grpc.Subscribe.Types;

namespace MQContract.KubeMQ.Subscriptions
{
    internal class PubSubscription(ConnectionOptions options, KubeClient client,
        Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group,
        StoredChannelOptions? storageOptions, CancellationToken cancellationToken) :
        SubscriptionBase<EventReceive>(options.Logger,options.ReconnectInterval,client,errorRecieved,cancellationToken)
    {
        private readonly KubeClient Client = client;

        protected override AsyncServerStreamingCall<EventReceive> EstablishCall()
        {
            options.Logger?.LogTrace("Attempting to establish subscription {SubscriptionID} to {Address} on channel {Channel}", ID, options.Address, channel);
            return Client.SubscribeToEvents(new Subscribe()
            {
                Channel = channel,
                ClientID = options.ClientId,
                Group = group,
                SubscribeTypeData = storageOptions == null ? SubscribeType.Events : SubscribeType.EventsStore,
                EventsStoreTypeData = (EventsStoreType?)(int?)storageOptions?.ReadStyle ?? EventsStoreType.Undefined,
                EventsStoreTypeValue = storageOptions?.ReadOffset ?? 0
            },
            options.GrpcMetadata,
            cancelToken.Token);
        }

        protected override ValueTask MessageRecieved(EventReceive message)
        {
            messageRecieved(new(message.EventID,message.Metadata,message.Channel,Connection.ConvertMessageHeader(message.Tags),message.Body.ToArray()));
            return ValueTask.CompletedTask;
        }
    }
}
