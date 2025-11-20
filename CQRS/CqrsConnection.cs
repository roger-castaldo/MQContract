using MQContract.CQRS.Consumers;
using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Command;
using MQContract.CQRS.Interfaces.Query;
using MQContract.CQRS.Registrars;
using MQContract.Interfaces;
using MQContract.Messages;
using System.Collections.Concurrent;

namespace MQContract.CQRS
{
    internal sealed class CqrsConnection : ICQRSConnection
    {
        private readonly ConcurrentDictionary<(Guid messageId,Guid correlationId, Guid? causationId),CancellationTokenSource> invocationInstances = new();
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
                        new CancellationRequestConsumer(invocationInstances),
                        channel: cancelationTokenChannel
                    ).AsTask();
                processorRegistrar = new ContractedConnectionRegistrar(contractedConnection);
            }
            else if (contractConnection is IMappedContractConnection mappedContractConnection)
            {
                if (!string.IsNullOrWhiteSpace(cancelationTokenChannel))
                    task = mappedContractConnection.RegisterPubSubAsyncConsumerAsync<CancellationRequest, CancellationRequestConsumer>(
                        new CancellationRequestConsumer(invocationInstances),
                        channel: cancelationTokenChannel
                    ).AsTask();
                processorRegistrar = new MappedConnectionRegistrar(mappedContractConnection);
            }
            else
                throw new InvalidConnectionException(contractConnection.GetType());
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
                invocationInstances.TryAdd((context.MessageId, context.CorrelationId, context.CausationId), result);
            }
            return result;
        }

        public void UnregisterInvocation(Context context)
        {
            if (string.IsNullOrWhiteSpace(cancelationTokenChannel))
                return;
            invocationInstances.TryRemove((context.MessageId,context.CorrelationId,context.CausationId), out _);
        }

        private void TransmitCancellation(Context context)
        {
            if (!string.IsNullOrEmpty(cancelationTokenChannel))
#pragma warning disable CA2012 // Use ValueTasks correctly
                _ = contractConnection.PublishAsync<CancellationRequest>(new(context.CorrelationId, context.MessageId), channel: cancelationTokenChannel);
#pragma warning restore CA2012 // Use ValueTasks correctly
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
                invocationInstances.Clear();
                await contractConnection.DisposeAsync();
            }
            GC.SuppressFinalize(this);
        }
    }
}
