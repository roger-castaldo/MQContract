using Google;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Grpc.Core;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.GooglePubSub
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using GooglePubSub
    /// </summary>
    public sealed class Connection :
        IPingableMessageServiceConnection, IAsyncDisposable
    {
        private readonly record struct MessageInstance(string ID, PublishRequest Request);
        private const string MessageTypeID = "_MessageTypeID";

        private readonly BatchedMessageStream<MessageInstance> batchedMessageStream;

        /// <summary>
        /// Houses the project id that was supplied in the constructor
        /// </summary>
        public string ProjectId { get; private init; }
        /// <summary>
        /// Houses the Publisher Service API Client used in the underlying service
        /// </summary>
        public PublisherServiceApiClient PublisherServiceApi { get; private init; }
        /// <summary>
        /// Houses the Subscriber Service API Client used in the underlying service
        /// </summary>
        public SubscriberServiceApiClient SubscriberServiceApi { get; private init; }

        /// <summary>
        /// Default constructor for creating instance
        /// </summary>
        /// <param name="projectId">The project id to connect to through the PubSub Connections</param>
        /// <param name="publisherServiceBuilder">Used for building publishers</param>
        /// <param name="subscriberServiceBuilder">Used for building subscribers</param>
        public Connection(string projectId, PublisherServiceApiClientBuilder publisherServiceBuilder, SubscriberServiceApiClientBuilder subscriberServiceBuilder)
        {
            ProjectId = projectId;
            PublisherServiceApi = publisherServiceBuilder.Build();
            SubscriberServiceApi = subscriberServiceBuilder.Build();
            batchedMessageStream = new(
                async (serviceMessage, _) =>
                {
                    var message = new PubsubMessage()
                    {
                        Data = ByteString.CopyFrom(serviceMessage.Data.ToArray()),
                        MessageId=serviceMessage.ID
                    };
                    message.Attributes.Add(MessageTypeID, serviceMessage.MessageTypeID);
                    serviceMessage.Header.ForEach(pair => message.Attributes.Add(pair.Key, pair.Value));
                    var result = new PublishRequest()
                    {
                        TopicAsTopicName = TopicName.FromProjectTopic(projectId, serviceMessage.Channel)
                    };
                    result.Messages.Add(message);
                    return new(serviceMessage.ID, result);
                },
                async (message, cancellationToken) =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                            return new TransmissionResult(message.ID, Error: new(new OperationCanceledException("Transmission cancelled"), true));
                        _ = await PublisherServiceApi.PublishAsync(message.Request, cancellationToken);
                        return new TransmissionResult(message.ID);
                    }
                    catch (RpcException rpc)
                    {
                        return new TransmissionResult(message.ID, Error: new(rpc, rpc.StatusCode switch
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
                        }));
                    }
                    catch (GoogleApiException google)
                    {
                        return new(message.ID, Error: new(google, true));
                    }
                    catch (Exception ex)
                    {
                        return new(message.ID, Error: new(ex, false));
                    }
                }
            );
        }

        uint? IMessageServiceConnection.MaxMessageBodySize => 10*1024*1024; // 10MB limt according to current google definition

        ValueTask IMessageServiceConnection.CloseAsync()
            => batchedMessageStream.DisposeAsync();

        internal static ReceivedServiceMessage ConvertMessage(ReceivedMessage message, string channel, Func<ValueTask> acknowledge)
            => new(
                message.Message.MessageId,
                message.Message.Attributes[MessageTypeID],
                channel,
                new(message.Message.Attributes.Where(pair => !Equals(pair.Key, MessageTypeID))
                    .Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value))
                ),
                message.Message.Data.ToArray(),
                acknowledge
            );

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(message, cancellationToken);
        ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(messages, cancellationToken);

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            IServiceSubscription? result;
            var subscriptionName = new SubscriptionName(ProjectId, group??channel);
            var topicName = new TopicName(ProjectId, channel);
            var createSubscription = false;
            try
            {
                createSubscription = (await SubscriberServiceApi.GetSubscriptionAsync(subscriptionName))==null;
            }
            catch
            {
                createSubscription=true;
            }
            if (createSubscription)
                await SubscriberServiceApi.CreateSubscriptionAsync(subscriptionName, topicName, new() { }, 60);
            result = new Subscription(
                SubscriberServiceApi,
                subscriptionName,
                messageReceived,
                errorReceived,
                channel
            );
            ((Subscription)result).Start();
            return result;
        }

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            var start = Stopwatch.GetTimestamp();
            try
            {
                await PublisherServiceApi.GetTopicAsync(TopicName.FromProjectTopic(ProjectId, "ping"));
                return new(ProjectId, string.Empty, Stopwatch.GetElapsedTime(start));
            }
            catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                // Means the connection worked but resource doesn’t exist — still a good "ping"
                return new(ProjectId, string.Empty, Stopwatch.GetElapsedTime(start));
            }
            catch
            {
                throw new PingFailedException("Unable to make a call against the Google PubSub services");
            }
        }

        ValueTask IAsyncDisposable.DisposeAsync()
            => batchedMessageStream.DisposeAsync();
    }
}
