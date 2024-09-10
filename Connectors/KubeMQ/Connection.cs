﻿using Google.Protobuf;
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
    public sealed class Connection : IQueryableMessageServiceConnection,IPingableMessageServiceConnection, IDisposable,IAsyncDisposable
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

        /// <summary>
        /// The maximum message body size allowed
        /// </summary>
        public uint? MaxMessageBodySize => (uint)Math.Abs(connectionOptions.MaxBodySize);

        /// <summary>
        /// The default timeout to use for RPC calls when not specified by the class or in the call.
        /// DEFAULT:30 seconds if not specified inside the connection options
        /// </summary>
        public TimeSpan DefaultTimout => TimeSpan.FromMilliseconds(connectionOptions.DefaultRPCTimeout??30000);

        private KubeClient EstablishConnection()
        { 
            var result =  new KubeClient(client.Address, connectionOptions.SSLCredentials??ChannelCredentials.Insecure, connectionOptions.MaxBodySize+4096, connectionOptions.Logger);
            if (result.Ping()==null)
                throw new UnableToConnectException();
            return result;
        }
        
        /// <summary>
        /// Called to ping the KubeMQ service
        /// </summary>
        /// <returns>The Ping result, specically a PingResponse instance</returns>
        /// <exception cref="UnableToConnectException">Thrown when the Ping fails</exception>
        public ValueTask<MQContract.Messages.PingResult> PingAsync()
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

        /// <summary>
        /// Called to publish a message into the KubeMQ server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>Transmition result identifying if it worked or not</returns>
        public async ValueTask<TransmissionResult> PublishAsync(ServiceMessage message, CancellationToken cancellationToken = default)
        {
            try { 
                var res = await client.SendEventAsync(new Event()
                {
                    Body=ByteString.CopyFrom(message.Data.ToArray()),
                    Metadata=message.MessageTypeID,
                    Channel=message.Channel,
                    ClientID=connectionOptions.ClientId,
                    EventID=message.ID,
                    Store=storedChannelOptions.Any(sco=>Equals(message.Channel,sco.ChannelName)),
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

        /// <summary>
        /// Called to publish a query into the KubeMQ server
        /// </summary>
        /// <param name="message">The service message being sent</param>
        /// <param name="timeout">The timeout supplied for the query to response</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The resulting response</returns>
        /// <exception cref="NullResponseException">Thrown when the response from KubeMQ is null</exception>
        /// <exception cref="RPCErrorException">Thrown when there is an RPC exception from the KubeMQ server</exception>
        public async ValueTask<ServiceQueryResult> QueryAsync(ServiceMessage message, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
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
        }

        /// <summary>
        /// Called to create a subscription to the underlying KubeMQ server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a message is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group"></param>
        /// <returns>A subscription instance</returns>
        /// <param name="cancellationToken">A cancellation token</param>
        public ValueTask<IServiceSubscription?> SubscribeAsync(Action<RecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string? group = null, CancellationToken cancellationToken = default)
        {
            var sub = new PubSubscription(
                connectionOptions,
                EstablishConnection(),
                messageRecieved,
                errorRecieved,
                channel,
                group??Guid.NewGuid().ToString(),
                storedChannelOptions.Find(sco=>Equals(sco.ChannelName,channel)),
                cancellationToken
            );
            sub.Run();
            return ValueTask.FromResult<IServiceSubscription?>(sub);
        }

        /// <summary>
        /// Called to create a subscription for queries to the underlying KubeMQ server
        /// </summary>
        /// <param name="messageRecieved">Callback for when a query is recieved</param>
        /// <param name="errorRecieved">Callback for when an error occurs</param>
        /// <param name="channel">The name of the channel to bind to</param>
        /// <param name="group">The group to bind the consumer to</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A subscription instance</returns>
        public ValueTask<IServiceSubscription?> SubscribeQueryAsync(Func<RecievedServiceMessage, ValueTask<ServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string? group = null, CancellationToken cancellationToken = default)
        {
            var sub = new QuerySubscription(
                connectionOptions,
                EstablishConnection(),
                messageRecieved,
                errorRecieved,
                channel,
                group??Guid.NewGuid().ToString(),
                cancellationToken
            );
            sub.Run();
            return ValueTask.FromResult<IServiceSubscription?>(sub);
        }
        /// <summary>
        /// Called to close the underlying KubeMQ Client connection
        /// </summary>
        /// <returns></returns>
        public ValueTask CloseAsync()
            => client.DisposeAsync();

        /// <summary>
        /// Called to dispose of the object correctly and allow it to clean up it's resources
        /// </summary>
        /// <returns>A task required for disposal</returns>
        public async ValueTask DisposeAsync()
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
        /// <summary>
        /// Called to dispose of the underlying resources
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
