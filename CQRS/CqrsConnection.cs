using MQContract.CQRS.Consumers;
using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;
using MQContract.CQRS.Registrars;
using MQContract.Interfaces;
using MQContract.Messages;

namespace MQContract.CQRS
{
    internal sealed class CqrsConnection : ICQRSConnection,IAsyncDisposable
    {
        private readonly List<InvocationInstance> invocationInstances = new();
        private readonly SemaphoreSlim lockSlim = new(1);
        private readonly IContractConnection contractConnection;
        private readonly string? cancelationTokenChannel;
        private readonly IProcessorRegistrar processorRegistrar;
        private bool disposedValue;

        public CqrsConnection(IContractConnection contractConnection,string? cancelationTokenChannel = null)
        {
            Task? task = null;
            if (contractConnection is IContractedConnection contractedConnection)
            {
                if (!string.IsNullOrWhiteSpace(cancelationTokenChannel))
                    task = contractedConnection.RegisterPubSubAsyncConsumerAsync<CancellationRequest, CancellationRequestConsumer>(
                        new CancellationRequestConsumer(lockSlim, invocationInstances),
                        channel: cancelationTokenChannel
                    ).AsTask();
                processorRegistrar = new ContractedConnectionRegistrar(contractedConnection);
            }
            else if (contractConnection is IMappedContractConnection mappedContractConnection)
            {
                if (!string.IsNullOrWhiteSpace(cancelationTokenChannel))
                    task = mappedContractConnection.RegisterPubSubAsyncConsumerAsync<CancellationRequest, CancellationRequestConsumer>(
                        new CancellationRequestConsumer(lockSlim, invocationInstances),
                        channel: cancelationTokenChannel
                    ).AsTask();
                processorRegistrar = new MappedConnectionRegistrar(mappedContractConnection);
            }
            else
                throw new Exception();
            this.contractConnection = contractConnection;
            this.cancelationTokenChannel = cancelationTokenChannel;
            task?.Wait();
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

        async ValueTask<TCommandResult?> ICQRSConnection.ExecuteCommandAsync<TCommand, TCommandResult>(TCommand command, Context? context, TimeSpan? timeout, CancellationToken cancellationToken) where TCommandResult : default
        {
            context = SetupContext(context, cancellationToken);
            try
            {
                var result = await contractConnection.QueryAsync<TCommand, TCommandResult>(command, messageHeader: context.AsMessageHeader(), timeout: timeout, cancellationToken: cancellationToken);
                if (result.IsError)
                    throw new CommandCallException(result.Error!);
                return result.Result;
            }
            catch (TimeoutException ex)
            {
                throw new CommandTimeoutException(ex);
            }
        }

        async ValueTask<TQueryResponse?> ICQRSConnection.ExecuteQueryAsync<TQuery, TQueryResponse>(TQuery query, Context? context, TimeSpan? timeout, CancellationToken cancellationToken) where TQueryResponse : default
        {
            context = SetupContext(context, cancellationToken);
            var result = await contractConnection.QueryAsync<TQuery, TQueryResponse>(query, messageHeader: context.AsMessageHeader(), timeout: timeout, cancellationToken: cancellationToken);
            if (result.IsError)
                throw new QueryCallException(result.Error!);
            return result.Result;
        }

        private static MessageFilters<TMessage>? ExtractMessageFilters<TMessage,TProcessor>(TProcessor processor)
            where TProcessor : IProcessor
            where TMessage : ICommand
        {
            Func<MessageHeader, ValueTask<MessageFilterResult>>? headerFilter = null;
            Func<TMessage, MessageHeader, ValueTask<MessageFilterResult>>? messageFilter = null;
            if (processor is IContextFilteredProcessor contextFilteredProcessor)
                headerFilter = (header) => contextFilteredProcessor.Filter(new Context(header));
            if (processor is IFilteredCommandProcessor<TMessage> commandFilteredProcessor)
                messageFilter = (message, header) => commandFilteredProcessor.Filter(message, new Context(header));
            if (headerFilter!=null || messageFilter!=null)
                return new(headerFilter, messageFilter);
            return null;
        }

        async ValueTask<ICQRSConnection> ICQRSConnection.RegisterCommandProcessorAsync<TCommand>(ICommandProcessor<TCommand> processor, string? group)
        {
            await processorRegistrar.RegisterCommandProcessorAsync<TCommand>(
                    new CommandConsumer<TCommand>(processor,this),
                    group:group,
                    messageFilters: CqrsConnection.ExtractMessageFilters<TCommand, ICommandProcessor<TCommand>>(processor)
                );
            return this;
        }

        async ValueTask<ICQRSConnection> ICQRSConnection.RegisterCommandProcessorAsync<TCommand, TCommandResult>(ICommandProcessor<TCommand, TCommandResult> processor, string? group)
        {
            await processorRegistrar.RegisterCommandProcessorAsync<TCommand, TCommandResult>(
                new CommandResponseConsumer<TCommand, TCommandResult>(processor,this),
                group:group,
                messageFilters: CqrsConnection.ExtractMessageFilters<TCommand, ICommandProcessor<TCommand, TCommandResult>>(processor)
            );
            return this;
        }

        async ValueTask<ICQRSConnection> ICQRSConnection.RegisterQueryProcessorAsync<TQuery, TQueryResponse>(IQueryProcessor<TQuery, TQueryResponse> processor, string? group)
        {
            MessageFilters<TQuery>? messageFilters = null;
            Func<MessageHeader, ValueTask<MessageFilterResult>>? headerFilter = null;
            Func<TQuery, MessageHeader, ValueTask<MessageFilterResult>>? messageFilter = null;
            if (processor is IContextFilteredProcessor contextFilteredProcessor)
                headerFilter = (header) => contextFilteredProcessor.Filter(new Context(header));
            if (processor is IFilteredQueryProcessor<TQuery,TQueryResponse> queryFilteredProcessor)
                messageFilter = (message, header) => queryFilteredProcessor.Filter(message, new Context(header));
            if (headerFilter!=null || messageFilter!=null)
                messageFilters = new(headerFilter, messageFilter);
            await processorRegistrar.RegisterQueryProcessorAsync<TQuery, TQueryResponse>(
                new QueryResponseConsumer<TQuery, TQueryResponse>(processor, this),
                group: group,
                messageFilters: messageFilters
            );
            return this;
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (!disposedValue)
            {
                disposedValue=true;
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
