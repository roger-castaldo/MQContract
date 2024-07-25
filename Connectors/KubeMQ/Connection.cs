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
    public class Connection : IMessageServiceConnection,IDisposable
    {
        private static readonly Regex regURL = new("^http(s)?://(.+)$", RegexOptions.Compiled|RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

        private readonly ConnectionOptions connectionOptions;
        private readonly KubeClient client;
        private readonly List<IServiceSubscription> subscriptions = [];
        private readonly SemaphoreSlim dataLock = new(1, 1);
        private bool disposedValue;

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

        public int? MaxMessageBodySize => connectionOptions.MaxBodySize;

        public TimeSpan DefaultTimout => TimeSpan.FromMilliseconds(connectionOptions.DefaultRPCTimeout??5000);

        private KubeClient EstablishConnection()
        { 
            var result =  new KubeClient(client.Address, connectionOptions.SSLCredentials??ChannelCredentials.Insecure, connectionOptions.MaxBodySize+4096, connectionOptions.Logger);
            if (result.Ping()==null)
                throw new UnableToConnectException();
            return result;
        }
        public Task<IPingResult> PingAsync()
        {
            var watch = new Stopwatch();
            watch.Start();
            var res = client.Ping()??throw new UnableToConnectException();
            watch.Stop();
            return Task.FromResult<IPingResult>(new PingResponse(res,watch.Elapsed));
        }

        public static MapField<string, string> ConvertTags(IMessageHeader header)
        {
            var result = new MapField<string, string>();
            foreach(var key in header.Keys)
                result.Add(key,header[key]!);
            return result;
        }

        public async Task<ITransmissionResult> PublishAsync(IServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null && options is not PublishChannelOptions)
                throw new InvalidChannelOptionsTypeException(typeof(PublishChannelOptions), options.GetType());
            try { 
                var res = await client.SendEventAsync(new Event()
                {
                    Body=ByteString.CopyFrom(message.Data.ToArray()),
                    Metadata=message.MessageTypeID,
                    Channel=message.Channel,
                    ClientID=connectionOptions.ClientId,
                    EventID=message.ID,
                    Store=(options is PublishChannelOptions pbc && pbc.Stored),
                    Tags={ ConvertTags(message.Header) }
                }, connectionOptions.GrpcMetadata, cancellationToken);
                return new TransmissionResult(res);
            }
            catch (RpcException ex)
            {
                connectionOptions.Logger?.LogError(ex,"RPC error occured on Send in send Message:{ErrorMessage}, Status: {StatusCode}", ex.Message, ex.Status);
                return new TransmissionResult(error: $"Status: {ex.Status}, Message: {ex.Message}");
            }
            catch (Exception ex)
            {
                connectionOptions.Logger?.LogError(ex, "Exception occured in Send Message:{ErrorMessage}", ex.Message);
                return new TransmissionResult(error: ex.Message);
            }
        }

        public async Task<IServiceQueryResult> QueryAsync(IServiceMessage message, TimeSpan timeout, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null)
                throw new ArgumentOutOfRangeException(nameof(options),"There are no service channel options available for this action");
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
                    Tags = { ConvertTags(message.Header) }
                }, connectionOptions.GrpcMetadata, cancellationToken);
                if (res==null)
                {
                    connectionOptions.Logger?.LogError("Transmission Result for RPC {MessageID} is null", message.ID);
                    return new QueryResult(error: "null response recieved from KubeMQ server");
                }
                connectionOptions.Logger?.LogDebug("Transmission Result for RPC {MessageID} (IsError:{IsError},Error:{ErrorMessage})", message.ID, !string.IsNullOrEmpty(res.Error), res.Error);
                return new QueryResult(message.ID,res);
            }
            catch (RpcException ex)
            {
                connectionOptions.Logger?.LogError(ex, "RPC error occured on Send in send Message:{ErrorMessage}, Status: {StatusCode}", ex.Message, ex.Status);
                return new QueryResult(error: $"Status: {ex.Status}, Message: {ex.Message}");
            }
            catch (Exception ex)
            {
                connectionOptions.Logger?.LogError(ex, "Exception occured in Send Message:{ErrorMessage}", ex.Message);
                return new QueryResult(error: ex.Message);
            }
        }

        public async Task<IServiceSubscription?> SubscribeAsync(Action<IRecievedServiceMessage> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null && options is not StoredEventsSubscriptionOptions)
                throw new InvalidChannelOptionsTypeException(typeof(StoredEventsSubscriptionOptions), options.GetType());
            var sub = new PubSubscription(
                connectionOptions,
                EstablishConnection(),
                messageRecieved,
                errorRecieved,
                channel,
                group,
                (StoredEventsSubscriptionOptions?)options,
                cancellationToken
            );
            sub.Run();
            await dataLock.WaitAsync(cancellationToken);
            subscriptions.Add(sub);
            dataLock.Release();
            return sub;
        }

        public async Task<IServiceSubscription?> SubscribeQueryAsync(Func<IRecievedServiceMessage, Task<IServiceMessage>> messageRecieved, Action<Exception> errorRecieved, string channel, string group, IServiceChannelOptions? options = null, CancellationToken cancellationToken = default)
        {
            if (options!=null)
                throw new ArgumentOutOfRangeException(nameof(options), "There are no service channel options available for this action");
            var sub = new QuerySubscription(
                connectionOptions,
                EstablishConnection(),
                messageRecieved,
                errorRecieved,
                channel,
                group,
                cancellationToken
            );
            sub.Run();
            await dataLock.WaitAsync(cancellationToken);
            subscriptions.Add(sub);
            dataLock.Release();
            return sub;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    dataLock.Wait();
                    foreach (var sub in subscriptions)
                        sub.EndAsync().Wait();
                    subscriptions.Clear();
                    client.Dispose();
                    dataLock.Release();
                    dataLock.Dispose();
                }
                disposedValue=true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
