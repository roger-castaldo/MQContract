using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using MQContract.KubeMQ.SDK.Grpc;
using Microsoft.Extensions.Logging;

namespace MQContract.KubeMQ.SDK.Connection
{
    internal class KubeClient : IDisposable
    {
        private const int RETRY_COUNT = 5;

        private static readonly ServiceConfig defaultServiceConfig = new()
        {
            MethodConfigs={
                new(){
                    Names={ MethodName.Default },
                    RetryPolicy=new()
                    {
                        MaxAttempts=RETRY_COUNT,
                        InitialBackoff = TimeSpan.FromSeconds(1),
                        MaxBackoff = TimeSpan.FromSeconds(5),
                        BackoffMultiplier = 1.5,
                        RetryableStatusCodes = { StatusCode.Unavailable}
                    }
                }
            }
        };

        private readonly ILogger? logger;
        private readonly GrpcChannel channel;
        private readonly kubemq.kubemqClient client;
        private bool disposedValue;

        public string Address { get; private init; }

        public KubeClient(string address, ChannelCredentials credentials, int messageSize, ILogger? logger)
        {
            this.logger = logger;
            Address=address;
            channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions()
            {
                MaxReceiveMessageSize = messageSize,
                MaxSendMessageSize = messageSize,
                Credentials=credentials,
                HttpHandler = new SocketsHttpHandler
                {
                    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                    KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                    EnableMultipleHttp2Connections = true
                },
                ServiceConfig=defaultServiceConfig
            });
            client = new(channel);
        }

        private void CheckDisposed()
        {
            if (disposedValue)
                throw new ClientDisposedException();
        }

        private R TryInvoke<R>(Func<R> call)
        {
            CheckDisposed();
            Exception? err = null;
            R? result = default;
            try
            {
                result = call();
            }
            catch (RpcException ex)
            {
                err=ex;
                logger?.LogError(ex,"KubeClient RPC Error[Message:{Message},Status:{StatusCode}]", ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                err=ex;
                logger?.LogError(ex,"KubeClient Error[Message:{Message}]", ex.Message);
            }
            if (err!=null)
                throw err;
            return result!;
        }

        private async Task<R> TryInvokeAsync<R>(Func<Task<R>> call)
        {
            CheckDisposed();
            Exception? err = null;
            R? result = default;
            try
            {
                result = await call();
            }
            catch (RpcException ex)
            {
                err=ex;
                logger?.LogError(ex, "KubeClient RPC Error[Message:{Message},Status:{StatusCode}]", ex.Message, ex.StatusCode);
            }
            catch (Exception ex)
            {
                err=ex;
                logger?.LogError(ex, "KubeClient Error[Message:{Message}]", ex.Message);
            }
            if (err!=null)
                throw err;
            return result!;
        }

        internal MQContract.KubeMQ.SDK.Grpc.PingResult Ping()
            => TryInvoke<MQContract.KubeMQ.SDK.Grpc.PingResult>(() =>
            {
                return client.Ping(new Empty());
            });

        internal Task<MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult> SendQueueMessageAsync(QueueMessage queueMessage, Metadata headers, CancellationToken cancellationToken)
            => TryInvokeAsync<MQContract.KubeMQ.SDK.Grpc.SendQueueMessageResult>(async () =>
            {
                return await client.SendQueueMessageAsync(queueMessage, headers: headers, cancellationToken: cancellationToken);
            });

        internal Task<MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchResponse> SendQueueMessagesBatchAsync(QueueMessagesBatchRequest queueMessagesBatchRequest, Metadata headers, CancellationToken cancellationToken)
            => TryInvokeAsync<MQContract.KubeMQ.SDK.Grpc.QueueMessagesBatchResponse>(async () =>
            {
                return await client.SendQueueMessagesBatchAsync(queueMessagesBatchRequest, headers: headers, cancellationToken: cancellationToken);
            });

        internal MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse ReceiveQueueMessages(ReceiveQueueMessagesRequest receiveQueueMessagesRequest, Metadata headers, CancellationToken token)
            => TryInvoke<MQContract.KubeMQ.SDK.Grpc.ReceiveQueueMessagesResponse>(() =>
            {
                return client.ReceiveQueueMessages(receiveQueueMessagesRequest, headers: headers, cancellationToken: token);
            });

        internal Task<MQContract.KubeMQ.SDK.Grpc.Result> SendEventAsync(Event @event, Metadata headers, CancellationToken cancellationToken)
            => TryInvokeAsync<MQContract.KubeMQ.SDK.Grpc.Result>(async () =>
            {
                return await client.SendEventAsync(@event, headers: headers, cancellationToken: cancellationToken);
            });

        internal Task<MQContract.KubeMQ.SDK.Grpc.Response> SendRequestAsync(Request request, Metadata headers, CancellationToken cancellationToken)
            => TryInvokeAsync<MQContract.KubeMQ.SDK.Grpc.Response>(async () =>
            {
                return await client.SendRequestAsync(request, headers: headers, cancellationToken: cancellationToken);
            });

        internal AsyncServerStreamingCall<MQContract.KubeMQ.SDK.Grpc.Request> SubscribeToRequests(Subscribe subscribe, Metadata headers, CancellationToken cancellationToken)
            => TryInvoke<AsyncServerStreamingCall<MQContract.KubeMQ.SDK.Grpc.Request>>(() =>
            {
                return client.SubscribeToRequests(subscribe, headers: headers, cancellationToken: cancellationToken);
            });

        internal Task SendResponseAsync(Response response, Metadata headers, CancellationToken cancellationToken)
            => TryInvokeAsync<Empty>(async () =>
            {
                return await client.SendResponseAsync(response, headers: headers, cancellationToken: cancellationToken);
            });

        internal AsyncServerStreamingCall<EventReceive> SubscribeToEvents(Subscribe subscribe, Metadata headers, CancellationToken cancellationToken)
            => TryInvoke<AsyncServerStreamingCall<EventReceive>>(() =>
            {
                return client.SubscribeToEvents(subscribe, headers: headers, cancellationToken: cancellationToken);
            });

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        channel.ShutdownAsync().Wait();
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex,"Error shutting down grpc Kube Channel: {ErrorMessage}",ex.Message);
                    }
                    channel.Dispose();
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
