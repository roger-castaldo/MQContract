using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using MQContract.Interfaces.Service;
using MQContract.Messages;
using System.Diagnostics;

namespace MQContract.AmazonSNQS
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using Amazon SNS/SQS
    /// </summary>
    public sealed class Connection
        : IPingableMessageServiceConnection, IAsyncDisposable
    {
        /// <summary>
        /// Houses the SNSClient that was supplied to the connection, this is used for access wrt administration and other items
        /// </summary>
        public AmazonSimpleNotificationServiceClient? SNSClient { get; private init; }

        /// <summary>
        /// Houses the SQSClient that was supplied to the connection, this is used for access wrt administration and other items
        /// </summary>
        public AmazonSQSClient? SQSClient { get; private init; }

        private readonly BatchedMessageStream<ServiceMessage> batchedMessageStream;
        private readonly CancellationTokenSource cancelToken = new();
        private bool disposed = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <paramref name="snsClientConfiguration">The sns client configuration and credentials to establish a SNS client</paramref>
        /// <paramref name="sqsClientConfiguration">The sqs client configuration and credentials to establish a SQS client</paramref>
        public Connection((AWSCredentials credentials, AmazonSimpleNotificationServiceConfig config)? snsClientConfiguration = null,
            (AWSCredentials credentials, AmazonSQSConfig config)? sqsClientConfiguration = null)
        {
            NoClientsSetException.ThrowIfBothNull(snsClientConfiguration, sqsClientConfiguration);
            SNSClient = (snsClientConfiguration==null ? null : new(snsClientConfiguration.Value.credentials, snsClientConfiguration.Value.config));
            SQSClient = (sqsClientConfiguration==null ? null : new(sqsClientConfiguration.Value.credentials, sqsClientConfiguration.Value.config));
            batchedMessageStream = new(
                (serviceMessage, _) => ValueTask.FromResult(serviceMessage),
                async (message, cancellationToken) =>
                {
                    if (SNSClient!=null)
                    {
                        var topic = await SNSClient.FindTopicAsync(message.Channel);
                        if (topic!=null)
                        {
                            var snsResult = await SNSClient.PublishAsync(MessageMapper.Map(message, topic), cancellationToken);
                            return new(snsResult.SequenceNumber??message.ID);
                        }
                    }
                    if (SQSClient!=null)
                    {
                        var queue = (await SQSClient.ListQueuesAsync(message.Channel, cancellationToken)).QueueUrls.FirstOrDefault();
                        if (queue!=null)
                        {
                            var sqsResult = await SQSClient.SendMessageAsync(MessageMapper.Map(message, queue), cancellationToken);
                            return new(sqsResult.MessageId??message.ID);
                        }
                    }
                    return new(message.ID, Error: new(new NoChannelFoundException(message.Channel), true));
                }
            );
        }


        /// <summary>
        /// The maximum message body size allowed, defaults to 256Kb
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 256*1024;

        async ValueTask IMessageServiceConnection.CloseAsync()
        {
            if (!cancelToken.IsCancellationRequested)
            {
                await batchedMessageStream.DisposeAsync();
                await cancelToken.CancelAsync();
                SNSClient?.Dispose();
                SQSClient?.Dispose();
            }
        }

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            var start = Stopwatch.GetTimestamp();
            try
            {
                if (SNSClient!=null)
                {
                    _ = await SNSClient.FindTopicAsync("ping");
                    return new((string.IsNullOrWhiteSpace(SNSClient.Config.ServiceURL) ? SNSClient.Config.RegionEndpoint.DisplayName : SNSClient.Config.ServiceURL),
                        SNSClient.Config.ServiceVersion,
                        Stopwatch.GetElapsedTime(start)
                    );
                }
                else
                {
                    _ = await SQSClient!.GetQueueUrlAsync("ping");
                    return new((string.IsNullOrEmpty(SQSClient.Config.ServiceURL) ? SQSClient.Config.RegionEndpoint.DisplayName : SQSClient.Config.ServiceURL),
                        SQSClient.Config.ServiceVersion,
                        Stopwatch.GetElapsedTime(start)
                    );
                }
            }
            catch
            {
                throw new PingFailedException("Unable to ping AWS services");
            }
        }

        ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(message, cancellationToken);
        ValueTask<IEnumerable<TransmissionResult>> IMessageServiceConnection.BulkPublishAsync(IEnumerable<ServiceMessage> messages, CancellationToken cancellationToken)
            => batchedMessageStream.TransmitAsync(messages, cancellationToken);

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Func<ReceivedServiceMessage, ValueTask> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            SqsClientNullException.ThrowIfNull(SQSClient);
            var queue = (await SQSClient!.ListQueuesAsync(channel, cancellationToken)).QueueUrls.FirstOrDefault();
            UnableToLocateQueueException.ThrowIfNullOrWhitespace(queue, channel);
            var result = new Subscription(SQSClient, queue!, messageReceived, errorReceived, cancelToken.Token);
            result.Start();
            return result;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposed)
            {
                disposed=true;
                await ((IMessageServiceConnection)this).CloseAsync();
                cancelToken.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
