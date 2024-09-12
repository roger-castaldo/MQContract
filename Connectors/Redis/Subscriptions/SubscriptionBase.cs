using MQContract.Interfaces.Service;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQContract.Redis.Subscriptions
{
    internal abstract class SubscriptionBase(Action<Exception> errorRecieved, IDatabase database, Guid connectionID, string channel, string? group) : IServiceSubscription,IDisposable
    {
        private readonly CancellationTokenSource tokenSource = new();
        private bool disposedValue;

        protected CancellationToken Token=>tokenSource.Token;
        protected IDatabase Database => database;
        protected string Channel => channel;
        protected string? Group=> group;

        public Task StartAsync()
        {
            var resultSource = new TaskCompletionSource();
            RedisValue minId = "-";
            Task.Run(async () =>
            {
                resultSource.TrySetResult();
                while (!Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await (group==null ? database.StreamRangeAsync(channel, minId, "+", 1) : database.StreamReadGroupAsync(channel, group!, connectionID.ToString(), ">", 1));
                        if (result.Length!=0)
                        {
                            minId = result[0].Id+1;
                            await ProcessMessage(result[0], channel, group);
                        }
                        else
                            await Task.Delay(50);
                    }
                    catch (Exception ex)
                    {
                        errorRecieved(ex);
                    }
                }
            });
            return resultSource.Task;
        }

        protected async ValueTask Acknowledge(RedisValue Id)
        {
            if (Group!=null)
                await Database.StreamAcknowledgeAsync(channel, group, Id);
        }

        protected abstract ValueTask ProcessMessage(StreamEntry streamEntry,string channel,string? group);

        public async ValueTask EndAsync()
            =>await tokenSource.CancelAsync();

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && !tokenSource.IsCancellationRequested)
                    tokenSource.Cancel();
                tokenSource.Dispose();
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
