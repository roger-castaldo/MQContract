using MQContract.CQRS.Interfaces.Command;
using MQContract.Interfaces;

namespace MQContract.CQRS.Contexts;

internal sealed class CommandInvocationContext<TCommand>(IReceivedMessage<TCommand> receivedMessage, CqrsConnection connection)
    : AInvocationContext<TCommand>(receivedMessage, connection), ICommandInvocationContext<TCommand>
    where TCommand : ICommand
{
    TCommand ICommandInvocationContext<TCommand>.Command => Message;
}
