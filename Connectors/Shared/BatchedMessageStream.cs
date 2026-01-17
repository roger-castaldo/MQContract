using MQContract.Messages;
using System.Threading.Channels;

namespace MQContract;

internal class BatchedMessageStream<TServiceMessage> : IAsyncDisposable
{
    private class MessageBatch<TMessage>(IEnumerable<TMessage> items, CancellationToken cancellationToken)
    {
        private readonly TaskCompletionSource<IEnumerable<TransmissionResult>> completionSource = new();

        public IEnumerable<TMessage> Items => items;
        public CancellationToken CancellationToken => cancellationToken;
        public Task<IEnumerable<TransmissionResult>> Result => completionSource.Task;

        internal void ProcessResults(IEnumerable<Task<TransmissionResult>> results)
        {
            var completionCall = completionSource;
            _ = Task.Run(async () => completionCall.TrySetResult(await Task.WhenAll(results)));
        }
    }

    private readonly Channel<MessageBatch<TServiceMessage>> channel = Channel.CreateBounded<MessageBatch<TServiceMessage>>(new BoundedChannelOptions(10)
    {
        SingleReader=true,
        SingleWriter=false,
        FullMode=BoundedChannelFullMode.Wait
    });
    private readonly CancellationTokenSource cancelToken = new();
    private readonly Func<ServiceMessage, CancellationToken, ValueTask<TServiceMessage>> convert;
    private readonly Func<TServiceMessage, CancellationToken, Task<TransmissionResult>> transmit;

    public BatchedMessageStream(Func<ServiceMessage, CancellationToken, ValueTask<TServiceMessage>> convert,
    Func<TServiceMessage, CancellationToken, Task<TransmissionResult>> transmit){
        this.convert = convert;
        this.transmit = transmit;
        _ = Task.Run(async () =>
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var request = await channel.Reader.ReadAsync(cancelToken.Token);
                    var results = request.Items.Select(req => transmit(req,request.CancellationToken));
                    request.ProcessResults(results);
                }
                catch 
                {}
            }
        });
    }

    public async ValueTask<TransmissionResult> TransmitAsync(ServiceMessage serviceMessage, CancellationToken cancellationToken)
    {
        var batch = new MessageBatch<TServiceMessage>([await convert(serviceMessage,cancellationToken)], cancellationToken);
        await channel.Writer.WriteAsync(batch,cancellationToken);
        return (await batch.Result).First();
    }

    public async ValueTask<IEnumerable<TransmissionResult>> TransmitAsync(IEnumerable<ServiceMessage> serviceMessages, CancellationToken cancellationToken)
    {
        var batch = new MessageBatch<TServiceMessage>(await Task.WhenAll(serviceMessages.Select(message=>convert(message,cancellationToken).AsTask())),cancellationToken);
        await channel.Writer.WriteAsync(batch, cancellationToken);
        return await batch.Result;
    }

    public async ValueTask DisposeAsync()
    {
        channel.Writer.TryComplete();
        await cancelToken.CancelAsync();
    }
}