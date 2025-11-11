using MQContract.CQRS.Consumers;
using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;
using MQContract.Interfaces;

namespace MQContract.CQRS
{
    internal sealed class CqrsConnection : ICQRSConnection,IAsyncDisposable
    {
        private sealed record InvocationInstance(Guid MessageId,Guid CorrelationId, Guid? CausationId, CancellationTokenSource CancellationTokenSource)
        {
            public bool IsMatch(CancellationRequest request)
                => Equals(CorrelationId, request.CorrelationId)
                && (Equals(MessageId, request.MessageId) || Equals(CausationId, request.MessageId));
        }

        private readonly List<InvocationInstance> invocationInstances = new();
        private readonly SemaphoreSlim lockSlim = new(1);
        private readonly IContractConnection contractConnection;
        private readonly ISubscription? cancelationTokenSubscription;
        private readonly string? cancelationTokenChannel;
        private bool disposedValue;

        public CqrsConnection(IContractConnection contractConnection,string? cancelationTokenChannel = null)
        {
            this.contractConnection = contractConnection;
            this.cancelationTokenChannel = cancelationTokenChannel;
            if (!string.IsNullOrWhiteSpace(cancelationTokenChannel)) {
                var task = contractConnection.SubscribeAsync<CancellationRequest>(
                    async (message) =>
                    {
                        await lockSlim.WaitAsync();
                        foreach (var instance in invocationInstances.Where(inst => inst.IsMatch(message.Message)).ToArray())
                        {
                            try
                            {
                                if (!instance.CancellationTokenSource.IsCancellationRequested)
                                    instance.CancellationTokenSource.Cancel();
                            }
                            catch {
                                //no exception catch needed
                            }
                            invocationInstances.Remove(instance);
                        }
                        lockSlim.Release();
                    },
                    (error) => { },
                    channel: cancelationTokenChannel
                ).AsTask();
                task.Wait();
                cancelationTokenSubscription = task.Result;
            }else
                cancelationTokenSubscription = null;
        }

        public CancellationTokenSource RegisterInvocation(Context context)
        {
            var result = new CancellationTokenSource();
            if (!string.IsNullOrWhiteSpace(cancelationTokenChannel))
            {
                result.Token.Register(() => TransmitCancellation(context));
                lockSlim.Wait();
                invocationInstances.Add(new(context.MessageId, context.CorrelationId, context.CausationId, result));
                lockSlim.Release();
            }
            return result;
        }

        public void UnregisterInvocation(Context context)
        {
            if (string.IsNullOrWhiteSpace(cancelationTokenChannel))
                return;
            lockSlim.Wait();
            invocationInstances.RemoveAll(inst => Equals(inst.MessageId, context.MessageId) 
                && Equals(inst.CorrelationId, context.CorrelationId)
                && Equals(inst.CausationId,context.CausationId)
            );
            lockSlim.Release();
        }

        private void TransmitCancellation(Context context)
        {
            if (!string.IsNullOrEmpty(cancelationTokenChannel))
                _ = contractConnection.PublishAsync<CancellationRequest>(new(context.CorrelationId, context.MessageId), channel: cancelationTokenChannel);
        }

        private Context SetupContext(Context? context, CancellationToken cancellationToken)
        {
            context??=new();
            cancellationToken.Register(() => TransmitCancellation(context));
            return context;
        }

        async ValueTask ICQRSConnection.ExecuteCommandAsync<TCommand>(TCommand command, Context? context, CancellationToken cancellationToken)
        {
            context = SetupContext(context, cancellationToken);
            var result = await contractConnection.PublishAsync<TCommand>(command, messageHeader: context.AsMessageHeader(), cancellationToken: cancellationToken);
            if (result.IsError)
                throw new CommandCallException(result.Error!);
        }

        async ValueTask<TCommandResult?> ICQRSConnection.ExecuteCommandAsync<TCommand, TCommandResult>(TCommand command, Context? context, CancellationToken cancellationToken) where TCommandResult : default
        {
            context = SetupContext(context, cancellationToken);
            var result = await contractConnection.QueryAsync<TCommand,TCommandResult>(command, messageHeader: context.AsMessageHeader(), cancellationToken: cancellationToken);
            if (result.IsError)
                throw new CommandCallException(result.Error!);
            return result.Result;
        }

        async ValueTask<TQueryResponse?> ICQRSConnection.ExecuteQueryAsync<TQuery, TQueryResponse>(TQuery query, Context? context, CancellationToken cancellationToken) where TQueryResponse : default
        {
            context = SetupContext(context, cancellationToken);
            var result = await contractConnection.QueryAsync<TQuery, TQueryResponse>(query, messageHeader: context.AsMessageHeader(), cancellationToken: cancellationToken);
            if (result.IsError)
                throw new CommandCallException(result.Error!);
            return result.Result;
        }

        async ValueTask<ICQRSConnection> ICQRSConnection.RegisterCommandProcessorAsync<TCommand>(ICommandProcessor<TCommand> processor, string? group)
        {
            _ = await ((IConsumerContractConnection<IBaseContractConnection>)contractConnection).RegisterPubSubAsyncConsumerAsync<TCommand,CommandConsumer<TCommand>>(
                new CommandConsumer<TCommand>(processor,this),
                group:group
            );
            return this;
        }

        async ValueTask<ICQRSConnection> ICQRSConnection.RegisterCommandProcessorAsync<TCommand, TCommandResult>(ICommandProcessor<TCommand, TCommandResult> processor, string? group)
        {
            _ = await ((IConsumerContractConnection<IBaseContractConnection>)contractConnection).RegisterQueryResponseAsyncConsumerAsync<TCommand, TCommandResult, CommandResponseConsumer<TCommand, TCommandResult>>(
                new CommandResponseConsumer<TCommand, TCommandResult>(processor,this),
                group:group
            );
            return this;
        }

        async ValueTask<ICQRSConnection> ICQRSConnection.RegisterQueryProcessorAsync<TQuery, TQueryResponse>(IQueryProcessor<TQuery, TQueryResponse> processor, string? group)
        {
            _ = await((IConsumerContractConnection<IBaseContractConnection>)contractConnection).RegisterQueryResponseAsyncConsumerAsync<TQuery, TQueryResponse, QueryResponseConsumer<TQuery, TQueryResponse>>(
                new QueryResponseConsumer<TQuery, TQueryResponse>(processor, this),
                group: group
            );
            return this;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
                await (cancelationTokenSubscription?.EndAsync()??ValueTask.CompletedTask);
                await (cancelationTokenSubscription?.DisposeAsync()??ValueTask.CompletedTask);
                await lockSlim.WaitAsync();
                invocationInstances.Clear();
                lockSlim.Release();
                lockSlim.Dispose();
                await contractConnection.DisposeAsync();
            }
            GC.SuppressFinalize(this);
        }
    }
}
