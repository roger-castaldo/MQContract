using Google.Protobuf;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Service;
using MQContract.KubeMQ.Messages;
using MQContract.KubeMQ.Options;
using MQContract.KubeMQ.SDK.Connection;
using MQContract.KubeMQ.SDK.Grpc;
using MQContract.KubeMQ.Subscriptions;
using MQContract.Messages;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MQContract.KubeMQ
{
    /// <summary>
    /// This is the MessageServiceConnection implementation for using KubeMQ
    /// </summary>
    public sealed class Connection : IQueryResponseMessageServiceConnection,IPingableMessageServiceConnection, IDisposable,IAsyncDisposable
    {
        /// <summary>
        /// These are the different read styles to use when subscribing to a stored Event PubSub
        /// </summary>
        public enum MessageReadStyle
        {
            /// <summary>
            /// Start from the new ones (unread ones) only
            /// </summary>
            StartNewOnly = 1,
            /// <summary>
            /// Start at the beginning
            /// </summary>
            StartFromFirst = 2,
            /// <summary>
            /// Start at the last message
            /// </summary>
            StartFromLast = 3,
            /// <summary>
            /// Start at message number X (this value is specified when creating the listener)
            /// </summary>
            StartAtSequence = 4,
            /// <summary>
            /// Start at time X (this value is specified when creating the listener)
            /// </summary>
            StartAtTime = 5,
            /// <summary>
            /// Start at Time Delte X (this value is specified when creating the listener)
            /// </summary>
            StartAtTimeDelta = 6
        };

        private static readonly Regex regURL = new("^http(s)?://(.+)$", RegexOptions.Compiled|RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

        private readonly ConnectionOptions connectionOptions;
        private readonly KubeClient client;
        private readonly List<StoredChannelOptions> storedChannelOptions = [];
        private bool disposedValue;

        /// <summary>
        /// Primary constructor to create an instance using the supplied configuration options
        /// </summary>
        /// <param name="options">The configuration options to use</param>
        /// <exception cref="UnableToConnectException">Thrown when the initial attempt to connect fails</exception>
        public Connection(ConnectionOptions options)
        {
            this.connectionOptions = options;
            var match = regURL.Match(options.Address);
            var addy = (match.Success, options.SSLCredentials, match.Groups) switch
            {
                (false, _, _) => $"http{(options.SSLCredentials!=null ? "s" : "")}://{options.Address}",
                (true, var cred, var grps) when cred!=null && string.IsNullOrWhiteSpace(grps[1].Value) => $"https://{grps[2].Value}",
                (true, var cred, var grps) when cred==null && !string.IsNullOrWhiteSpace(grps[1].Value) => $"http://{grps[2].Value}",
                _ => options.Address
            };
            client = new KubeClient(addy, options.SSLCredentials??ChannelCredentials.Insecure, options.MaxBodySize+4096, options.Logger);
            var watch = new Stopwatch();
            watch.Start();
            var rec = client.Ping()??throw new UnableToConnectException();
            watch.Stop();
            var pingResult = new PingResponse(rec,watch.Elapsed);
            options.Logger?.LogInformation("Established connection to [Host:{Address}, Version:{Version}, StartTime:{ServerStartTime}, UpTime:{ServerUpTime}]",
                pingResult.Host,
                pingResult.Version,
                pingResult.ServerStartTime,
                pingResult.ServerUpTime
            );
        }

        /// <summary>
        /// Called to flag a particular channel as Stored Events when publishing or subscribing
        /// </summary>
        /// <param name="channelName">The name of the channel</param>
        /// <returns>The current connection to allow for chaining</returns>
        public Connection RegisterStoredChannel(string channelName)
        {
            storedChannelOptions.Add(new(channelName));
            return this;
        }

        /// <summary>
        /// Called to flag a particular channel as Stored Events when publishing or subscribing
        /// </summary>
        /// <param name="channelName">The name of the channel</param>
        /// <param name="readStyle">Set the message reading style when subscribing</param>
        /// <returns>The current connection to allow for chaining</returns>
        public Connection RegisterStoredChannel(string channelName, MessageReadStyle readStyle)
        {
            storedChannelOptions.Add(new(channelName, readStyle));
            return this;
        }

        /// <summary>
        /// Called to flag a particular channel as Stored Events when publishing or subscribing
        /// </summary>
        /// <param name="channelName">The name of the channel</param>
        /// <param name="readStyle">Set the message reading style when subscribing</param>
        /// <param name="readOffset">Set the readoffset to use for the given reading style</param>
        /// <returns>The current connection to allow for chaining</returns>
        public Connection RegisterStoredChannel(string channelName, MessageReadStyle readStyle, long readOffset)
        {
            storedChannelOptions.Add(new(channelName, readStyle, readOffset));
            return this;
        }

        uint? IMessageServiceConnection.MaxMessageBodySize => (uint)Math.Abs(connectionOptions.MaxBodySize);

        TimeSpan IQueryableMessageServiceConnection.DefaultTimeout => TimeSpan.FromMilliseconds(connectionOptions.DefaultRPCTimeout??30000);

        private KubeClient EstablishConnection()
        { 
            var result =  new KubeClient(client.Address, connectionOptions.SSLCredentials??ChannelCredentials.Insecure, connectionOptions.MaxBodySize+4096, connectionOptions.Logger);
            if (result.Ping()==null)
                throw new UnableToConnectException();
            return result;
        }
        
        ValueTask<MQContract.Messages.PingResult> IPingableMessageServiceConnection.PingAsync()
        {
            var watch = new Stopwatch();
            watch.Start();
            var res = client.Ping()??throw new UnableToConnectException();
            watch.Stop();
            return ValueTask.FromResult<MQContract.Messages.PingResult>(new PingResponse(res,watch.Elapsed));
        }

        internal static MapField<string, string> ConvertMessageHeader(MessageHeader header)
        {
            var result = new MapField<string, string>();
            foreach(var key in header.Keys)
                result.Add(key,header[key]!);
            return result;
        }

        internal static MessageHeader ConvertMessageHeader(MapField<string, string> header)
            => new(header.AsEnumerable());

        async ValueTask<TransmissionResult> IMessageServiceConnection.PublishAsync(ServiceMessage message, CancellationToken cancellationToken)
        {
            try { 
                var res = await client.SendEventAsync(new Event()
                {
                    Body=ByteString.CopyFrom(message.Data.ToArray()),
                    Metadata=message.MessageTypeID,
                    Channel=message.Channel,
                    ClientID=connectionOptions.ClientId,
                    EventID=message.ID,
                    Store=storedChannelOptions.Exists(sco=>Equals(message.Channel,sco.ChannelName)),
                    Tags={ ConvertMessageHeader(message.Header) }
                }, connectionOptions.GrpcMetadata, cancellationToken);
                return new TransmissionResult(res.EventID, res.Error);
            }
            catch (RpcException ex)
            {
                connectionOptions.Logger?.LogError(ex,"RPC error occured on Send in send Message:{ErrorMessage}, Status: {StatusCode}", ex.Message, ex.Status);
                return new TransmissionResult(message.ID,$"Status: {ex.Status}, Message: {ex.Message}");
            }
            catch (Exception ex)
            {
                connectionOptions.Logger?.LogError(ex, "Exception occured in Send Message:{ErrorMessage}", ex.Message);
                return new TransmissionResult(message.ID,ex.Message);
            }
        }

        async ValueTask<ServiceQueryResult> IQueryResponseMessageServiceConnection.QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken)
        {
#pragma warning disable S2139 // Exceptions should be either logged or rethrown but not both
            try
            {
                var res = await client.SendRequestAsync(new Request()
                {
                    RequestID=message.ID,
                    RequestTypeData = Request.Types.RequestType.Query,
                    Timeout = (int)timeout.TotalMilliseconds,
                    ClientID = connectionOptions.ClientId,
                    Channel = message.Channel,
                    Metadata = message.MessageTypeID,
                    Body = ByteString.CopyFrom(message.Data.ToArray()),
                    Tags = { ConvertMessageHeader(message.Header) }
                }, connectionOptions.GrpcMetadata, cancellationToken);
                if (res==null)
                {
                    connectionOptions.Logger?.LogError("Transmission Result for RPC {MessageID} is null", message.ID);
                    throw new NullResponseException();
                }
                connectionOptions.Logger?.LogDebug("Transmission Result for RPC {MessageID} (IsError:{IsError},Error:{ErrorMessage})", message.ID, !string.IsNullOrEmpty(res.Error), res.Error);
                return new ServiceQueryResult(message.ID, ConvertMessageHeader(res.Tags),res.Metadata,res.Body.ToArray());
            }
            catch (RpcException ex)
            {
                connectionOptions.Logger?.LogError(ex, "RPC error occured on Send in send Message:{ErrorMessage}, Status: {StatusCode}", ex.Message, ex.Status);
                throw new RPCErrorException(ex);
            }
            catch (Exception ex)
            {
                connectionOptions.Logger?.LogError(ex, "Exception occured in Send Message:{ErrorMessage}", ex.Message);
                throw;
            }
#pragma warning restore S2139 // Exceptions should be either logged or rethrown but not both
        }

        ValueTask<IServiceSubscription?> IMessageServiceConnection.SubscribeAsync(Action<ReceivedServiceMessage> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var sub = new PubSubscription(
                connectionOptions,
                EstablishConnection(),
                messageReceived,
                errorReceived,
                channel,
                group??Guid.NewGuid().ToString(),
                storedChannelOptions.Find(sco=>Equals(sco.ChannelName,channel)),
                cancellationToken
            );
            sub.Run();
            return ValueTask.FromResult<IServiceSubscription?>(sub);
        }

        ValueTask<IServiceSubscription?> IQueryableMessageServiceConnection.SubscribeQueryAsync(Func<ReceivedServiceMessage, ValueTask<ServiceMessage>> messageReceived, Action<Exception> errorReceived, string channel, string? group, CancellationToken cancellationToken)
        {
            var sub = new QuerySubscription(
                connectionOptions,
                EstablishConnection(),
                messageReceived,
                errorReceived,
                channel,
                group??Guid.NewGuid().ToString(),
                cancellationToken
            );
            sub.Run();
            return ValueTask.FromResult<IServiceSubscription?>(sub);
        }
        
        ValueTask IMessageServiceConnection.CloseAsync()
            => client.DisposeAsync();

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await client.DisposeAsync();

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                    client.DisposeAsync().AsTask().Wait();
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
