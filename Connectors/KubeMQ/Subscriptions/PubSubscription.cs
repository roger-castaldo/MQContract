using Grpc.Core;
using Microsoft.Extensions.Logging;
using MQContract.KubeMQ.Messages;
using MQContract.KubeMQ.Options;
using MQContract.KubeMQ.SDK.Connection;
using MQContract.KubeMQ.SDK.Grpc;
using MQContract.Messages;
using static MQContract.KubeMQ.SDK.Grpc.Subscribe.Types;

namespace MQContract.KubeMQ.Subscriptions
{
    internal class PubSubscription(ConnectionOptions options, KubeClient client,
        Action<IRecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group,
        StoredEventsSubscriptionOptions? storageOptions, CancellationToken cancellationToken) :
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

        protected override Task MessageRecieved(EventReceive message)
        {
            messageRecieved(new RecievedMessage(message));
            return Task.CompletedTask;
        }
    }
}
