using MQContract.CQRS.Contexts;
using MQContract.CQRS.Interfaces.Command;
using MQContract.Interfaces;
using MQContract.Interfaces.Consumers;

namespace MQContract.CQRS.Consumers
{
    internal class CommandConsumer<TCommand>(ICommandProcessor<TCommand> commandProcessor, CqrsConnection connection) : IPubSubAsyncConsumer<TCommand>
        where TCommand : ICommand
    {
        void IBaseConsumer.ErrorRecieved(Exception error)
            => commandProcessor.ErrorRecieved(error);

        async ValueTask IPubSubAsyncConsumer<TCommand>.MessageReceivedAsync(IReceivedMessage<TCommand> message)
        {
            await using var context = new CommandInvocationContext<TCommand>(message, connection);
            await commandProcessor.ProcessCommandAsync(context,context.CancellationTokenSource.Token);
        }
    }
}
