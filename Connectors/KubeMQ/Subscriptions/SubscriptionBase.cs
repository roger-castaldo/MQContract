using Grpc.Core;
using Microsoft.Extensions.Logging;
using MQContract.Interfaces.Service;
using MQContract.KubeMQ.SDK.Connection;

namespace MQContract.KubeMQ.Subscriptions
{
    internal abstract class SubscriptionBase<T>(ILogger? logger,int reconnectInterval, KubeClient client,
        Action<Exception> errorRecieved, CancellationToken cancellationToken) : IServiceSubscription
        where T : class
    {
        private bool disposedValue;
        private bool active = true;
        protected readonly Guid ID = Guid.NewGuid();
        protected readonly CancellationTokenSource cancelToken = new();

        protected abstract AsyncServerStreamingCall<T> EstablishCall();
        protected abstract Task MessageRecieved(T message);

        public void Run()
        {
            cancellationToken.Register(() =>
            {
                cancelToken.Cancel();
            });

            cancelToken.Token.Register(async () =>
            {
                active = false;
                await client.DisposeAsync();
            });
            Task.Run(async () =>
            {
                while (active && !cancelToken.IsCancellationRequested)
                {
                    try
                    {
                        using var call = EstablishCall();
                        logger?.LogTrace("Connection for subscription {SubscriptionID} established", ID);
                        await foreach (var resp in call.ResponseStream.ReadAllAsync(cancelToken.Token))
                        {
                            if (active)
                                await MessageRecieved(resp);
                            else
                                break;
                        }
                    }
                    catch (RpcException rpcx)
                    {
                        if (active && !cancelToken.IsCancellationRequested)
                        {
                            switch (rpcx.StatusCode)
                            {
                                case StatusCode.Cancelled:
                                case StatusCode.PermissionDenied:
                                case StatusCode.Aborted:
                                    EndAsync().Wait();
                                    break;
                                case StatusCode.Unknown:
                                case StatusCode.Unavailable:
                                case StatusCode.DataLoss:
                                case StatusCode.DeadlineExceeded:
                                    logger?.LogTrace("RPC Error recieved on subscription {SubscriptionID}, retrying connection after delay {ReconnectDelay}ms.  StatusCode:{StatusCode},Message:{ErrorMessage}", ID, reconnectInterval, rpcx.StatusCode, rpcx.Message);
                                    break;
                                default:
                                    logger?.LogError(rpcx, "RPC Error recieved on subscription {SubscriptionID}.  StatusCode:{StatusCode},Message:{ErrorMessage}", ID, rpcx.StatusCode, rpcx.Message);
                                    errorRecieved(rpcx);
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger?.LogError(e, "Error recieved on subscription {SubscriptionID}.  Message:{ErrorMessage}", ID, e.Message);
                        errorRecieved(e);
                    }
                    if (active && !cancellationToken.IsCancellationRequested)
                        await Task.Delay(reconnectInterval);
                }
            });
        }

        public async Task EndAsync()
        {
            if (active)
            {
                active = false;
                try
                {
                    await cancelToken.CancelAsync();
                    await client.DisposeAsync();
                    cancelToken.Dispose();
                }
                catch{ }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!disposedValue && active)
            {
                disposedValue=true;
                await EndAsync();
            }
        }
    }
}
