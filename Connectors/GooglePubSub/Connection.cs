using Google;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using MQContract.Interfaces.Service;
using MQContract.Messages;

namespace MQContract.GooglePubSub
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using GooglePubSub
    /// </summary>
    /// <param name="projectId">The project id to connect to through the PubSub Connections</param>
    /// <param name="publisherClientApi">Used for building publishers</param>
    /// <param name="subscriberClientApi">Used for building subscribers</param>
    public sealed class Connection(string projectId, PublisherServiceApiClient publisherClientApi, SubscriberServiceApiClient subscriberClientApi) :
        IMessageServiceConnection, IAsyncDisposable
    {
        private const string MessageTypeID = "_MessageTypeID";
        private bool disposedValue;
        private readonly SemaphoreSlim builderLock = new(1, 1);

        uint? IMessageServiceConnection.MaxMessageBodySize => 10*1024*1024; // 10MB limt according to current google definition

        ValueTask IMessageServiceConnection.CloseAsync()
            => ValueTask.CompletedTask;

        private static PubsubMessage ConvertMessage(ServiceMessage message)
        {
            var result = new PubsubMessage()
            {
                Data = ByteString.CopyFrom(message.Data.ToArray()),
                MessageId=message.ID
            };
            result.Attributes.Add(MessageTypeID, message.MessageTypeID);
            foreach (var key in message.Header.Keys)
                result.Attributes.Add(key, message.Header[key]!);
            return result;
        }

        internal static ReceivedServiceMessage ConvertMessage(ReceivedMessage message, string channel, Func<ValueTask> acknowledge)
            => new(
                message.Message.MessageId,
                message.Message.Attributes[MessageTypeID],
                channel,
                new(message.Message.Attributes.Where(pair => !Equals(pair.Key, MessageTypeID))
                    .Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value))
                ),
                message.Message.Data.ToArray(),
                acknowledge
            );

        private PublishRequest ProduceRequest(IEnumerable<PubsubMessage> message, string channel)
        {
            var result = new PublishRequest()
            {
                TopicAsTopicName = TopicName.FromProjectTopic(projectId, channel)
            };
            result.Messages.AddRange(message);
            return result;
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            await builderLock.WaitAsync(cancellationToken);
            try
            {
                _ = await publisherClientApi.PublishAsync(ProduceRequest([ConvertMessage(message)], message.Channel), cancellationToken);
            }
            catch (Exception ex)
            {
                return new TransmissionResult(message.ID, Error: new(ex, ex switch
                {
                    RpcException => ((RpcException)ex).StatusCode switch
                    {
                        StatusCode.Aborted => true,
                        StatusCode.AlreadyExists => true,
                        StatusCode.Cancelled => true,
                        StatusCode.DataLoss => true,
                        StatusCode.DeadlineExceeded => false,
                        StatusCode.FailedPrecondition => true,
                        StatusCode.Internal => true,
                        StatusCode.InvalidArgument => true,
                        StatusCode.NotFound => true,
                        StatusCode.OutOfRange => true,
                        StatusCode.PermissionDenied => true,
                        StatusCode.ResourceExhausted => false,
                        StatusCode.Unauthenticated => true,
                        StatusCode.Unavailable => false,
                        StatusCode.Unimplemented => true,
                        _ => true
                    },
                    GoogleApiException => true,
                    _ => false
                }));
            }
            finally
            {
                builderLock.Release();
            }
            return new(message.ID);
        }

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            await builderLock.WaitAsync(cancellationToken);
            IServiceSubscription? result;
            try
            {
                var subscriptionName = new SubscriptionName(projectId, group??channel);
                var topicName = new TopicName(projectId, channel);
                var createSubscription = false;
                try
                {
                    createSubscription = (await subscriberClientApi.GetSubscriptionAsync(subscriptionName))==null;
                }
                catch
                {
                    createSubscription=true;
                }
                if (createSubscription)
                    await subscriberClientApi.CreateSubscriptionAsync(subscriptionName, topicName, new() { }, 60);
                result = new Subscription(
                    subscriberClientApi,
                    subscriptionName,
                    messageReceived,
                    errorReceived,
                    channel
                );
                ((Subscription)result).Start();
            }
            finally
            {
                builderLock.Release();
            }
            return result;
        }

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                builderLock.Dispose();
            }
            return ValueTask.CompletedTask;
        }
    }
}
