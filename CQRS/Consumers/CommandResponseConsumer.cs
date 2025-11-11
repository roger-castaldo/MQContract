using MQContract.CQRS.Contexts;
using MQContract.CQRS.Interfaces.Command;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;
using MQContract.Messages;

namespace MQContract.CQRS.Consumers
{
    internal class CommandResponseConsumer<TCommand, TCommandResult>(ICommandProcessor<TCommand, TCommandResult> commandProcessor, CqrsConnection connection)
        : IQueryResponseAsyncConsumer<TCommand, TCommandResult>
        where TCommand : ICommand<TCommandResult>
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
            => commandProcessor.ErrorRecieved(error);

        async ValueTask<QueryResponseMessage<TCommandResult>> IQueryResponseAsyncConsumer<TCommand, TCommandResult>.MessageReceivedAsync(IReceivedMessage<TCommand> message)
        {
            await using var context = new CommandInvocationContext<TCommand>(message, connection);
            var result = await commandProcessor.ProcessCommandAsync(context, context.CancellationTokenSource.Token);
            return new(result, context.Headers);
        }
    }
}
