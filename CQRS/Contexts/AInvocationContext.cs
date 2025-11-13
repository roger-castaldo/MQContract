using MQContract.CQRS.Interfaces;
using MQContract.CQRS.Interfaces.Query;
using MQContract.Interfaces;
using System.Diagnostics;

namespace MQContract.CQRS.Contexts
{
    internal abstract class AInvocationContext<TMessage> : IInvocationContext, IAsyncDisposable
    {
        private readonly Context context;
        protected readonly TMessage Message;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Activity? activity;
        private readonly ICQRSConnection connection;

        protected AInvocationContext(IReceivedMessage<TMessage> receivedMessage, CqrsConnection connection){
            context = new(receivedMessage.Headers);
            Message = receivedMessage.Message;
            cancellationTokenSource = connection.RegisterInvocation(context);
            activity = receivedMessage.Activity;
            this.connection = connection;
            receivedMessage.Activity?.AddTag("mqcontract.cqrs.correlationid", context.CorrelationId);
            receivedMessage.Activity?.AddTag("mqcontract.cqrs.messageid", context.MessageId);
            receivedMessage.Activity?.AddTag("mqcontract.cqrs.causationid", context.CausationId);
            receivedMessage.Activity?.AddTag("mqcontract.cqrs.type", (receivedMessage.Message is IQuery ? "query" : "command"));
        }

        internal CancellationTokenSource CancellationTokenSource => cancellationTokenSource;

        string? IInvocationContext.this[string key] { get => context[key]; set => context[key] = value; }

        Guid IInvocationContext.MessageId => context.MessageId;

        Guid IInvocationContext.CorrelationId => context.CorrelationId;

        Guid? IInvocationContext.CausationId => context.CausationId;

        Activity? IInvocationContext.Activity => activity;

        internal Dictionary<string, string?> Headers => context.AsDictionary();

        ValueTask IInvocationContext.ExecuteCommandAsync<TCommand>(TCommand command)
            => connection.ExecuteCommandAsync<TCommand>(command, context.CloneToChild(), cancellationTokenSource.Token);

        ValueTask<TCommandResult?> IInvocationContext.ExecuteCommandAsync<TCommand, TCommandResult>(TCommand command) where TCommandResult : default
            => connection.ExecuteCommandAsync<TCommand, TCommandResult>(command, context.CloneToChild(), cancellationTokenSource.Token);

        ValueTask<TQueryResponse?> IInvocationContext.ExecuteQueryAsync<TQuery, TQueryResponse>(TQuery query) where TQueryResponse : default
            => connection.ExecuteQueryAsync<TQuery, TQueryResponse>(query, context.CloneToChild(), cancellationTokenSource.Token);

        ValueTask IAsyncDisposable.DisposeAsync()
        {
            ((CqrsConnection)connection).UnregisterInvocation(context);
            cancellationTokenSource.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
