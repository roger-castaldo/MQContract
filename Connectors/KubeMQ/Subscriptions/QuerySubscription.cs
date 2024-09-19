using Grpc.Core;
using Microsoft.Extensions.Logging;
using MQContract.KubeMQ.SDK.Connection;
using MQContract.KubeMQ.SDK.Grpc;
using MQContract.Messages;

namespace MQContract.KubeMQ.Subscriptions
{
    internal class QuerySubscription(ConnectionOptions options, KubeClient client, 
        Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, 
        string channel, string group, CancellationToken cancellationToken)
        : SubscriptionBase<Request>(options.Logger,options.ReconnectInterval,client,errorReceived,cancellationToken)
    {
        private readonly KubeClient Client = client;
        protected override AsyncServerStreamingCall<Request> EstablishCall()
        {
            options.Logger?.LogTrace("Attempting to establish subscription {SubscriptionID} to {Address} on channel {Channel}", ID, options.Address, channel);
            return Client.SubscribeToRequests(new Subscribe()
            {
                Channel = channel,
                ClientID = options.ClientId,
                Group = group,
                SubscribeTypeData = Subscribe.Types.SubscribeType.Queries
            },
            options.GrpcMetadata,
            cancelToken.Token);
        }

        protected override async ValueTask MessageReceived(Request message)
        {
            ServiceMessage? result;
            try
            {
                result = await messageReceived(new(message.RequestID,message.Metadata,message.Channel,Connection.ConvertMessageHeader(message.Tags),message.Body.ToArray()));
            }
            catch (Exception ex)
            {
                options.Logger?.LogError(ex, "Message {MessageID} failed on subscription {SubscriptionID}.  Message:{ErrorMessage}", message.RequestID, ID, ex.Message);
                await Client.SendResponseAsync(new Response()
                {
                    RequestID=message.RequestID,
                    ClientID=message.ClientID,
                    Executed=true,
                    Error=ex.Message,
                    ReplyChannel=message.ReplyChannel,
                    Body=Google.Protobuf.ByteString.Empty,
                    Timestamp=Utility.ToUnixTime(DateTime.Now)
                },
                options.GrpcMetadata,
                cancelToken.Token);
                result=null;
            }
            if (result!=null)
            {
                options.Logger?.LogTrace("Response generated for {MessageID} on RPC subscription {SubscriptionID}", message.RequestID, ID);
                try
                {
                    await Client.SendResponseAsync(new Response()
                    {
                        CacheHit=false,
                        RequestID= message.RequestID,
                        ClientID=message.ClientID,
                        Executed=true,
                        Error=string.Empty,
                        ReplyChannel=message.ReplyChannel,
                        Body=Google.Protobuf.ByteString.CopyFrom(result.Data.ToArray()),
                        Metadata=result.MessageTypeID,
                        Tags = { Connection.ConvertMessageHeader(result.Header) },
                        Timestamp=Utility.ToUnixTime(DateTime.Now)
                    },
                    options.GrpcMetadata,
                    cancelToken.Token);
                }
                catch (Exception e)
                {
                    options.Logger?.LogError(e, "Response for {MessageID} on RPC subscription {SubscriptionID} failed to send. Exception: {ErrorMessage}", message.RequestID, ID, e.Message);
                    throw new MessageResponseTransmissionException(ID, message.RequestID, e);
                }
            }
        }
    }
}
