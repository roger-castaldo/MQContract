using MQContract.CQRS.Interfaces.Command;
using Messages;

namespace CQRSSample
{
    public class CreateUserProcessor : ICommandProcessor<CreateUserCommand>
    {
        private static readonly Dictionary<string, User> _users = new();

        public async ValueTask ProcessCommandAsync(ICommandInvocationContext<CreateUserCommand> context, CancellationToken cancellationToken)
        {
            var command = context.Command;
            var user = new User(command.UserId, command.UserName, command.Email);
            _users[command.UserId] = user;
            Console.WriteLine($"User created: {user.UserName} ({user.UserId})");
        }

        public void ErrorRecieved(Exception error)
        {
            Console.WriteLine($"CreateUserProcessor error: {error.Message}");
        }
    }
}