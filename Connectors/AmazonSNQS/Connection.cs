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
    public class Connection(AmazonSimpleNotificationServiceClient? snsClient = null, AmazonSQSClient? sqsClient = null)
        : IPingableMessageServiceConnection, IAsyncDisposable
    {
        /// <summary>
        /// Houses the SNSClient that was supplied to the connection, this is used for access wrt administration and other items
        /// </summary>
        public AmazonSimpleNotificationServiceClient? SNSClient => snsClient;

        /// <summary>
        /// Houses the SQSClient that was supplied to the connection, this is used for access wrt administration and other items
        /// </summary>
        public AmazonSQSClient? SQSClient => sqsClient;

        private readonly CancellationTokenSource cancelToken = new();
        private bool disposed = false;

        /// <summary>
        /// The maximum message body size allowed, defaults to 256Kb
        /// </summary>
        public uint? MaxMessageBodySize { get; init; } = 256*1024;

        async ValueTask IMessageServiceConnection.CloseAsync()
        {
            if (!cancelToken.IsCancellationRequested)
            {
                await cancelToken.CancelAsync();
                snsClient?.Dispose();
                sqsClient?.Dispose();
            }
        }

        async ValueTask<PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            var start = Stopwatch.GetTimestamp();
            try
            {
                if (snsClient!=null)
                {
                    _ = await snsClient.FindTopicAsync("ping");
                    return new((string.IsNullOrWhiteSpace(snsClient.Config.ServiceURL) ? snsClient.Config.RegionEndpoint.DisplayName : snsClient.Config.ServiceURL),
                        snsClient.Config.ServiceVersion,
                        Stopwatch.GetElapsedTime(start)
                    );
                }
                else
                {
                    _ = await sqsClient!.GetQueueUrlAsync("ping");
                    return new((string.IsNullOrEmpty(sqsClient.Config.ServiceURL) ? sqsClient.Config.RegionEndpoint.DisplayName : sqsClient.Config.ServiceURL),
                        sqsClient.Config.ServiceVersion,
                        Stopwatch.GetElapsedTime(start)
                    );
                }
            }
            catch
            {
                throw new PingFailedException("Unable to ping AWS services");
            }
        }

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            NoClientsSetException.ThrowIfBothNull(snsClient, sqsClient);
            if (snsClient!=null)
            {
                var topic = await snsClient.FindTopicAsync(message.Channel);
                if (topic!=null)
                {
                    var snsResult = await snsClient.PublishAsync(MessageMapper.Map(message, topic), cancellationToken);
                    return new(snsResult.SequenceNumber??message.ID);
                }
            }
            if (sqsClient!=null)
            {
                var queue = (await sqsClient.ListQueuesAsync(message.Channel, cancellationToken)).QueueUrls.FirstOrDefault();
                if (queue!=null)
                {
                    var sqsResult = await sqsClient.SendMessageAsync(MessageMapper.Map(message, queue), cancellationToken);
                    return new(sqsResult.MessageId??message.ID);
                }
            }
            throw new TransmissionException(new NoChannelFoundException(message.Channel),true);
        }

        async ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            SqsClientNullException.ThrowIfNull(sqsClient);
            var queue = (await sqsClient!.ListQueuesAsync(channel, cancellationToken)).QueueUrls.FirstOrDefault();
            UnableToLocateQueueException.ThrowIfNullOrWhitespace(queue, channel);
            var result = new Subscription(sqsClient, queue!, messageReceived, errorReceived, cancelToken.Token);
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
